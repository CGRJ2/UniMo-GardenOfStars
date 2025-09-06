using Cinemachine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

// 건물 데이터를 여기서 관리해야할듯 (업그레이드 상태, 위치정보, <<< 이걸 또 스테이지 별로 나눠야함)

public class BuildingManager : Singleton<BuildingManager>
{
    public WorkStatoinLists workStatinLists = new ();

    // 건물id(string)에 해당하는 업그레이드 정보를 저장
    Dictionary<string, UpgradeData> upgradeDataDic = new();

    // 스테이지id(string) 별, 건물들의 배치 정보를 저장
    Dictionary<string, Dictionary<Vector3Int, BiPlacementData>> biPlacementDataDic = new();

    public UnityAction<int> upgradeEvent;

    private void Awake() => Init();
    void Init()
    {
        base.SingletonInit();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach(var kvp in biPlacementDataDic[Manager.game.curStageId])
            {
                Debug.Log($"건물ID:{kvp.Value.buildingId} & 좌표:{kvp.Key}");
            }
        }
    }

    #region 건물 인스턴스 생성/제거 시, 현재 스테이지의 건물 배치 데이터 업데이트
    public void AddBiTransformData(Transform biTransform, string buildingId)
    {
        // 현재 스테이지에 건물 배치 정보가 없다면 빈 리스트 만들어주기
        if (!biPlacementDataDic.ContainsKey(Manager.game.curStageId))
        {
            biPlacementDataDic[Manager.game.curStageId] = new();
        }

        Vector3Int pos = Vector3Int.RoundToInt(biTransform.position);

        if (CanPlaceBuilding(pos))
        {
            biPlacementDataDic[Manager.game.curStageId][pos] = new BiPlacementData(biTransform, buildingId);
            Debug.Log("건물 배치 완료/biPlacementDataDic에 해당 건물 위치정보 저장");
        }
        else
        {
            Debug.Log("해당 좌표에 이미 건물이 존재함. 배치 불가능");
        }
        
    }
    public void RemoveBiTransformData(Transform biTransform)
    {
        if (Manager.game == null) return;

        if (biPlacementDataDic.ContainsKey(Manager.game.curStageId))
        {
            Vector3Int pos = Vector3Int.RoundToInt(biTransform.position);

            if (biPlacementDataDic[Manager.game.curStageId].ContainsKey(pos))
                biPlacementDataDic[Manager.game.curStageId].Remove(pos);
        }
    }
    public bool CanPlaceBuilding(Vector3Int pos)
    {
        // 현재 스테이지에 해당 좌표에 이미 건물이 있으면 false
        if (biPlacementDataDic[Manager.game.curStageId].ContainsKey(pos))
        {
            return false;
        }
        else return true;
    }
    #endregion

    public void UpdateUpgradedData(string buildingId, int statProdTimeAdd, int statCapacityAdd = 0)
    {
        upgradeDataDic[buildingId].Level_ProdTime.Value += statProdTimeAdd;
        upgradeDataDic[buildingId].Level_Capacity.Value += statCapacityAdd;
    }

    UpgradeData temp_UpgradeData;

    public UpgradeData GetUpgradeData(string buildingId)
    {
        // 해당 건물에 대한 업그레이드 데이터가 없다면 => 0단계로 생성
        UpgradeData upgradeData = Manager.firebase.UserData.BuildingUpgradeList.Get(buildingId);
        if (upgradeData == null)
        {
            Manager.firebase.UserData.BuildingUpgradeList.OnAdded.AddListener(UpgradeDataAddEvent);
            Manager.firebase.UserData.BuildingUpgradeList.Add(buildingId);

            upgradeDataDic.TryAdd(buildingId, temp_UpgradeData);
            return temp_UpgradeData;
        }
        else
        {
            upgradeDataDic.TryAdd(buildingId, upgradeData);
            return upgradeData;
        }
    }


    private void UpgradeDataAddEvent(UpgradeData upgradeData)
    {
        Debug.Log("리스트의 값이 변화해도 이벤트가 실행되나?");
        
        temp_UpgradeData = upgradeData;
        Manager.firebase.UserData.BuildingUpgradeList.OnAdded.RemoveListener(UpgradeDataAddEvent);
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

    public int level_ProdTime => Level_ProdTime.Value;
    public int level_Capacity => Level_Capacity.Value;

    public UpgradeData(string id, string parentPath = null) : base(id, parentPath)
    {
        Level_ProdTime = new FirebaseProperty<int>("Level_ProdTime", Path);
        Level_ProdTime.Subscribe((value) => Manager.buildings.upgradeEvent?.Invoke(value));
        InitList.Add(Level_ProdTime);

        Level_Capacity = new FirebaseProperty<int>("Level_Capacity", Path);
        Level_Capacity.Subscribe((value) => Manager.buildings.upgradeEvent?.Invoke(value));
        InitList.Add(Level_Capacity);
    }

    
}

[Serializable]
public class BiPlacementData
{
    public string buildingId;
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ;

    public BiPlacementData(Transform t, string buildingId)
    {
        this.buildingId = buildingId;
        var e = t.rotation.eulerAngles;
        posX = t.position.x;
        posY = t.position.y;
        posZ = t.position.z;
        rotX = e.x;
        rotY = e.y;
        rotZ = e.z;
    }

    public void ApplyTo(Transform t)
    {
        t.position = new Vector3(posX, posY, posZ);
        t.rotation = Quaternion.Euler(rotX, rotY, rotZ);
    }
}