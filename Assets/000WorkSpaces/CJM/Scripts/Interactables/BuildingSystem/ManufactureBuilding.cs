using System;
using System.Collections.Generic;
using UnityEngine;

public class ManufactureBuilding : BuildingInstance
{
    public ManufactureRuntimeData runtimeData;
    [HideInInspector] public ManufactureBD originData;

    [Header("��Ḧ �׾Ƴ��� ��ġ")]
    public Transform attachPoint;

    [Header("��� ���� ��� ������")]
    public float insertDelayTime = 0.2f;

    [Header("ȸ�� ��� ������")]
    public float prodsAbsorbDelayTime = 0.2f;

    [HideInInspector]
    public float progressedTime = 0f;   // ���� ���൵

    [Header("���� ���� ��ü")]
    public InsertArea insertArea;
    [Header("�۾� ���� ��ü")]
    public WorkArea workArea;
    public WorkArea_SwitchType workArea_SwitchType;
    [Header("ȸ�� ���� ��ü")]
    public ProdsArea prodsArea;


    public Stack<IngrediantInstance> ingrediantStack = new();
    //public Stack<IngrediantInstance> prodsStack = new();  // ȸ�������� ����ó�� ǥ���� �� ����ϴ°ɷ�

    private void Awake()
    {
        base.BIBaseInit();
        InitRuntimeData();
        activatePopUI.Init(this);
        
        insertArea.Init(this);
        workArea?.Init(this);
        workArea_SwitchType?.Init(this);
        prodsArea.Init(this);
    }

    void InitRuntimeData()
    {
        if (_OriginData is ManufactureBD mfBD)
        {
            originData = mfBD;
            runtimeData = new();
            runtimeData.SetCurLevelStatDatas(mfBD);
        }
    }


    public void CheckUpgradable()
    {
        int curLevel_ProdTime = runtimeData.level_ProductionTime; 
        int curLevel_StackCount = runtimeData.level_StackCount; 
        int curMoney = Manager.player.Data.Money;

        // �� ���� �� ���׷��̵� ����� ������ ��
        if (curMoney > originData.Stat_MaxStackableCount.cost[curLevel_StackCount] 
            || curMoney > originData.Stat_ProductionTime.cost[curLevel_StackCount])
        {
            // ��ư ��� ���׷��̵� ���·� �ٲٱ�
        }
        //Manager.player.Data.Money
    }
}

[Serializable]
public class ManufactureRuntimeData
{
    public int level_ProductionTime;
    public int level_StackCount;
    public int capacity;
    public float productionTime;

    public void SetCurLevelStatDatas(ManufactureBD manufactureBD)
    {
        this.capacity = manufactureBD.Stat_MaxStackableCount.Values[level_StackCount];
        this.productionTime = manufactureBD.Stat_ProductionTime.Values[level_ProductionTime];
    }
}