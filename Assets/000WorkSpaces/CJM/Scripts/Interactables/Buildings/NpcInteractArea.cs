using GameNpc;
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
            // 리팩토링 필요 => 전부 TutorialManager에서 처리할 수 있도록

            // 튜토리얼 NPC면 바로 첫대화 진행
            if (TutorialManager.Instance != null)
            {
                if (Manager.firebase.UserData.TutorialSequence.Value == 0)
                {
                    // 딱 한번만 실행되게
                    if (isTutoInteracted) return;
                    isTutoInteracted = true;

                    TutorialManager.Instance.arrows[0].SetActive(false);

                    var npc = Manager.firebase.UserData.CurStageData.Npc;
                    Manager.dialogue.OnDialogueCompleted += TutorialManager.Instance.SequenceEnd; // 대화 완료 시, 시퀀스 00종료

                    // 그냥 키를 넣었음
                    Manager.dialogue.StartDialogueWithPanel(npc.NpcID.Value, "Tutorial", $"Quest_{npc.NpcID.Value}_Start");
                    // 해당 대화가 종료되면 콜백함수로 Sequence00 종료
                    return;
                }
                else return;
            }

            else
            {
                var npc = Manager.firebase.UserData.CurStageData.Npc;
                string stageID = Manager.firebase.UserData.CurStage.Value;

                // 첫 대화 진행이 안된 경우
                if (!npc.IsTalked.Value)
                {
                    // 대화 실행 후, 대화 종료 시 퀘스트 발판 업데이트

                    Manager.dialogue.OnDialogueCompleted += FirstTalkInited;

                    Manager.dialogue.StartDialogueWithPanel(npc.NpcID.Value, stageID, $"Quest_{npc.NpcID.Value}_Start");
                }
                else
                {
                    if (activatePopUI != null)
                        activatePopUI.gameObject.SetActive(true);  // 기본 상호작용 팝업 활성화
                }
            }
        }
    }

    void FirstTalkInited(DialogueData data)
    {
        Manager.dialogue.OnDialogueCompleted -= FirstTalkInited;
        var npc = Manager.firebase.UserData.CurStageData.Npc;
        GetComponent<NpcController>().UpdateQuestData();
        npc.IsTalked.Value = true;
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

