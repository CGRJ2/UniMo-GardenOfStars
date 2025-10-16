using System;
using System.Collections.Generic;
using UnityEngine;

public class ManufactureBuilding : BuildingInstance
{
    [HideInInspector] public ManufactureBD originData;
    [Header("일꾼이 바라볼 위치")]
    [SerializeField] private Transform _viewPoint;
    public Transform viewPoint { get => _viewPoint ?? transform; }

    [Header("재료를 쌓아놓을 위치")]
    public Transform attachPoint;

    [Header("재료 투입 & 회수 모션 딜레이")]
    public float insertDelayTime = 0.2f;

    [HideInInspector]
    public float progressedTime = 0f;   // 현재 진행도

    //[Header("투입 영역 객체")]
    [HideInInspector] public InsertArea insertArea;
    //[Header("작업 영역 객체")]
    [HideInInspector] public WorkArea workArea;
    [HideInInspector] public WorkArea_SwitchType workArea_SwitchType;
    //[Header("회수 영역 객체")]
    [HideInInspector] public ProdsArea prodsArea;

    public Stack<IngrediantInstance> ingrediantStack = new();
    //public Stack<IngrediantInstance> prodsStack = new();  // 회수영역을 스택처럼 표현할 때 사용하는걸로

    [SerializeField] GameObject _ProdAreaShowing;

    public float ProdTime // 현재 업그레이드 단계에 따른 [생산 속도]
    {
        get
        {
            UpgradeData upgradeData = Manager.buildings.GetUpgradeData(originData.ID);
            int level = upgradeData == null ? 0 : upgradeData.Level_ProdTime.Value;
            return originData.Stat_ProdTime.Values[level];
        }
    }

    public int Capacity // 현재 업그레이드 단계에 따른 [재료 최대 보유 개수]
    {
        get
        {
            UpgradeData upgradeData = Manager.buildings.GetUpgradeData(originData.ID);
            int level = upgradeData == null ? 0 : upgradeData.Level_Capacity.Value;
            return originData.Stat_Capacity.Values[level];
        }
    }

    private void Awake() => Init();
    
    public override void Init()
    {
        base.Init();
        if (Manager.data.Building.Values[ID] is ManufactureBD mfBD) originData = mfBD;

        activatePopUI ??= GetComponentInChildren<BuildingActivePopUI>();
        insertArea ??= GetComponentInChildren<InsertArea>();
        workArea_SwitchType = GetComponentInChildren<WorkArea_SwitchType>();
        prodsArea ??= GetComponentInChildren<ProdsArea>();

        activatePopUI.Init(this);
        insertArea.Init(this);
        workArea?.Init(this);
        workArea_SwitchType?.Init(this);
        prodsArea.Init(this);
    }

    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);
        if (characterRuntimeData is PlayerRunTimeData)
        {
            if (workArea_SwitchType.isWorkable)
                _ProdAreaShowing.SetActive(true);
        }
    }

    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);
        if (characterRuntimeData is PlayerRunTimeData)
        {
            _ProdAreaShowing.SetActive(false);
        }
    }

    public override void Stay(CharaterRuntimeData characterRuntimeData)
    {
        base.Stay(characterRuntimeData);

        if (characterRuntimeData is PlayerRunTimeData)
        {
            if (workArea_SwitchType.isWorkable)
                _ProdAreaShowing.SetActive(true);
            else
                _ProdAreaShowing.SetActive(false);
        }
    }
}
