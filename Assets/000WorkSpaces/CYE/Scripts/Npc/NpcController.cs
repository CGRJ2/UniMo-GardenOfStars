using System.Collections;
using System.Collections.Generic;
using GameQuest;
using UnityEngine;

namespace GameNpc
{
    public class NpcController : MonoBehaviour
    {
        
        void Awake()
        {
            Init();
        }
        private void Init()
        {

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
