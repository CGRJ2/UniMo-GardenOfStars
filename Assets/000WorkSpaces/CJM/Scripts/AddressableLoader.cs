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
    // static Ŭ������ �ڷ�ƾ�� ����� �� ����, ���� �����ӱ��� ����ϴ� ����� ���� ������ ������ Runner ����
    #region Runner (�ڷ�ƾ/������ ����) 
    private sealed class Runner : MonoBehaviour { }
    private static Runner _runner;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static async void RuntimeInit()
    {
        if (_runner != null) return;
        var go = new GameObject("[Addr.Runner]");
        UnityEngine.Object.DontDestroyOnLoad(go);
        _runner = go.AddComponent<Runner>();

        // Addressables ��� �ʱ�ȭ(����)
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

    #region ���� ���� (�ڵ� ĳ��/����ī��Ʈ)
    private readonly struct AssetKey : IEquatable<AssetKey>
    {
        public readonly object Key;           // string Ű �Ǵ� AssetReference ��
        public readonly Type Type;          // ��û Ÿ��(T)
        public AssetKey(object key, Type type) { Key = key; Type = type; }
        public bool Equals(AssetKey other) => Equals(Key, other.Key) && Type == other.Type;
        public override bool Equals(object obj) => obj is AssetKey ak && Equals(ak);
        public override int GetHashCode() => ((Key?.GetHashCode() ?? 0) * 397) ^ (Type?.GetHashCode() ?? 0);
        public override string ToString() => $"{Key} ({Type?.Name})";
    }

    // ���� �ε� �ڵ�(���� ī��Ʈ ����)
    private static readonly Dictionary<AssetKey, (AsyncOperationHandle handle, int refCount)>
        _assetHandles = new();

    // Instantiate �ڵ�: �ν��Ͻ� �� ���� �ڵ�
    private static readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>>
        _instanceHandles = new();

    // �� �ε� �ڵ� (Ÿ�Ժ� �и�)
    private static readonly Dictionary<AssetKey, AsyncOperationHandle>
        _labelLoadHandles = new();

    // �� �ڵ�: sceneKey �� SceneInstance �ڵ�
    private static readonly Dictionary<string, AsyncOperationHandle<SceneInstance>>
        _sceneHandles = new();
    #endregion

    #region ���� ��ƿ
    // handle.Task�ε� ��ٸ� �� ������, �Ʒ� 4������ ������ ���� ��ٸ��� ��� �߰�
    // 1. ������� �� ������ ����ϰ� ����(PercentComplete)
    // 2. ���(CancellationToken) �� �߰��� ���� ����(Release)���� ����
    // 3. await NextFrame()�� �׻� ���� ������ ������ �������� ����
    // 4. UI �����/�ε� ���ǳ�/��Ʈ��ũ �ߴ� ���� UX�� �ϰ������� ���̱� ����
    private static async Task<T> AwaitWithProgress<T>(AsyncOperationHandle<T> handle, IProgress<float> progress, CancellationToken ct)
    {
        while (!handle.IsDone)
        {
            progress?.Report(handle.PercentComplete);
            if (ct.IsCancellationRequested)
            {
                // ���� ����
                if (handle.IsValid()) Addressables.Release(handle);
                ct.ThrowIfCancellationRequested();
            }
            await NextFrame(); // ���ν����� ����
        }
        progress?.Report(1f);
        return handle.Result;
    }
    #endregion

    // ----------------------------------------------------------------------
    // 1) ���� ���� �ε�/����
    /// <����>
    /// // �ε� (������ ����)
    /// var data = await AddressableLoader.LoadAssetAsync<MyData>("Data/MyData");
    /// 
    /// ���� ���... �ؽ�ó/�����/ū �޽�/�ִϸ��̼ǰ� ���� �޸� �Ҹ� ū �༮���� ���� �� �޸� �Ҹ��� ��ιǷ�
    /// ���� �ڵ��� ���� �ʰ� ������ �������� ����. (Instantiate�� ����ϴ� ��)
    /// // ���� (*�� �̻� ������� �ʴ� �������� ����)
    /// AddressableLoader.ReleaseAsset<MyData>("Data/MyData");
    /// 
    /// data �������� ������ ���� �� ������ ���... �۰� ������ SO/Ŭ����/����/����/Struct�� ���� ������ ��� ����
    /// => ���� �����͸��� �ҷ��ͼ� ����ϴ� �뵵(���� ����x)�� ��
    /// // ���� (*������ ���� �� �� �Ҵ� ��� ����)
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
    // 2) ������ �ν��Ͻ� ����/�ı�
    /// <����>
    /// GameObject inst = await AddressableLoader.InstantiateAsync("Enemies/Goblin", parent: transform);
    /// // ���...
    /// AddressableLoader.ReleaseInstance(inst); -> OnDestroy���� �־��ָ� ���� �ڵ� ������. ������ ���� ReleaseAsset�� ȣ������ �ʾƵ� ��.
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
            // �ڵ��� �� ã�Ҿ �ν��Ͻ� �ܵ� ���� ����
            Addressables.ReleaseInstance(instance);
        }
    }

    // ----------------------------------------------------------------------
    // 3) �� ��� �����ε�/�ϰ� �ε�/����
    /// <����>
    /// var list = await AddressableLoader.LoadByLabelAsync<UnityEngine.Object>("Level1");
    /// // �������� ��� ����...
    /// AddressableLoader.ReleaseLabel<UnityEngine.Object>("Level1"); 
    /// 
    /// // ��Ʈ��ũ �ʿ� ��(���� �ٿ�ε�)
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
        if (handle.IsValid()) Addressables.Release(handle); // �ٿ�ε� �ڵ��� ��� �����ص� ĳ�ô� ����
    }

    // ���� �޸𸮿� �ε�(�ʿ� �� ����)
    public static async Task<IList<T>> LoadByLabelAsync<T>(string label, Action<T> onItemLoaded = null, IProgress<float> progress = null, CancellationToken ct = default)
    {
        var akey = new AssetKey(label, typeof(T));
        if (_labelLoadHandles.TryGetValue(akey, out var found))
            return (IList<T>)found.Convert<IList<T>>().Result;

        var handle = Addressables.LoadAssetsAsync<T>(label, item => onItemLoaded?.Invoke(item));
        // ������ ���� ����
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
    // 4) �� �ε�/��ε�
    /// <����> 
    /// await AddressableLoader.LoadSceneAsync("Scenes/MyAddressableScene", LoadSceneMode.Additive);
    /// // ...�� ���...
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
    // 5) ��ü ����(�����/�������)
    /// <����>
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
    // 6) AssetReference ���� ���
    ///
    /// public AssetReferenceGameObject enemyRef;
    ///
    /// // �ε� + �ν��Ͻ�
    /// var enemy = await AddressableLoader.InstantiateAsync(enemyRef, parent: transform);
    ///
    /// // �ı�
    /// AddressableLoader.ReleaseInstance(enemy);
    ///
    /// // ���¸� �ε� �����ϰ� �ʹٸ�
    /// var clip = await AddressableLoader.LoadAssetAsync<AudioClip>(someClipRef);
    /// // ...
    /// AddressableLoader.ReleaseAsset<AudioClip>(someClipRef);

    // ----------------------------------------------------------------------


    // ----------------------------------------------------------------------
    // 7) ��ư Ŭ��(����Ƽ �̺�Ʈ)���� ����
    ///
    /// async Task�� UnityEvent�� ���� ������ �� ���� ������ �Ʒ�ó�� ȣ��
    /// spawnBtn.onClick.AddListener(() => _ = OnClickSpawn()); // fire-and-forget  
    /// 
    /// ��ư �ڵ鷯�� async void��
    /// private async System.Threading.Tasks.Task OnClickSpawn()
    /// {
    ///    try
    ///    {
    ///        spawned(=Ŭ���� �ʵ� ����) = await Addr.InstantiateAsync("Prefabs/Cube", parent: transform);
    ///    }
    ///    catch (System.Exception e)
    ///    {
    ///        Debug.LogError(e);
    ///    }
    /// }
    // ----------------------------------------------------------------------
}
*/