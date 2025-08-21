using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInstance_Work : BuildingInstance
{
    [Header("넣을 수 있는 재료 데이터 및 수량")]
    public IngrediantSO insertableProdData; // 임시
    public int maxStackableCount = 10;

    [Header("결과물 데이터")]
    public IngrediantSO resultProdData; // 임시

    [Header("재료를 쌓아놓을 위치")]
    public Transform attachPoint;

    [Header("재료 투입 모션 딜레이")]
    public float insertDelayTime = 0.2f;

    [Header("작업이 완료되는데 소모되는 시간")]
    public float taskTime = 3f;
    [HideInInspector]
    public float progressedTime = 0f;

    [Header("투입 영역 객체")]
    public Interactable_Insert insertArea;
    [Header("작업 영역 객체")]
    public Interactable_Work workArea;



    public Stack<IngrediantInstance> prodsStack = new();


    private void Awake()
    {
        insertArea.Init(this);
        workArea.Init(this);
    }

}
