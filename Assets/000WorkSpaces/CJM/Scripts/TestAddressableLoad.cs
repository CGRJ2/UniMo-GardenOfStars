using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class TestAddressableLoad : MonoBehaviour
{
    void Start()
    {
        //StartCoroutine(LoadTest());
        //StartCoroutine(Test());

        Addressables.CleanBundleCache().Completed += task =>
        {
            Caching.ClearCache();

            Addressables.InitializeAsync().Completed += task =>
            {
                Addressables.CheckForCatalogUpdates().Completed += task =>  // ����� īŻ�α� ID��
                {
                    Debug.Log($"����� īŻ�α� ID�� �ֳ�? => {task.Result.Count}");

                    Addressables.UpdateCatalogs(task.Result).Completed += task =>   // ���� �ε�� īŻ�α��� �������͵�
                    {
                        var locators = task.Result;
                        var locations = new List<IResourceLocation>();

                        foreach (var locator in locators)
                        {
                            foreach (var key in locator.Keys)
                            {
                                Debug.LogWarning($"Locator:{key.ToString()}");
                                if (locator.Locate(key, typeof(object), out var found))
                                    locations.AddRange(found);
                            }
                        }

                        Addressables.DownloadDependenciesAsync(locations).Completed += task =>
                        {
                            DownloadStatus downloadStatus = task.GetDownloadStatus();

                            Debug.Log($"�ٿ�ε������Ȳ{downloadStatus.DownloadedBytes}bytes �ٿ��. �ۼ�Ʈ:{task.GetDownloadStatus().Percent}");
                            //Debug.Log(task.Result);
                        };
                    };
                };
            };
        };

    }

    public IEnumerator LoadTest()
    {
        //Load a catalog and automatically release the operation handle.
        AsyncOperationHandle<IResourceLocator> handle
            = Addressables.LoadContentCatalogAsync("https://storage.googleapis.com/unimo_gardenofstars_addressableassets/Android/catalog_Test01.hash", true);
        yield return handle;
        Debug.Log(handle.Result.LocatorId);
        //...


        List<string> catalogsToUpdate = new List<string>();
        AsyncOperationHandle<List<string>> checkForUpdateHandle
            = Addressables.CheckForCatalogUpdates();
        checkForUpdateHandle.Completed += op =>
        {
            catalogsToUpdate.AddRange(op.Result);
            Debug.Log($"������Ʈ ��� ���� {op.Result.Count}");
        };

        yield return checkForUpdateHandle;

        if (catalogsToUpdate.Count > 0)
        {
            AsyncOperationHandle<List<IResourceLocator>> updateHandle
                = Addressables.UpdateCatalogs(catalogsToUpdate);
            yield return updateHandle;
            Addressables.Release(updateHandle);
        }

        Addressables.Release(checkForUpdateHandle);
    }


    /////////////
    IEnumerator Test()
    {
        // ���丮�� �ٿ�ε� �׽�Ʈ��
        //yield return Addressables.CleanBundleCache();

        // �ʱ�ȭ
        yield return Addressables.InitializeAsync();

        // ������� �ִ� �� üũ
        Debug.Log("������Ʈ �ڵ� Ȯ��");
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;
        Debug.Log(checkHandle.Result.Count);


        if (checkHandle.Result.Count > 0)
        {
            Debug.Log("�߰��� Ű �ڵ� Ȯ��");
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result);   // ������Ʈ �� īŻ�α� ����
            yield return updateHandle;
            //Debug.LogWarning(updateHandle.Result.Count);

            /*var locators = updateHandle.Result; // IList<IResourceLocator>
            var allKeys = new List<object>();
            foreach (var loc in locators)
                allKeys.AddRange(loc.Keys);

            // �ٿ� ������ Ȯ��
            var sizeHandle = Addressables.GetDownloadSize(allKeys);*/

            //UnityEngine.Debug.Log(sizeHandle.Result);

            Debug.Log("�ٿ�ε� ����");
            // �ٿ�ε�
            Addressables.DownloadDependenciesAsync(updateHandle.Result);
        }

        //if (������ == 0) yield break;

        //yield return Addressables.DownloadDependenciesAsync();
    }

    public void aa(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle handle)
    {
        UnityEngine.Debug.Log(handle.Result);
    }

    IEnumerator TestTTT()
    {
        // 1) Initialize
        var init = Addressables.InitializeAsync(false);
        yield return init;
        if (init.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Initialize failed: {init.OperationException}");
            yield break;
        }

        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;
        Debug.Log(checkHandle.Task.Result.Count);

        // 2) ���� īŻ�α� �ε� (���� GCS��� storage.googleapis.com ���)
        //    autoReleaseHandle=false �̹Ƿ� ���߿� ���� Release
        string remoteCatalogUrl = "https://storage.googleapis.com/unimo_gardenofstars_addressableassets/Android/catalog_Test01.hash";
        var loadCatalog = Addressables.LoadContentCatalogAsync(remoteCatalogUrl, false);
        yield return loadCatalog;
        if (loadCatalog.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"LoadContentCatalogAsync failed: {loadCatalog.OperationException}");
            // init �ڵ��� Addressables�� �����ص� ������, ���⼱ ����
            SafeRelease(ref init);
            yield break;
        }
        // (����) �������� ��� ����
        IResourceLocator locator = loadCatalog.Result;
        Debug.Log($"Catalog loaded. Locator Keys: {locator.Keys}");
        // 3) ������Ʈ üũ
        var check = Addressables.CheckForCatalogUpdates(false);
        yield return check;
        if (check.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"CheckForCatalogUpdates failed: {check.OperationException}");
            SafeRelease(ref loadCatalog);
            SafeRelease(ref init);
            yield break;
        }

        Addressables.InternalIdTransformFunc = id =>
        {
            Debug.Log($"[Addr Request] {id}");
            return "dsd";
        };
        // int size = 0;
        // var downloadSize = Addressables.GetDownloadSizeAsync(remoteCatalogUrl);
        //
        // yield return downloadSize;
        //
        // Debug.Log($"DownloadSize: {downloadSize.Result}");

        List<string> catalogsToUpdate = check.Result;
        Debug.Log($"Need Update Count: {catalogsToUpdate?.Count ?? 0}");

        if (catalogsToUpdate != null && catalogsToUpdate.Count > 0)
        {
            // 4) īŻ�α� ������Ʈ
            var update = Addressables.UpdateCatalogs(catalogsToUpdate);
            yield return update;

            if (update.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("īŻ�α� ������Ʈ �Ϸ�!");
            }
            else
            {
                Debug.LogError($"UpdateCatalogs failed: {update.OperationException}");
            }

            // Update �ڵ� ����
            SafeRelease(ref update);
        }
        else
        {
            Debug.Log("�ֽ� �����Դϴ�. ������Ʈ �ʿ� ����.");
        }

        // 5) ��� �� ���� (�ߺ� ���� ����)
        SafeRelease(ref check);
        SafeRelease(ref loadCatalog);

        // Initialize �ڵ��� ��Ⱓ �����ص� ������, �� �׽�Ʈ �ڷ�ƾ ���� �� ����
        SafeRelease(ref init);
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
}
