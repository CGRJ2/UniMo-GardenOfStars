using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameQuest;
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
            //yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.Npc.CurrentQuestID.IsInit);
            yield return new WaitForSeconds(2f);
            Init();
        }

        private void Init()
        {
            requireTiles = requireTilesParent.GetComponentsInChildren<QuestRequireTile>();
            UpdateQuestData();

            Manager.firebase.UserData.CurStageData.Npc.CurrentQuestID.Subscribe(UpdateQuestData);

        }

        public void UpdateQuestData(string questID = null)
        {
            // QC데이터가 있는 만큼만 발판 활성화
            var QCDataList = Manager.firebase.UserData.CurStageData.Npc.CurQuestData.QuestContentList.List;
            for (int i = 0; i < QCDataList.Count; i++)
            {
                requireTiles[i].gameObject.SetActive(true);

                requireTiles[i].QC_Data = QCDataList[i];
                requireTiles[i].Init();
            }
            if (QCDataList.Count < requireTiles.Length)
            {
                for (int i = QCDataList.Count; i < requireTiles.Length; i++)
                {
                    requireTiles[i].gameObject.SetActive(false);
                }
            }
    }


        public void Talk()
        {
            Debug.Log($"[NpcContoller] {nameof(Talk)} Call");
            // Dialogue 실행
            Manager.dialogue.StartDialogueWithPanel("npc001", "stage_01", "npc001_start");
        }

        public void Focus()
        {
            // 대사 출력
            // Debug.Log($"{_focusTextList[NpcUtil.GetRandomIndex(_focusTextList.Count)]}");
        }
    }
}
