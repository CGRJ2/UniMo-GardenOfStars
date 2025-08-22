using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInstance_Manufacture : BuildingInstance
{
    public BuildingRuntimeData runtimeData;
    [HideInInspector] public ManufactureBD originData;

    [Header("재료를 쌓아놓을 위치")]
    public Transform attachPoint;

    [Header("재료 투입 모션 딜레이")]
    public float insertDelayTime = 0.2f;

    [HideInInspector]
    public float progressedTime = 0f;   // 현재 진행도

    [Header("투입 영역 객체")]
    public Interactable_Insert insertArea;
    [Header("작업 영역 객체")]
    public Interactable_Work workArea;



    public Stack<IngrediantInstance> prodsStack = new();


    private void Awake()
    {
        InitRuntimeData();
        insertArea.Init(this);
        workArea.Init(this);
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

