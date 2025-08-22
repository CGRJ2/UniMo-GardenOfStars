using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInstance_Manufacture : BuildingInstance
{
    public BuildingRuntimeData runtimeData;
    [HideInInspector] public ManufactureBD originData;

    [Header("��Ḧ �׾Ƴ��� ��ġ")]
    public Transform attachPoint;

    [Header("��� ���� ��� ������")]
    public float insertDelayTime = 0.2f;

    [HideInInspector]
    public float progressedTime = 0f;   // ���� ���൵

    [Header("���� ���� ��ü")]
    public Interactable_Insert insertArea;
    [Header("�۾� ���� ��ü")]
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

