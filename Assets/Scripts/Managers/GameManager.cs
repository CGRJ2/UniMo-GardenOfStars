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


    #region Addressable Assets Storage 동기화 체크

    IEnumerator Fetch()
    {
        yield return Addressables.InitializeAsync(true);
        var checkHandle = Addressables.CheckForCatalogUpdates(false);  // 변경된 카탈로그 ID들
        yield return checkHandle;
        Debug.Log($"업데이트 존재 여부 => {checkHandle.Result.Count}");

        if (checkHandle.Result.Count > 0)
        {
            // 새로 로드된 카탈로그의 IResourceLocator들
            var updateCatalogHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
            yield return updateCatalogHandle;

            // IResourceLocator들을 IResourceLocation으로 치환
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

            // 다운로드 사이즈 체크
            var sizeCheckHandle = Addressables.GetDownloadSizeAsync(locations);
            yield return sizeCheckHandle;
            Debug.Log($"다운로드사이즈 어싱크{sizeCheckHandle.Result}");


            // 다운로드 진행
            var downloadHandle = Addressables.DownloadDependenciesAsync(locations);

            while (!downloadHandle.IsDone)
            {
                yield return null;
                DownloadStatus downloadStatus = downloadHandle.GetDownloadStatus();
                Debug.Log($"다운로드진행상황{downloadStatus.DownloadedBytes} / {sizeCheckHandle.Result}bytes 다운됨. 퍼센트:{(int)downloadStatus.Percent * 100}");
            }
            yield return downloadHandle;
            Debug.Log($"다운로드 완료:{downloadHandle.GetDownloadStatus().IsDone}");



            // 새로운 요소 추가 이후에 사용하지 않는 참조 캐시 삭제
            var clearCacheHandle = Addressables.CleanBundleCache();

            SafeRelease(ref updateCatalogHandle);
            SafeRelease(ref clearCacheHandle);
        }
        SafeRelease(ref checkHandle);
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

    #endregion

}
