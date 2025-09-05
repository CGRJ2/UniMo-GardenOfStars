using System.Collections;
using System.Collections.Generic;
using GameQuest;
using UnityEngine;

namespace GameNpc
{
    public class Npc : MonoBehaviour
    {
        public int _id;
        public string _name;
        public string _description;
        public List<string> _hoveringSpeech = new();

        public Npc(CYETestNpcDataSO rawData)
        {
            this._id = rawData._id;
            this._name = rawData._name;
            this._description = rawData._description;
        }

        void Awake()
        {
            Init();
        }
        private void Init()
        {

        }

        /// <summary>
        /// 하나씩 업데이트
        /// </summary>
        /// <param name="targetId"></param>
        public void ReceiveEachProduct(string targetId)
        {
            Manager.quest.UpdateCurrentQuestProgress(targetId, 1);
        }

        public void Talk()
        { 
            
        }
    }
}
