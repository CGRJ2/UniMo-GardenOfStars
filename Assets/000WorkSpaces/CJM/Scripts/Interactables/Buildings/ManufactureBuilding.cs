using System;
using System.Collections.Generic;
using UnityEngine;

public class ManufactureBuilding : BuildingInstance
{
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

    [Header("작업 준비 시간")]
    public float prepareTime = 0.85f;


    public Stack<IngrediantInstance> ingrediantStack = new();
    //public Stack<IngrediantInstance> prodsStack = new();  // 회수영역을 스택처럼 표현할 때 사용하는걸로
    
    public float ProdTime // 현재 업그레이드 단계에 따른 [생산 속도]
    {
        get
        {
            UpgradeData upgradeData = Manager.buildings.GetUpgradeData(originData.ID);
            int level = upgradeData == null ? 0 : upgradeData.level_ProdTime;
            return originData.Stat_ProdTime.Values[level];
        }
    }

    public int Capacity // 현재 업그레이드 단계에 따른 [재료 최대 보유 개수]
    {
        get
        {
            UpgradeData upgradeData = Manager.buildings.GetUpgradeData(originData.ID);
            int level = upgradeData == null ? 0 : upgradeData.level_Capacity;
            return originData.Stat_Capacity.Values[level];
        }
    }

    private void Awake() => Init();
    
    public override void Init()
    {
        base.Init();

        if (_OriginData is ManufactureBD mfBD) originData = mfBD;
        activatePopUI.Init(this);

        insertArea.Init(this);
        workArea?.Init(this);
        workArea_SwitchType?.Init(this);
        prodsArea.Init(this);
    }
}
