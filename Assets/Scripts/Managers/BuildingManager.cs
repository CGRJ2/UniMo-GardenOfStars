using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

// 건물 데이터를 여기서 관리해야할듯 (업그레이드 상태, 위치정보, <<< 이걸 또 스테이지 별로 나눠야함)

public class BuildingManager : Singleton<BuildingManager>
{
    public WorkStatoinLists workStatinLists = new ();

    public BuildingSeller buildingSeller;
    public WorkerManageBuilding workerBuilding;

    // 건물 구매 시, 건축모드 On / 설치 시, 건축모드 Off
    public Action<bool, string> BuildModEvent;

    private void Awake() => Init();
    void Init()
    {
        base.SingletonInit();
    }

    public UpgradeData GetUpgradeData(string buildingId)
    {
        // 해당 건물에 대한 업그레이드 데이터가 없다면 => 0단계로 생성
        UpgradeData upgradeData = Manager.firebase.UserData.BuildingUpgradeList.Get(buildingId);
        if (upgradeData == null)
        {
            Manager.firebase.UserData.BuildingUpgradeList.Add(buildingId);
            return null;
        }
        else
        {
            return upgradeData;
        }
    }
}

[Serializable]
public struct WorkStatoinLists
{
    public List<InsertArea> insertAreas;
    public List<WorkArea> workAreas;
    public List<WorkArea_SwitchType> workAreas_SwitchType;
    public List<ProdsArea> prodsAreas;
    public List<ProductGenerater> productGeneraters;

    public WorkStatoinLists(bool init = true)
    {
        insertAreas = new();
        workAreas = new();
        workAreas_SwitchType = new();
        prodsAreas = new();
        productGeneraters = new();
    }
}

[Serializable]
public class UpgradeData : FirebaseData
{
    public FirebaseProperty<int> Level_ProdTime;
    public FirebaseProperty<int> Level_Capacity;

    public UpgradeData(string id, string parentPath = null) : base(id, parentPath)
    {
        Level_ProdTime = new FirebaseProperty<int>("Level_ProdTime", Path);

        InitList.Add(Level_ProdTime);

        Level_Capacity = new FirebaseProperty<int>("Level_Capacity", Path);
        InitList.Add(Level_Capacity);
    }
}



public partial class DataManager
{
    public Dictionary<string, IngrediantData> Ingrediant = new();

    public void IngrediantDataInitRoutine()
    {
        Addressables.LoadAssetsAsync<IngrediantData>("Data", null, true).Completed += task =>
        {
            foreach (var data in task.Result)
            {
                if (Ingrediant.ContainsKey(data.ID)) continue;
                else
                {
                    Ingrediant.Add(data.ID, data);
                }
            }
        };
    }
}