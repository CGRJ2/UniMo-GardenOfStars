using System;
using UnityEngine;

public class BuildingInstance : InteractableBase
{
    [SerializeField] protected BuildingData _OriginData;           // CSV or Sheet로 변경 예정

    protected void BIBaseInit()
    {
        base.InitPopUI();
        Manager.buildings.ActivatedBIList.Add(this);
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        Manager.buildings?.ActivatedBIList.Remove(this);
    }

    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);
        //활성화영역 (플레이어라면 UI버튼 활성화)

    }

    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);
        //비활성화 (플레이어라면 UI버튼 비활성화)
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
