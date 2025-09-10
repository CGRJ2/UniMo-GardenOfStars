using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public FirebaseProperty<int> QuestState;

        public FirebaseDataList<QuestContentProgressData> QuestContentList;

        public QuestState State => (QuestState)QuestState.Value;
        #endregion

        #region 
        public QuestBaseData(string id, string parentPath = null) : base(id, parentPath)
        {
            QuestState = new FirebaseProperty<int>("QuestState", Path);

            // QuestState가 변할 때 구독하는 함수를 넣자
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

    public class QuestRewardData : FirebaseData
    {
        private QuestRewardDataCsv _questRewardCsv => Manager.data.QuestReward.Values[Id];
        public string QuestId => _questRewardCsv.QuestId;
        public string RewardId => _questRewardCsv.RewardId;
        public int RewardAmount => _questRewardCsv.RewardAmount;
        public FirebaseProperty<bool> IsProvidedProp;
        public bool IsProvided => IsProvidedProp.Value;

        public QuestRewardData(string id, string parentPath = null) : base(id, parentPath)
        {
            IsProvidedProp = new FirebaseProperty<bool>("IsProvided", Path);
            InitList.Add(IsProvidedProp);
        }
        public void UpdateProvidedState(bool isProvided)
        {
            IsProvidedProp.Value = isProvided;
        }
    }
}