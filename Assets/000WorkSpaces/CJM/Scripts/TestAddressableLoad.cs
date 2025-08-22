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
                Addressables.CheckForCatalogUpdates().Completed += task =>  // 변경된 카탈로그 ID들
                {
                    Debug.Log($"변경된 카탈로그 ID가 있나? => {task.Result.Count}");

                    Addressables.UpdateCatalogs(task.Result).Completed += task =>   // 새로 로드된 카탈로그의 로케이터들
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

                            Debug.Log($"다운로드진행상황{downloadStatus.DownloadedBytes}bytes 다운됨. 퍼센트:{task.GetDownloadStatus().Percent}");
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
            Debug.Log($"업데이트 결과 개수 {op.Result.Count}");
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
        // 스토리지 다운로드 테스트용
        //yield return Addressables.CleanBundleCache();

        // 초기화
        yield return Addressables.InitializeAsync();

        // 변경사항 있는 지 체크
        Debug.Log("업데이트 핸들 확인");
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;
        Debug.Log(checkHandle.Result.Count);


        if (checkHandle.Result.Count > 0)
        {
            Debug.Log("추가된 키 핸들 확인");
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result);   // 업데이트 된 카탈로그 갱신
            yield return updateHandle;
            //Debug.LogWarning(updateHandle.Result.Count);

            /*var locators = updateHandle.Result; // IList<IResourceLocator>
            var allKeys = new List<object>();
            foreach (var loc in locators)
                allKeys.AddRange(loc.Keys);

            // 다운 사이즈 확인
            var sizeHandle = Addressables.GetDownloadSize(allKeys);*/

            //UnityEngine.Debug.Log(sizeHandle.Result);

            Debug.Log("다운로드 진행");
            // 다운로드
            Addressables.DownloadDependenciesAsync(updateHandle.Result);
        }

        //if (사이즈 == 0) yield break;

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

        // 2) 원격 카탈로그 로드 (공개 GCS라면 storage.googleapis.com 사용)
        //    autoReleaseHandle=false 이므로 나중에 수동 Release
        string remoteCatalogUrl = "https://storage.googleapis.com/unimo_gardenofstars_addressableassets/Android/catalog_Test01.hash";
        var loadCatalog = Addressables.LoadContentCatalogAsync(remoteCatalogUrl, false);
        yield return loadCatalog;
        if (loadCatalog.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"LoadContentCatalogAsync failed: {loadCatalog.OperationException}");
            // init 핸들은 Addressables가 유지해도 되지만, 여기선 정리
            SafeRelease(ref init);
            yield break;
        }
        // (선택) 로케이터 사용 가능
        IResourceLocator locator = loadCatalog.Result;
        Debug.Log($"Catalog loaded. Locator Keys: {locator.Keys}");
        // 3) 업데이트 체크
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
            // 4) 카탈로그 업데이트
            var update = Addressables.UpdateCatalogs(catalogsToUpdate);
            yield return update;

            if (update.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("카탈로그 업데이트 완료!");
            }
            else
            {
                Debug.LogError($"UpdateCatalogs failed: {update.OperationException}");
            }

            // Update 핸들 해제
            SafeRelease(ref update);
        }
        else
        {
            Debug.Log("최신 상태입니다. 업데이트 필요 없음.");
        }

        // 5) 사용 후 해제 (중복 해제 금지)
        SafeRelease(ref check);
        SafeRelease(ref loadCatalog);

        // Initialize 핸들은 장기간 유지해도 되지만, 이 테스트 코루틴 종료 시 정리
        SafeRelease(ref init);
    }

    // 핸들 안전 해제 유틸 (중복 Release 방지)
    static void SafeRelease<T>(ref AsyncOperationHandle<T> handle)
    {
        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }
        handle = default;
    }
}
