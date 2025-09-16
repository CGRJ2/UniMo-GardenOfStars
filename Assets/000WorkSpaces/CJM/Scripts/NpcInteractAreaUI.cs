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
        //if ()
        //Manager.dialogue.StartDialogueWithPanel("npc001", "stage_01", "npc001_start");

        if (Manager.firebase.UserData.CurStage.Value == "Tutorial")
            TutorialManager.Instance.tutorialNPC.UpdateQuestData();
    }
}
