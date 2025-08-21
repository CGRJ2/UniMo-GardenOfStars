using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class GameManager : Singleton<GameManager>
{
    private void Awake() => Init();

    void Init()
    {
        base.SingletonInit();
        StartCoroutine(Fetch());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Addressables.LoadAssetAsync<GameObject>("TestCube").Completed += task =>
            {
                Instantiate(task.Result);
            };
        }
    }


    #region Addressable Assets Storage ����ȭ üũ

    IEnumerator Fetch()
    {
        yield return Addressables.InitializeAsync(true);
        var checkHandle = Addressables.CheckForCatalogUpdates(false);  // ����� īŻ�α� ID��
        yield return checkHandle;
        Debug.Log($"������Ʈ ���� ���� => {checkHandle.Result.Count}");

        if (checkHandle.Result.Count > 0)
        {
            // ���� �ε�� īŻ�α��� IResourceLocator��
            var updateCatalogHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
            yield return updateCatalogHandle;

            // IResourceLocator���� IResourceLocation���� ġȯ
            var locators = updateCatalogHandle.Result;
            var locations = new List<IResourceLocation>();

            foreach (var locator in locators)
            {
                foreach (var key in locator.Keys)
                {
                    //Debug.LogWarning($"Locator:{key.ToString()}");
                    if (locator.Locate(key, typeof(object), out var found))
                        locations.AddRange(found);
                }
            }

            // �ٿ�ε� ������ üũ
            var sizeCheckHandle = Addressables.GetDownloadSizeAsync(locations);
            yield return sizeCheckHandle;
            Debug.Log($"�ٿ�ε������ ���ũ{sizeCheckHandle.Result}");


            // �ٿ�ε� ����
            var downloadHandle = Addressables.DownloadDependenciesAsync(locations);

            while (!downloadHandle.IsDone)
            {
                yield return null;
                DownloadStatus downloadStatus = downloadHandle.GetDownloadStatus();
                Debug.Log($"�ٿ�ε������Ȳ{downloadStatus.DownloadedBytes} / {sizeCheckHandle.Result}bytes �ٿ��. �ۼ�Ʈ:{(int)downloadStatus.Percent * 100}");
            }
            yield return downloadHandle;
            Debug.Log($"�ٿ�ε� �Ϸ�:{downloadHandle.GetDownloadStatus().IsDone}");



            // ���ο� ��� �߰� ���Ŀ� ������� �ʴ� ���� ĳ�� ����
            var clearCacheHandle = Addressables.CleanBundleCache();

            SafeRelease(ref updateCatalogHandle);
            SafeRelease(ref clearCacheHandle);
        }
        SafeRelease(ref checkHandle);
    }

    // �ڵ� ���� ���� ��ƿ (�ߺ� Release ����)
    static void SafeRelease<T>(ref AsyncOperationHandle<T> handle)
    {
        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }
        handle = default;
    }

    #endregion

}
