using UnityEngine;
using UnityEngine.UI;

public class NpcInteractAreaUI : MonoBehaviour
{
    [SerializeField] Button btn_Talk;

    public void Init()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
        btn_Talk.onClick.AddListener(StartConversation);
    }

    public void StartConversation()
    {
        var stageID = Manager.firebase.UserData.CurStage.Value;
        var npc = Manager.firebase.UserData.CurStageData.Npc;

        if (stageID == "Tutorial")
        {
            Debug.LogWarning("튜토리얼 완료 대화 실행(업그레이드 패널 진입 용도)");
            Manager.dialogue.StartDialogueWithPanel(npc.NpcID.Value, stageID, $"Normal_TutoDialog_Upgrade");
            TutorialManager.Instance.handPointer_PlayerUpradeBtn.SetActive(true);
            return;
        }

        // 퀘스트가 클리어 상태라면
        if (npc.CurQuestData.QuestState.Value == 3)
        {
            Manager.dialogue.StartDialogueWithPanel(npc.NpcID.Value, stageID, $"Normal_{npc.NpcID.Value}_{npc.CurrentQuestID.Value}_Clear");
        }
        else
        {
            Manager.dialogue.StartDialogueWithPanel(npc.NpcID.Value, stageID, $"Normal_{npc.NpcID.Value}_{npc.CurrentQuestID.Value}");
        }
            
    }
}
