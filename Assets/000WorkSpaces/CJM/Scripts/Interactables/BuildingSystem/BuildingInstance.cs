using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInstance : InteractableBase
{
    [SerializeField] protected BuildingData _OriginData;           // CSV or Sheet로 변경 예정
    [SerializeField] protected BuildingActivePopUI activatePopUI;
    
    protected void BIBaseInit()
    {
        Manager.buildings.ActivatedBIList.Add(this);

        if (activatePopUI != null)
        {
            activatePopUI.GetComponent<Canvas>().worldCamera = Camera.main;
            activatePopUI.gameObject.SetActive(false);
        }
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        Manager.buildings?.ActivatedBIList.Remove(this);

        if (activatePopUI != null)
            activatePopUI.gameObject.SetActive(false);
    }

    // 건물 활성화 범위 상호작용
    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);
        
        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            if (activatePopUI != null)
                activatePopUI.gameObject.SetActive(true);  // 기본 상호작용 팝업 활성화 (존재 한다면)
        }
    }

    // 건물 활성화 범위 상호작용
    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);

        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            if (activatePopUI != null)
                activatePopUI.gameObject.SetActive(false); // 기본 상호작용 팝업 비활성화 (존재 한다면)
        }
    }

    
}


