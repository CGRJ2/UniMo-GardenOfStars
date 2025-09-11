using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameQuest
{
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

        public FirebaseProperty<int> QuestState;

        public FirebaseDataList<QuestContentProgressData> QuestContentList;

        public QuestState State => (QuestState)QuestState.Value;
        #endregion

        #region 
        public QuestBaseData(string id, string parentPath = null) : base(id, parentPath)
        {
            QuestState = new FirebaseProperty<int>("QuestState", Path);
            InitList.Add(QuestState);

            QuestContentList = new FirebaseDataList<QuestContentProgressData>("QcList", Path, (id, parentPath) =>
            {
                return new QuestContentProgressData(id, parentPath);
            });
            InitList.Add(QuestContentList);

        }
        #endregion

        public void UpdateQuestState(QuestState nextState)
        {
            QuestState.Value = (int)nextState;
        }
    }
}