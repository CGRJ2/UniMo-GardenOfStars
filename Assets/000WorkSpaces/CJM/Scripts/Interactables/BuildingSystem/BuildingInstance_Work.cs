using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInstance_Work : BuildingInstance
{
    [Header("���� �� �ִ� ��� ������ �� ����")]
    public IngrediantSO insertableProdData; // �ӽ�
    public int maxStackableCount = 10;

    [Header("����� ������")]
    public IngrediantSO resultProdData; // �ӽ�

    [Header("��Ḧ �׾Ƴ��� ��ġ")]
    public Transform attachPoint;

    [Header("��� ���� ��� ������")]
    public float insertDelayTime = 0.2f;

    [Header("�۾��� �Ϸ�Ǵµ� �Ҹ�Ǵ� �ð�")]
    public float taskTime = 3f;
    [HideInInspector]
    public float progressedTime = 0f;

    [Header("���� ���� ��ü")]
    public Interactable_Insert insertArea;
    [Header("�۾� ���� ��ü")]
    public Interactable_Work workArea;



    public Stack<IngrediantInstance> prodsStack = new();


    private void Awake()
    {
        insertArea.Init(this);
        workArea.Init(this);
    }

}
