using System;
using UnityEngine;

public class BuildingInstance : InteractableBase
{
    [SerializeField] protected BuildingData _OriginData;           // CSV or Sheet�� ���� ����

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
        //Ȱ��ȭ���� (�÷��̾��� UI��ư Ȱ��ȭ)

    }

    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);
        //��Ȱ��ȭ (�÷��̾��� UI��ư ��Ȱ��ȭ)
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
