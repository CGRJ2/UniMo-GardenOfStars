using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameQuest
{
    /// <summary>
    /// 퀘스트 기본 데이터 클래스
    /// </summary>
    public class QuestBaseData : FirebaseData
    {
        private QuestDataCsv _questCsv => Manager.data.Quest.Values[Id];
        #region         
        public string QuestId => _questCsv.Id;
        public string NpcId => _questCsv.NpcId;
        public int QuestOrder => _questCsv.QuestOrder;
        public string QuestName => _questCsv.QuestName;
        public QuestType QuestType => _questCsv.QuestType;
        public string Description => _questCsv.Description;

        public FirebaseProperty<long> QuestState;
        public QuestState State => (QuestState)(int)QuestState.Value;
        #endregion

        #region 
        public QuestBaseData(string id, string parentPath = null) : base(id, parentPath)
        {
            QuestState = new FirebaseProperty<long>("QuestState", Path);
            InitList.Add(QuestState);
        }
        #endregion

        public void UpdateQuestState(QuestState nextState)
        {
            QuestState.Value = (int)nextState;
        }
    }
}