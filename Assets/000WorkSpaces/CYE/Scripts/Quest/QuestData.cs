using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameQuest
{
    /// <summary>
    /// 퀘스트 기본 데이터.
    /// </summary>
    public class QuestBaseData
    {
        #region 
        public int _id;
        // public int Id { get { return _id; } }
        public string _npcId;
        // public string NpcId { get { return _npcId; } }
        public int _questOrder;
        // public int QuestOrder { get { return _questOrder; } }
        public string _name;
        // public string Name { get { return _name; } }
        public QuestType _questType;
        // public QuestType QuestType { get { return _questType; } }
        public string _description;
        // public string Description { get { return _description; } }
        #endregion

        #region 
        /// <summary>
        /// 문자열 상태의 데이터를 변환하여 현재 클래스에 적용하는 함수.
        /// </summary>
        /// <param name="rawData">csv 형식의 문자열 데이터(한 줄)</param>
        public void Init(string rawData)
        {
            // csv => class
        }
        #endregion
    }
}