using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class GameManager : Singleton<GameManager>
{
    private void Awake() => Init();

    // 스테이지Id(key) 별, 언락여부(value) 딕셔너리 => 해당 데이터는 Firebase DB로 대체될 예정
    Dictionary<string, bool> stageUnlockDic = new();
    public void StageUnlock(string stageId) { stageUnlockDic[stageId] = true; } // DB에 바로 저장하는 걸로 대체될 예정
    public bool GetStageUnlockCheck(string stageId) { return stageUnlockDic[stageId]; } // DB에 바로 불러오는 걸로 대체될 예정


    // 스테이지 데이터에 관한 딕셔너리 (CSV파일을 로드해서 스테이지 id별로 데이터를 저장해둔 공간)
    public Dictionary<string, StageData> stageDataDic = new();

    // 현재 스테이지Id
    public string curStageId;

    void Init()
    {
        base.SingletonInit();
        StartCoroutine(Fetch());
        StageDatasInit();
    }
    
    void StageDatasInit()
    {
        Addressables.LoadAssetAsync<Temp_StageDataCsv>("Temp_StageDataCSV").Completed += csv =>
        {
            foreach(var value in csv.Result.stageDataColumns)
            {
                if (!stageUnlockDic.ContainsKey(value.StageId))
                {
                    // 첫 스테이지면 언락 항상 true /// for문으로 바꿔서 인덱스 0인걸로 처리해두면 키값 상관없이 가능할듯?
                    if (value.StageId == "Stage00")
                        stageUnlockDic.Add(value.StageId, true);
                    else
                        stageUnlockDic.Add(value.StageId, false);
                }

                // 스테이지 별 다음단계 언락 조건도 저장
                stageDataDic.TryAdd(value.StageId, value);

                // 데이터 베이스에서 로드 시엔 키 체크 후 해당 bool값으로 할당
            }
        };
    }

    /*void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            *//*Addressables.LoadAssetAsync<GameObject>("TestCube").Completed += task =>
            {
                Instantiate(task.Result);
            };*//*

            // 씬로드 테스트
        }
    }*/


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


    public IEnumerator Temp_InGameLoad()
    {
        Manager.game.curStageId = "Stage00";
        Manager.ui.ShowLoadingScreen();
        yield return new WaitForSeconds(0.3f);  // 임시

        var loadSceneHanlde = Addressables.LoadSceneAsync("StageScene");
        while (loadSceneHanlde.IsDone)
        {
            yield return null;
        }
        yield return loadSceneHanlde;
        Manager.ui.HideLoadingScreen();
    }
}
