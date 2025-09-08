using System.Collections;
using System.Collections.Generic;
using GameQuest;
using UnityEngine;

namespace GameNpc
{
    public class Npc : MonoBehaviour
    {
        public string _id;
        public string _name;
        public string _description;
        public List<string> _focusTextList = new();

        public Npc(CYETestNpcDataSO rawData)
        {
            // this._id = rawData._id;
            // this._name = rawData._name;
            // this._description = rawData._description;
        }

        void Awake()
        {
            Init();
        }
        private void Init()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="addCount"></param>
        public void ReceiveProduct(string targetId, int addCount = 1)
        {
            Manager.quest.UpdateCurrentQuestProgress(targetId, addCount);
        }

        public void Talk()
        {
            Manager.dialogue.StartDialogueWithPanel("npc001", "stage_01", "npc001_start");
        }

        public void Focus()
        {
            Debug.Log($"{_focusTextList[NpcUtil.GetRandomIndex(_focusTextList.Count)]}");
        }
    }
}
