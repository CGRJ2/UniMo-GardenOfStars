using System;
using System.Collections.Generic;
using UnityEngine;

public class ManufactureBuilding : BuildingInstance
{
    public ManufactureRuntimeData runtimeData;
    [HideInInspector] public ManufactureBD originData;

    [Header("재료를 쌓아놓을 위치")]
    public Transform attachPoint;

    [Header("재료 투입 모션 딜레이")]
    public float insertDelayTime = 0.2f;

    [Header("회수 모션 딜레이")]
    public float prodsAbsorbDelayTime = 0.2f;

    [HideInInspector]
    public float progressedTime = 0f;   // 현재 진행도

    [Header("투입 영역 객체")]
    public InsertArea insertArea;
    [Header("작업 영역 객체")]
    public WorkArea workArea;
    public WorkArea_SwitchType workArea_SwitchType;
    [Header("회수 영역 객체")]
    public ProdsArea prodsArea;


    public Stack<IngrediantInstance> ingrediantStack = new();
    //public Stack<IngrediantInstance> prodsStack = new();  // 회수영역을 스택처럼 표현할 때 사용하는걸로

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

        // 두 스탯 중 업그레이드 비용이 충족될 때
        if (curMoney > originData.Stat_MaxStackableCount.cost[curLevel_StackCount] 
            || curMoney > originData.Stat_ProductionTime.cost[curLevel_StackCount])
        {
            // 버튼 모양 업그레이드 형태로 바꾸기
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