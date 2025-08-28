using Cinemachine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 건물 데이터를 여기서 관리해야할듯 (업그레이드 상태, 위치정보, <<< 이걸 또 스테이지 별로 나눠야함)

public class BuildingManager : Singleton<BuildingManager>
{
    public WorkStatoinLists workStatinLists = new ();

    // 건물id(string)에 해당하는 업그레이드 정보를 저장
    Dictionary<string, UpgradeData> upgradeDataDic = new();

    // 스테이지id(string) 별, 건물들의 배치 정보를 저장
    Dictionary<string, Dictionary<Vector3Int, BiPlacementData>> biPlacementDataDic = new();

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
        upgradeDataDic[buildingId].level_ProdTime += statProdTimeAdd;
        upgradeDataDic[buildingId].level_Capacity += statCapacityAdd;
    }

    public UpgradeData GetUpgradeData(string buildingId)
    {
        // 해당 건물에 대한 업그레이드 데이터가 없다면 => 0단계로 생성
        if (!upgradeDataDic.ContainsKey(buildingId))
            upgradeDataDic[buildingId] = new UpgradeData();

        return upgradeDataDic[buildingId];
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
public class UpgradeData // 업그레이드 된 상태만 저장하면 됨
{
    public int level_ProdTime;
    public int level_Capacity;

    public UpgradeData()
    {
        level_ProdTime = 0;
        level_Capacity = 0;
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