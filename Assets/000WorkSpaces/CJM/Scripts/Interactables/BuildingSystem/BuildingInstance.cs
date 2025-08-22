using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInstance : InteractableBase
{
    [SerializeField] protected BuildingData _OriginData;           // CSV or Sheet�� ���� ����

    public override void EnterInteract_Player()
    {
        base.EnterInteract_Player();
        //Debug.Log($"�ǹ��ν��Ͻ�({buildingData?.name}): ����� ��ȣ�ۿ� ����");
    }

    public override void DeactiveInteract_Player()
    {
        base.DeactiveInteract_Player();
        //Debug.Log($"�ǹ��ν��Ͻ�({buildingData?.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}

[Serializable]
public class BuildingRuntimeData
{
    public int level;
    public int maxStackableCount;
    public float productionTime;

    public void SetCurLevelStatDatas(ManufactureBD manufactureBD)
    {
        this.maxStackableCount = manufactureBD.Stat_MaxStackableCount.Values[level];
        this.productionTime = manufactureBD.Stat_ProductionTime.Values[level];
    }

    public void SetCurLevelStatDatas(HarvestBD harvestBD)
    {
        this.productionTime = harvestBD.Stat_ProductionTime.Values[level];
    }
}
