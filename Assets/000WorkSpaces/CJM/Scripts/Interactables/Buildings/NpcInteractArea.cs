using KYS;
using UnityEngine;

public class NpcInteractArea : InteractableBase
{
    [SerializeField] protected NpcInteractAreaUI activatePopUI;

    private void Awake()
    {
        if (activatePopUI != null)
        {
            activatePopUI.Init();
            activatePopUI.gameObject.SetActive(false);
        }
    }

    protected override void OnDisableAdditionalActions()
    {
        if (activatePopUI != null)
            activatePopUI.gameObject.SetActive(false);
    }


    bool isTutoInteracted;
    // 활성화 범위 상호작용
    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);

        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            // 튜토리얼 NPC면 바로 첫대화 진행
            if (Manager.firebase.UserData.CurStage.Value == "Tutorial")
            {
                if (TutorialManager.Instance.Sequence.Value != 0) return; // 튜토 진행도는 Firebase에서 관리. 추후에 수정해야됨
                
                // 딱 한번만 실행되게
                if (isTutoInteracted) return;
                isTutoInteracted = true;

                var npc = Manager.firebase.UserData.CurStageData.Npc;
                Manager.dialogue.OnDialogueCompleted += TutorialManager.Instance.SequenceEnd; // 대화 완료 시, 시퀀스 00종료
                
                // 그냥 키를 넣었음
                Manager.dialogue.StartDialogueWithPanel(npc.NpcID.Value, "Tutorial", $"tutorial_game_01_001");
                // 해당 대화가 종료되면 콜백함수로 Sequence00 종료
                return;
            }

            if (activatePopUI != null)
                activatePopUI.gameObject.SetActive(true);  // 기본 상호작용 팝업 활성화
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
                activatePopUI.gameObject.SetActive(false); // 기본 상호작용 팝업 비활성화
        }
    }
}

