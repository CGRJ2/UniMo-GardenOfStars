using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : InteractableBase
{
    [SerializeField] protected PortalPopUI activatePopUI;

    private void Awake() => Init();
    protected void Init()
    {
        if (activatePopUI != null)
        {
            activatePopUI.Init();
        }
    }

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
