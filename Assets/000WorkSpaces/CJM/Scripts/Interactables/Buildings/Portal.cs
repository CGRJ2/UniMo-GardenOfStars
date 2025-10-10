using KYS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : InteractableBase
{
    [SerializeField] protected PortalPopUI activatePopUI;
    [SerializeField] WaitingTile interactTile;
    private void Awake() => Init();
    protected void Init()
    {
        if (activatePopUI != null)
        {
            activatePopUI.Init();
        }

        interactTile.WaitingCompletedAction = OpenStageTrastionPanel;
        interactTile.Init();

    }

    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);

        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            //if (activatePopUI != null)
            //    activatePopUI.gameObject.SetActive(true);  // 기본 상호작용 팝업 활성화 (존재 한다면)



        }
    }

    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);

        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            //if (activatePopUI != null)
                //activatePopUI.gameObject.SetActive(false); // 기본 상호작용 팝업 비활성화 (존재 한다면)
        }
    }


    // Stage Transition Panel 열기
    public void OpenStageTrastionPanel()
    {
        Debug.Log("부동산 패널 열기");

        if (UIManager.Instance == null)
        {
            Debug.LogError("[부동산 패널] UIManager.Instance가 null입니다!");
            return;
        }

        // 이미 부동산 패널이 열려있는지 확인
        var existingPanels = Manager.ui.GetUIsByLayer(UILayerType.Panel);
        foreach (var panel in existingPanels)
        {
            if (panel is StageTransitionPanel)
            {
                //Debug.Log("[부동산 패널] 이미 부동산 패널이 열려있습니다. 중복 호출 무시");
                return;
            }
        }

        // 부동산 패널 열기
        Manager.ui.ShowPanelAsync<StageTransitionPanel>((panel) =>
        {
            if (panel != null)
            {
                //Debug.Log("[부동산 패널] 부동산 패널 성공적으로 열림");

                /*if (buildingInstance is HarvestBuilding harvesst)
                    panel.SetUpgradeData(harvesst.originData);*/
            }
            else
            {
                //Debug.LogError("[부동산 패널]  열기 실패");
            }
        });
    }


}
