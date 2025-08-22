/*using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class AddrResourceTest : MonoBehaviour
{
    [Header("Keys")]
    public string soKey = "Data/MyDataSO";
    public string prefabKey = "Prefabs/Cube";
    public string audioKey = "Audio/BgmTest";
    public string testLabel = "Test"; // 선택
    private GameObject _inst;
    private ScriptableObject _soClone;
    private AudioSource _audio;

    private void Awake()
    {
        _audio = gameObject.GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
    }

    private void OnDestroy()
    {
        // 혹시 남은 것들 정리
        if (_inst) AddressableLoader.ReleaseInstance(_inst);
        if (_soClone) Destroy(_soClone);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 420, 300), GUI.skin.box);
        GUILayout.Label("<b>Addressables Resource Smoke Test</b>", new GUIStyle(GUI.skin.label) { richText = true });

        if (GUILayout.Button("1) Prefab - InstantiateOnly (스폰 → 해제)")) _ = Prefab_InstantiateOnly();
        if (GUILayout.Button("2) Prefab - Preload → Instantiate → ReleaseAsset → ReleaseInstance")) _ = Prefab_PreloadThenSpawn();

        if (GUILayout.Button("3) SO - Load → Clone → ReleaseAsset(원본) → Destroy(Clone)")) _ = SO_ClonePattern();

        if (GUILayout.Button("4) Audio - Load → Play 1.2s → Stop → ReleaseAsset")) _ = Audio_DirectUse();

        if (GUILayout.Button("5) (선택) Label - Download → LoadByLabel → ReleaseLabel")) _ = Label_Test();

        GUILayout.Space(8);
        if (GUILayout.Button("ReleaseAll (디버그용)")) AddressableLoader.ReleaseAll();

        GUILayout.EndArea();
    }

    // ---------------------- Tests ----------------------

    private async Task Prefab_InstantiateOnly()
    {
        try
        {
            Debug.Log("[TEST] Prefab_InstantiateOnly start");
            _inst = await AddressableLoader.InstantiateAsync(prefabKey, parent: transform);
            await Task.Delay(600);
            AddressableLoader.ReleaseInstance(_inst); _inst = null;
            Debug.Log("[TEST] Prefab_InstantiateOnly OK");
        }
        catch (Exception e) { Debug.LogError($"[TEST] Prefab_InstantiateOnly FAIL: {e}"); }
    }

    private async Task Prefab_PreloadThenSpawn()
    {
        try
        {
            Debug.Log("[TEST] Prefab_PreloadThenSpawn start");
            await AddressableLoader.LoadAssetAsync<GameObject>(prefabKey);      // 프리로드(메모리)
            _inst = await AddressableLoader.InstantiateAsync(prefabKey);        // 스폰
            AddressableLoader.ReleaseAsset<GameObject>(prefabKey);              // 원본 핸들 정리
            await Task.Delay(600);
            AddressableLoader.ReleaseInstance(_inst); _inst = null;             // 인스턴스 해제
            Debug.Log("[TEST] Prefab_PreloadThenSpawn OK");
        }
        catch (Exception e) { Debug.LogError($"[TEST] Prefab_PreloadThenSpawn FAIL: {e}"); }
    }

    private async Task SO_ClonePattern()
    {
        try
        {
            Debug.Log("[TEST] SO_ClonePattern start");
            var so = await AddressableLoader.LoadAssetAsync<RegionSO>(soKey);   // 원본
            _soClone = ScriptableObject.Instantiate(so);           // 런타임 복제본(내 것)
            Debug.Log($"[TEST] SO_Clone title={((RegionSO)_soClone).regionName}, value={((RegionSO)_soClone).testValue}");
            AddressableLoader.ReleaseAsset<RegionSO>(soKey);                    // 원본 해제(복제본은 계속 사용 가능)
            await Task.Delay(300);
            Destroy(_soClone); _soClone = null;                    // 복제본 정리
            Debug.Log("[TEST] SO_ClonePattern OK");
        }
        catch (Exception e) { Debug.LogError($"[TEST] SO_ClonePattern FAIL: {e}"); }
    }

    private async Task Audio_DirectUse()
    {
        try
        {
            Debug.Log("[TEST] Audio_DirectUse start");
            var clip = await AddressableLoader.LoadAssetAsync<AudioClip>(audioKey);
            _audio.clip = clip;
            _audio.loop = false;
            _audio.Play();
            await Task.Delay(1200); // 1.2초 재생
            _audio.Stop();
            _audio.clip = null;
            AddressableLoader.ReleaseAsset<AudioClip>(audioKey);                // 사용 끝난 뒤 해제
            Debug.Log("[TEST] Audio_DirectUse OK");
        }
        catch (Exception e) { Debug.LogError($"[TEST] Audio_DirectUse FAIL: {e}"); }
    }

    private async Task Label_Test()
    {
        try
        {
            Debug.Log("[TEST] Label_Test start");
            var bytes = await AddressableLoader.GetDownloadSizeAsync(testLabel);
            if (bytes > 0)
            {
                Debug.Log($"[TEST] Label need download: {bytes} bytes");
                await AddressableLoader.DownloadDependenciesAsync(testLabel);
            }
            var list = await AddressableLoader.LoadByLabelAsync<UnityEngine.Object>(testLabel);
            Debug.Log($"[TEST] Label loaded count={list.Count}");
            AddressableLoader.ReleaseLabel<UnityEngine.Object>(testLabel);
            Debug.Log("[TEST] Label_Test OK");
        }
        catch (Exception e) { Debug.LogError($"[TEST] Label_Test FAIL: {e}"); }
    }
}
*/