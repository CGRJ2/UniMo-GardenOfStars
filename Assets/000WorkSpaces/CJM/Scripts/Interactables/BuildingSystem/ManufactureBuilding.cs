using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManufactureBuilding : BuildingInstance
{
    public BuildingRuntimeData runtimeData;
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
}

