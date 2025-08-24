using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManufactureBuilding : BuildingInstance
{
    public BuildingRuntimeData runtimeData;
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

