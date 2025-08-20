using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInstance_Work : BuildingInstance
{
    [Header("���� �� �ִ� ��� ������")]
    public IngrediantSO insertableProdData; // �ӽ�

    [Header("����� ������")]
    public IngrediantSO resultProdData; // �ӽ�

    [Header("��Ḧ �׾Ƴ��� ��ġ")]
    public Transform attachPoint;

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
