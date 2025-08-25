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
        public void UpdateQuestUI()
        {
            foreach (QuestProgressData item in Manager.quest.CurrentQuest._questProgresses)
            {
                // item에 해당하는 icon 및 갯수를 가져와서 업데이트
                Debug.Log($"{item._targetId}");
            }
        }
        /// <summary>
        /// 하나씩 업데이트
        /// </summary>
        /// <param name="targetId"></param>
        public void ReceiveEachProduct(string targetId)
        {
            // interact 발판에 있는 재료 정보를 들고와서
            // interact 발판에 있는 재료를 차감함
            Manager.quest.UpdateCurrentQuestProgress(targetId, 1);
        }
    }
}
