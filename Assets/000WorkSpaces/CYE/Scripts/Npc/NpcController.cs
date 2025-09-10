using System.Collections;
using System.Collections.Generic;
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
            yield return new WaitForSeconds(1.5f);
            Init();
        }

        private void Init()
        {
            requireTiles = requireTilesParent.GetComponentsInChildren<QuestRequireTile>();

            for (int i =0; i< Manager.quest.CurrentQuest._progresses.Count; i++)
            {
                if (Manager.quest.CurrentQuest._progresses.Count > requireTiles.Length) 
                { Debug.LogError("퀘스트 조건 발판 개수보다 퀘스트 조건이 더 많음"); break; }

                requireTiles[i].requirement = Manager.quest.CurrentQuest._progresses[i];
                requireTiles[i].Init();
            }
        }

        /// <summary>
        /// 물품 납품
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="addCount"></param>
        public void ReceiveProduct(string targetId, int addCount = 1)
        {
            // 퀘스트 업데이트
            Manager.quest.UpdateCurrentQuestProgress(targetId, addCount);
        }

        /// <summary>
        /// 대화
        /// </summary>
        public void Talk()
        {
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
