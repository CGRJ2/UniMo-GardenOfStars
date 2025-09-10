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
            yield return new WaitForSeconds(3f);
            Init();
        }

        private void Init()
        {
            requireTiles = requireTilesParent.GetComponentsInChildren<QuestRequireTile>();
            UpdateQuestData();


            //var kvpList = Manager.data.QuestContent.Values.Where(item => item.Value.QuestId == )

            /* for (int i =0; i< Manager.quest.CurrentQuest._progresses.Count; i++)
             {
                 if (Manager.quest.CurrentQuest._progresses.Count > requireTiles.Length) 
                 { Debug.LogError("퀘스트 조건 발판 개수보다 퀘스트 조건이 더 많음"); break; }

                 requireTiles[i].requirement = Manager.quest.CurrentQuest._progresses[i];
                 requireTiles[i].Init();
             }*/
        }

        public void UpdateQuestData()
        {
            // QC데이터가 있는 만큼만 발판 활성화
            Debug.Log(Manager.firebase.UserData.CurStageData.Npc.CurQuestData);
            Debug.Log(Manager.firebase.UserData.CurStageData.Npc.CurQuestData.QuestContentList);
            Debug.Log(Manager.firebase.UserData.CurStageData.Npc.CurQuestData.QuestContentList.List);
            var QCDataList = Manager.firebase.UserData.CurStageData.Npc.CurQuestData.QuestContentList.List;
            for (int i = 0; i < QCDataList.Count; i++)
            {
                requireTiles[i].gameObject.SetActive(true);

                requireTiles[i].QC_Data = QCDataList[i];
                requireTiles[i].UpdateView();
            }
            if (QCDataList.Count < requireTiles.Length)
            {
                for (int i = QCDataList.Count; i < requireTiles.Length; i++)
                {
                    requireTiles[i].gameObject.SetActive(false);
                }
            }
    }


        /// <summary>
        /// 물품 납품
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="addCount"></param>
        public void ReceiveProduct(string targetId, int addCount = 1)
        {
            Debug.Log($"[NpcContoller] {nameof(ReceiveProduct)} Call");
            Manager.quest.UpdateCurrentQuestProgress(targetId, addCount);
        }

        /// <summary>
        /// 대화
        /// </summary>
        public void Talk()
        {
            Debug.Log($"[NpcContoller] {nameof(Talk)} Call");
            // Dialogue 실행
            Manager.dialogue.StartDialogueWithPanel("npc001", "stage_01", "npc001_start");
        }

        /// <summary>
        /// npc 포커스
        /// </summary>
        public void Focus()
        {
            // 대사 출력
            // Debug.Log($"{_focusTextList[NpcUtil.GetRandomIndex(_focusTextList.Count)]}");
        }
    }
}
