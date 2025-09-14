using System.Collections;
using UnityEngine;

namespace GameNpc
{
    public class NpcController : MonoBehaviour
    {
        [SerializeField] Transform requireTilesParent;
        QuestRequireTile[] requireTiles;

        void Awake()
        {
            // 타이틀씬에서 시작 시
            //Init();


            // 스테이지씬에서 시작 시
            StartCoroutine(WaitAndInit());
        }

        IEnumerator WaitAndInit()
        {
            yield return new WaitUntil(() => Manager.firebase.IsFirebaseInit);
            yield return new WaitUntil(() => Manager.firebase.UserData != null);
            yield return new WaitUntil(() => Manager.firebase.UserData.IsInit);
            //Debug.LogWarning("UserData Inited");

            yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData != null);
            yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.IsInit);
            //Debug.LogWarning("CurStageData Inited");

            yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.Npc != null);
            //Debug.LogWarning("Npc Inited");

            var npc = Manager.firebase.UserData.CurStageData.Npc;
            //yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.Npc.CurrentQuestID.IsInit);

            yield return new WaitUntil(() => !string.IsNullOrEmpty(npc.CurrentQuestID.Value));
            //Debug.LogWarning("CurQuestID Inited");

            yield return new WaitUntil(() => npc.QuestList.IsInit); // <<<<<<=== Error
            //Debug.LogWarning("QuestList Inited");

            yield return new WaitUntil(() => npc.CurQuestData != null);
            //Debug.LogWarning("CurQuestData Inited");

            yield return new WaitUntil(() => npc.CurQuestData.QuestContentList.IsInit);
            //Debug.LogWarning("QuestContentList Inited");


            Init();
        }

        private void Init()
        {
            requireTiles = requireTilesParent.GetComponentsInChildren<QuestRequireTile>(true);

            if (Manager.firebase.UserData.CurStage.Value != "Tutorial")
            {
                UpdateQuestData();
            }
            else
            {
                TutorialManager.Instance.tutorialNPC = this;

                // 튜토리얼 진행도가 1 이상으로 저장되어있는 경우엔 퀘스트 발판 바로 띄우기
                if (TutorialManager.Instance.Sequence.Value > 1) UpdateQuestData();
            }

            Manager.firebase.UserData.CurStageData.Npc.CurrentQuestID.Subscribe(UpdateQuestData);
        }

        public void UpdateQuestData(string questID = null)
        {
            Debug.LogWarning($"퀘스트 발판 업데이트(현재 퀘스트ID : {Manager.firebase.UserData.CurStageData.Npc.CurrentQuestID.Value})");


            // QC데이터가 있는 만큼만 발판 활성화
            var QCDataList = Manager.firebase.UserData.CurStageData.Npc.CurQuestData.QuestContentList.List;
            for (int i = 0; i < QCDataList.Count; i++)
            {
                requireTiles[i].gameObject.SetActive(true);

                requireTiles[i].QC_Data = QCDataList[i];
                requireTiles[i].SetUp();
            }
            if (QCDataList.Count < requireTiles.Length)
            {
                for (int i = QCDataList.Count; i < requireTiles.Length; i++)
                {
                    requireTiles[i].gameObject.SetActive(false);
                }
            }
        }


        /*public void Talk()
        {
            //Debug.Log($"[NpcContoller] {nameof(Talk)} Call");
            // Dialogue 실행
            Manager.dialogue.StartDialogueWithPanel("npc001", "stage_01", "npc001_start");
        }*/

        public void Focus()
        {
            // 대사 출력
            // Debug.Log($"{_focusTextList[NpcUtil.GetRandomIndex(_focusTextList.Count)]}");
        }
    }
}
