/*using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public static class AddressableLoader
{
    // static 클래스는 코루틴을 사용할 수 없고, 다음 프레임까지 대기하는 방법이 없기 때문에 별도의 Runner 생성
    #region Runner (코루틴/프레임 대기용) 
    private sealed class Runner : MonoBehaviour { }
    private static Runner _runner;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static async void RuntimeInit()
    {
        if (_runner != null) return;
        var go = new GameObject("[Addr.Runner]");
        UnityEngine.Object.DontDestroyOnLoad(go);
        _runner = go.AddComponent<Runner>();

        // Addressables 명시 초기화(선택)
        try { await Addressables.InitializeAsync().Task; }
        catch (Exception e) { Debug.LogError($"[Addr] Initialize failed: {e}"); }
    }

    private static Task NextFrame()
    {
        var tcs = new TaskCompletionSource<bool>();
        _runner.StartCoroutine(WaitOneFrame(tcs));
        return tcs.Task;
    }
    private static System.Collections.IEnumerator WaitOneFrame(TaskCompletionSource<bool> tcs)
    {
        yield return null;
        tcs.TrySetResult(true);
    }
    #endregion

    #region 내부 상태 (핸들 캐시/참조카운트)
    private readonly struct AssetKey : IEquatable<AssetKey>
    {
        public readonly object Key;           // string 키 또는 AssetReference 등
        public readonly Type Type;          // 요청 타입(T)
        public AssetKey(object key, Type type) { Key = key; Type = type; }
        public bool Equals(AssetKey other) => Equals(Key, other.Key) && Type == other.Type;
        public override bool Equals(object obj) => obj is AssetKey ak && Equals(ak);
        public override int GetHashCode() => ((Key?.GetHashCode() ?? 0) * 397) ^ (Type?.GetHashCode() ?? 0);
        public override string ToString() => $"{Key} ({Type?.Name})";
    }

    // 에셋 로드 핸들(참조 카운트 포함)
    private static readonly Dictionary<AssetKey, (AsyncOperationHandle handle, int refCount)>
        _assetHandles = new();

    // Instantiate 핸들: 인스턴스 → 생성 핸들
    private static readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>>
        _instanceHandles = new();

    // 라벨 로드 핸들 (타입별 분리)
    private static readonly Dictionary<AssetKey, AsyncOperationHandle>
        _labelLoadHandles = new();

    // 씬 핸들: sceneKey → SceneInstance 핸들
    private static readonly Dictionary<string, AsyncOperationHandle<SceneInstance>>
        _sceneHandles = new();
    #endregion

    #region 공통 유틸
    // handle.Task로도 기다릴 수 있지만, 아래 4가지의 이유로 직접 기다리는 방식 추가
    // 1. 진행률을 매 프레임 깔끔하게 보고(PercentComplete)
    // 2. 취소(CancellationToken) 시 중간에 안전 해제(Release)까지 수행
    // 3. await NextFrame()로 항상 메인 스레드 프레임 루프에서 폴링
    // 4. UI 진행바/로딩 스피너/네트워크 중단 등의 UX를 일괄적으로 붙이기 쉬움
    private static async Task<T> AwaitWithProgress<T>(AsyncOperationHandle<T> handle, IProgress<float> progress, CancellationToken ct)
    {
        while (!handle.IsDone)
        {
            progress?.Report(handle.PercentComplete);
            if (ct.IsCancellationRequested)
            {
                // 안전 정리
                if (handle.IsValid()) Addressables.Release(handle);
                ct.ThrowIfCancellationRequested();
            }
            await NextFrame(); // 메인스레드 안전
        }
        progress?.Report(1f);
        return handle.Result;
    }
    #endregion

    // ----------------------------------------------------------------------
    // 1) 단일 에셋 로드/해제
    /// <사용법>
    /// // 로드 (데이터 원본)
    /// var data = await AddressableLoader.LoadAssetAsync<MyData>("Data/MyData");
    /// 
    /// 원본 사용... 텍스처/오디오/큰 메시/애니메이션과 같은 메모리 소모가 큰 녀석들은 복제 시 메모리 소모값이 비싸므로
    /// 원본 핸들을 놓지 않고 유지는 방향으로 진행. (Instantiate로 사용하는 등)
    /// // 해제 (*더 이상 사용하지 않는 시점에서 해제)
    /// AddressableLoader.ReleaseAsset<MyData>("Data/MyData");
    /// 
    /// data 데이터의 복제본 생성 및 복제본 사용... 작고 가벼운 SO/클래스/숫자/문자/Struct만 얕은 복제로 사용 가능
    /// => 순수 데이터만을 불러와서 사용하는 용도(원본 수정x)일 때
    /// // 해제 (*복제본 생성 및 값 할당 즉시 해제)
    /// AddressableLoader.ReleaseAsset<MyData>("Data/MyData");
    // ----------------------------------------------------------------------
    public static async Task<T> LoadAssetAsync<T>(string key, IProgress<float> progress = null, CancellationToken ct = default)
    {
        var akey = new AssetKey(key, typeof(T));
        if (_assetHandles.TryGetValue(akey, out var found))
        {
            _assetHandles[akey] = (found.handle, found.refCount + 1);
            return (T)found.handle.Result;
        }

        var handle = Addressables.LoadAssetAsync<T>(key);
        var result = await AwaitWithProgress(handle, progress, ct);
        _assetHandles[akey] = (handle, 1);
        return result;
    }

    public static async Task<T> LoadAssetAsync<T>(AssetReferenceT<T> aref, IProgress<float> progress = null, CancellationToken ct = default) where T : UnityEngine.Object
    {
        if (aref == null || !aref.RuntimeKeyIsValid()) throw new ArgumentException("Invalid AssetReference");
        var akey = new AssetKey(aref.RuntimeKey, typeof(T));

        if (_assetHandles.TryGetValue(akey, out var found))
        {
            _assetHandles[akey] = (found.handle, found.refCount + 1);
            return (T)found.handle.Result;
        }

        var handle = aref.LoadAssetAsync<T>();
        var result = await AwaitWithProgress(handle, progress, ct);
        _assetHandles[akey] = (handle, 1);
        return result;
    }

    public static void ReleaseAsset<T>(string key)
    {
        var akey = new AssetKey(key, typeof(T));
        if (!_assetHandles.TryGetValue(akey, out var tuple)) return;

        var (handle, refCount) = tuple;
        refCount--;
        if (refCount <= 0)
        {
            if (handle.IsValid()) Addressables.Release(handle);
            _assetHandles.Remove(akey);
        }
        else _assetHandles[akey] = (handle, refCount);
    }

    public static void ReleaseAsset<T>(AssetReferenceT<T> aref) where T : UnityEngine.Object
    {
        if (aref == null) return;
        var akey = new AssetKey(aref.RuntimeKey, typeof(T));
        if (!_assetHandles.TryGetValue(akey, out var tuple)) return;

        var (handle, refCount) = tuple;
        refCount--;
        if (refCount <= 0)
        {
            if (handle.IsValid()) Addressables.Release(handle);
            _assetHandles.Remove(akey);
        }
        else _assetHandles[akey] = (handle, refCount);
    }

    // ----------------------------------------------------------------------
    // 2) 프리팹 인스턴스 생성/파괴
    /// <사용법>
    /// GameObject inst = await AddressableLoader.InstantiateAsync("Enemies/Goblin", parent: transform);
    /// // 사용...
    /// AddressableLoader.ReleaseInstance(inst); -> OnDestroy에서 넣어주면 수명 자동 정리됨. 별도로 원본 ReleaseAsset을 호출하지 않아도 됨.
    // ----------------------------------------------------------------------
    public static async Task<GameObject> InstantiateAsync(string prefabKey, Transform parent = null, Vector3? position = null, Quaternion? rotation = null, IProgress<float> progress = null, CancellationToken ct = default)
    {
        AsyncOperationHandle<GameObject> handle;
        if (position.HasValue || rotation.HasValue)
            handle = Addressables.InstantiateAsync(prefabKey, position ?? Vector3.zero, rotation ?? Quaternion.identity, parent);
        else
            handle = Addressables.InstantiateAsync(prefabKey, parent);

        var go = await AwaitWithProgress(handle, progress, ct);
        _instanceHandles[go] = handle;
        return go;
    }

    public static async Task<GameObject> InstantiateAsync(AssetReferenceGameObject aref, Transform parent = null, Vector3? position = null, Quaternion? rotation = null, IProgress<float> progress = null, CancellationToken ct = default)
    {
        if (aref == null || !aref.RuntimeKeyIsValid()) throw new ArgumentException("Invalid AssetReferenceGameObject");
        AsyncOperationHandle<GameObject> handle;

        if (position.HasValue || rotation.HasValue)
            handle = aref.InstantiateAsync(position ?? Vector3.zero, rotation ?? Quaternion.identity, parent);
        else
            handle = aref.InstantiateAsync(parent);

        var go = await AwaitWithProgress(handle, progress, ct);
        _instanceHandles[go] = handle;
        return go;
    }

    public static void ReleaseInstance(GameObject instance)
    {
        if (instance == null) return;

        if (_instanceHandles.TryGetValue(instance, out var handle))
        {
            if (handle.IsValid()) Addressables.ReleaseInstance(handle);
            _instanceHandles.Remove(instance);
        }
        else
        {
            // 핸들을 못 찾았어도 인스턴스 단독 해제 지원
            Addressables.ReleaseInstance(instance);
        }
    }

    // ----------------------------------------------------------------------
    // 3) 라벨 기반 프리로드/일괄 로드/해제
    /// <사용법>
    /// var list = await AddressableLoader.LoadByLabelAsync<UnityEngine.Object>("Level1");
    /// // 전역에서 사용 가능...
    /// AddressableLoader.ReleaseLabel<UnityEngine.Object>("Level1"); 
    /// 
    /// // 네트워크 필요 시(원격 다운로드)
    /// long bytes = await AddressableLoader.GetDownloadSizeAsync("Level1");
    /// if (bytes > 0) await AddressableLoader.DownloadDependenciesAsync("Level1"); 
    // ----------------------------------------------------------------------
    public static async Task<long> GetDownloadSizeAsync(object labelOrKey)
    {
        var h = Addressables.GetDownloadSizeAsync(labelOrKey);
        var size = await h.Task;
        if (h.IsValid()) Addressables.Release(h);
        return size;
    }

    public static async Task DownloadDependenciesAsync(object labelOrKey, IProgress<float> progress = null, CancellationToken ct = default)
    {
        var handle = Addressables.DownloadDependenciesAsync(labelOrKey);
        while (!handle.IsDone)
        {
            progress?.Report(handle.PercentComplete);
            if (ct.IsCancellationRequested)
            {
                if (handle.IsValid()) Addressables.Release(handle);
                ct.ThrowIfCancellationRequested();
            }
            await NextFrame();
        }
        progress?.Report(1f);
        if (handle.IsValid()) Addressables.Release(handle); // 다운로드 핸들은 즉시 해제해도 캐시는 남음
    }

    // 실제 메모리에 로드(필요 시 유지)
    public static async Task<IList<T>> LoadByLabelAsync<T>(string label, Action<T> onItemLoaded = null, IProgress<float> progress = null, CancellationToken ct = default)
    {
        var akey = new AssetKey(label, typeof(T));
        if (_labelLoadHandles.TryGetValue(akey, out var found))
            return (IList<T>)found.Convert<IList<T>>().Result;

        var handle = Addressables.LoadAssetsAsync<T>(label, item => onItemLoaded?.Invoke(item));
        // 간단한 진행 보고
        while (!handle.IsDone)
        {
            progress?.Report(handle.PercentComplete);
            if (ct.IsCancellationRequested)
            {
                if (handle.IsValid()) Addressables.Release(handle);
                ct.ThrowIfCancellationRequested();
            }
            await NextFrame();
        }
        _labelLoadHandles[akey] = handle;
        progress?.Report(1f);
        return handle.Result;
    }

    public static void ReleaseLabel<T>(string label)
    {
        var akey = new AssetKey(label, typeof(T));
        if (_labelLoadHandles.TryGetValue(akey, out var handle))
        {
            if (handle.IsValid()) Addressables.Release(handle);
            _labelLoadHandles.Remove(akey);
        }
    }

    // ----------------------------------------------------------------------
    // 4) 씬 로드/언로드
    /// <사용법> 
    /// await AddressableLoader.LoadSceneAsync("Scenes/MyAddressableScene", LoadSceneMode.Additive);
    /// // ...씬 사용...
    /// await AddressableLoader.UnloadSceneAsync("Scenes/MyAddressableScene");
    // ----------------------------------------------------------------------
    public static async Task<SceneInstance> LoadSceneAsync(string sceneKey, LoadSceneMode mode = LoadSceneMode.Additive, bool activateOnLoad = true, IProgress<float> progress = null, CancellationToken ct = default)
    {
        if (_sceneHandles.ContainsKey(sceneKey))
            return _sceneHandles[sceneKey].Result;

        var handle = Addressables.LoadSceneAsync(sceneKey, mode, activateOnLoad);
        while (!handle.IsDone)
        {
            progress?.Report(handle.PercentComplete);
            if (ct.IsCancellationRequested)
            {
                if (handle.IsValid()) Addressables.Release(handle);
                ct.ThrowIfCancellationRequested();
            }
            await NextFrame();
        }
        _sceneHandles[sceneKey] = handle;
        progress?.Report(1f);
        return handle.Result;
    }

    public static async Task UnloadSceneAsync(string sceneKey, IProgress<float> progress = null)
    {
        if (!_sceneHandles.TryGetValue(sceneKey, out var handle)) return;
        var unload = Addressables.UnloadSceneAsync(handle);
        while (!unload.IsDone)
        {
            progress?.Report(unload.PercentComplete);
            await NextFrame();
        }
        if (handle.IsValid()) Addressables.Release(handle);
        _sceneHandles.Remove(sceneKey);
        progress?.Report(1f);
    }

    // ----------------------------------------------------------------------
    // 5) 전체 정리(디버그/씬종료용)
    /// <사용법>
    /// AddressableLoader.ReleaseAll(); 
    // ----------------------------------------------------------------------
    public static void ReleaseAll()
    {
        foreach (var kv in _instanceHandles)
            if (kv.Value.IsValid()) Addressables.ReleaseInstance(kv.Value);
        _instanceHandles.Clear();

        foreach (var kv in _assetHandles)
            if (kv.Value.handle.IsValid()) Addressables.Release(kv.Value.handle);
        _assetHandles.Clear();

        foreach (var kv in _labelLoadHandles)
            if (kv.Value.IsValid()) Addressables.Release(kv.Value);
        _labelLoadHandles.Clear();

        foreach (var kv in _sceneHandles)
            if (kv.Value.IsValid()) Addressables.Release(kv.Value);
        _sceneHandles.Clear();

        Debug.Log("[Addr] Released all handles.");
    }

    // ----------------------------------------------------------------------
    // 6) AssetReference 안전 사용
    ///
    /// public AssetReferenceGameObject enemyRef;
    ///
    /// // 로드 + 인스턴스
    /// var enemy = await AddressableLoader.InstantiateAsync(enemyRef, parent: transform);
    ///
    /// // 파괴
    /// AddressableLoader.ReleaseInstance(enemy);
    ///
    /// // 에셋만 로드 유지하고 싶다면
    /// var clip = await AddressableLoader.LoadAssetAsync<AudioClip>(someClipRef);
    /// // ...
    /// AddressableLoader.ReleaseAsset<AudioClip>(someClipRef);

    // ----------------------------------------------------------------------


    // ----------------------------------------------------------------------
    // 7) 버튼 클릭(유니티 이벤트)에서 사용법
    ///
    /// async Task를 UnityEvent에 직접 연결할 수 없기 때문에 아래처럼 호출
    /// spawnBtn.onClick.AddListener(() => _ = OnClickSpawn()); // fire-and-forget  
    /// 
    /// 버튼 핸들러는 async void로
    /// private async System.Threading.Tasks.Task OnClickSpawn()
    /// {
    ///    try
    ///    {
    ///        spawned(=클래스 필드 변수) = await Addr.InstantiateAsync("Prefabs/Cube", parent: transform);
    ///    }
    ///    catch (System.Exception e)
    ///    {
    ///        Debug.LogError(e);
    ///    }
    /// }
    // ----------------------------------------------------------------------
}
*/