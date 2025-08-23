using System;
using UnityEngine;

public class BuildingInstance : InteractableBase
{
    [SerializeField] protected BuildingData _OriginData;           // CSV or Sheet�� ���� ����
    [SerializeField] Canvas activateAreaPopUI;
    protected void BIBaseInit()
    {
        Manager.buildings.ActivatedBIList.Add(this);

        if (activateAreaPopUI != null)
        {
            activateAreaPopUI.worldCamera = Camera.main;
            activateAreaPopUI.gameObject.SetActive(false);
        }
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        Manager.buildings?.ActivatedBIList.Remove(this);

        if (activateAreaPopUI != null)
            activateAreaPopUI.gameObject.SetActive(false);
    }

    // �ǹ� Ȱ��ȭ ���� ��ȣ�ۿ�
    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);
        
        // ��ȣ�ۿ��� ��ü�� �÷��̾��� (�÷��̾� ����)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            if (activateAreaPopUI != null)
                activateAreaPopUI.gameObject.SetActive(true);  // �⺻ ��ȣ�ۿ� �˾� Ȱ��ȭ (���� �Ѵٸ�)
        }
    }

    // �ǹ� Ȱ��ȭ ���� ��ȣ�ۿ�
    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);

        // ��ȣ�ۿ��� ��ü�� �÷��̾��� (�÷��̾� ����)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            if (activateAreaPopUI != null)
                activateAreaPopUI.gameObject.SetActive(false); // �⺻ ��ȣ�ۿ� �˾� ��Ȱ��ȭ (���� �Ѵٸ�)
        }
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
