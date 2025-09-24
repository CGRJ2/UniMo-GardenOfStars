using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class GameManager : Singleton<GameManager>
{
    //임시
    public bool initialized { get; private set; }
    public bool inDownloading { get; private set; }

    public ObservableProperty<float> downloadProgress = new();

    private void Awake() => Init();

    void Init()
    {
        base.SingletonInit();
        Application.targetFrameRate = 60;

        StartCoroutine(Fetch());
    }
    
    #region Addressable Assets Storage 동기화 체크

    public IEnumerator Fetch()
    {
        yield return Addressables.InitializeAsync(true);
        var checkHandle = Addressables.CheckForCatalogUpdates(false);  // 변경된 카탈로그 ID들
        yield return checkHandle;

        if(checkHandle.Status == AsyncOperationStatus.Failed)
        {
            Manager.firebase.NetworkDisconnected();
            yield break;
        }

        Debug.Log($"업데이트 존재 여부 => {checkHandle.Result.Count}");

        if (checkHandle.Result.Count > 0)
        {
            // 새로 로드된 카탈로그의 IResourceLocator들
            var updateCatalogHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
            yield return updateCatalogHandle;

            if(updateCatalogHandle.Status == AsyncOperationStatus.Failed)
            {
                Manager.firebase.NetworkDisconnected();
                yield break;
            }

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
            if (sizeCheckHandle.Status == AsyncOperationStatus.Failed)
            {
                Manager.firebase.NetworkDisconnected();
                yield break;
            }
            Debug.Log($"다운로드사이즈 어싱크{sizeCheckHandle.Result}");

            if(sizeCheckHandle.Result <= 0)
            {
                SafeRelease(ref updateCatalogHandle);
                SafeRelease(ref checkHandle);
                initialized = true;
                yield break;
            }

            inDownloading = true;

            // 다운로드 진행
            var downloadHandle = Addressables.DownloadDependenciesAsync(locations);

            while (!downloadHandle.IsDone)
            {
                yield return null;
                DownloadStatus downloadStatus = downloadHandle.GetDownloadStatus();
                Debug.Log($"다운로드진행상황{downloadStatus.DownloadedBytes} / {sizeCheckHandle.Result}bytes 다운됨. 퍼센트:{(int)downloadStatus.Percent * 100}");
                downloadProgress.Value = downloadStatus.Percent;

                if(downloadHandle.Status == AsyncOperationStatus.Failed)
                {
                    inDownloading = false;
                    Manager.firebase.NetworkDisconnected();
                    yield break;
                }
            }
            downloadProgress.Value = 1f;

            yield return downloadHandle;
            Debug.Log($"다운로드 완료:{downloadHandle.GetDownloadStatus().IsDone}");

            // 새로운 요소 추가 이후에 사용하지 않는 참조 캐시 삭제
            var clearCacheHandle = Addressables.CleanBundleCache();

            SafeRelease(ref updateCatalogHandle);
            SafeRelease(ref clearCacheHandle);
        }
        initialized = true;
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

public class StageDataCsv : IUsableId
{
    public string Id;

    public string Name_KR;
    public string Name_En;

    public Sprite WheelSprite;
    public Sprite CenterSprite;

    public int RequiredQuestIndex;
    public string NextStageId;

    public string NpcID;
    public string BuildingIDs;

    public int StageInflationRate;
    public int StageAutoReward;


    public string GetId()
    {
        return Id;
    }

    public string[] GetBuildingIdList()
    {
        string[] buildingIdList =
            BuildingIDs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

        return buildingIdList;
    }
}

public partial class DataManager
{
    [SerializeField] private bool _isStageAdressable = true;

    // 구글 스프레드 시트 다운로드 주소
    private const string _stageDataTableURL = "https://docs.google.com/spreadsheets/d/1CwrcyyODjYAwjCgYkofKQl815o-vOWkUH7yy6mdUtY4/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _stageAdress = "StageCsv";

    public DataTableParser<StageDataCsv> Stage;
    private async void StageRoutine()
    {
        string dataCsv;

        if (_isStageAdressable)
        {
            dataCsv = await GetDataString(_isStageAdressable, _stageAdress);
        }
        else
        {
            dataCsv = await GetDataString(_isStageAdressable, _stageDataTableURL);
        }

        Stage = new DataTableParser<StageDataCsv>((words, dict) =>
        {
            StageDataCsv stage = new StageDataCsv();

            stage.Id = words[dict["ID"]];

            stage.Name_KR = words[dict["Name_Korean"]];
            stage.Name_En = words[dict["Name_English"]];

            if (Addressables.ResourceLocators.Any(locator => locator.Locate($"Sprite/{words[dict["WheelSprite"]]}", typeof(Sprite), out var locations)))
            {
                /*Addressables.LoadAssetAsync<Sprite>($"Sprite/{words[dict["WheelSprite"]]}").Completed += task =>
                {
                    if (task.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogWarning("WheelSprite 로드 실패");
                        return;
                    }

                    stage.WheelSprite = task.Result;
                };*/
            }

            if (Addressables.ResourceLocators.Any(locator => locator.Locate($"Sprite/{words[dict["CenterSprite"]]}", typeof(Sprite), out var locations)))
            {
                /*Addressables.LoadAssetAsync<Sprite>($"Sprite/{words[dict["CenterSprite"]]}").Completed += task =>
                {
                    if (task.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogWarning("CenterSprite 로드 실패");
                        return;
                    }

                    stage.WheelSprite = task.Result;
                };*/
            }

            int.TryParse(words[dict["RequiredQuestIndex"]], out stage.RequiredQuestIndex);
            int.TryParse(words[dict["InflationRate"]], out stage.StageInflationRate);
            int.TryParse(words[dict["StageAutoReward"]], out stage.StageAutoReward);

            stage.NextStageId = words[dict["NextStageId"]];

            stage.NpcID = words[dict["NpcID"]];
            stage.BuildingIDs = words[dict["BuildingIDs"]];

            return stage;
        });

        Stage.Load(dataCsv);
    }
}
