using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameQuest
{
    /// <summary>
    /// 퀘스트 진행도 데이터 클래스
    /// </summary>
    [Serializable]
    public class QuestContentProgressData : FirebaseData
    {
        private QuestContentDataCsv _questContentCsv => Manager.data.QuestContent.Values[Id];
        public string QuestId => _questContentCsv.QuestId;
        public string ContentTargetId => _questContentCsv.ContentTargetId;
        public int CurrentTargetCount => GetCurrentStepTargetCount();//_questContentCsv.ContentTargetCount;
        //public List<QuestContentStepData> _questContentStep = new();

        // ProgressdIndex가 CSV의 최대Index를 넘어갈 때 클리어 판정.
        public FirebaseProperty<int> ProgressdIndex;
        public FirebaseProperty<int> ProgressdProdsCount;

        public int Count => ProgressdProdsCount.Value;

        public bool IsContentClear => ProgressdIndex.Value > CurrentTargetCount;
        public bool IsStepClear => ProgressdProdsCount.Value >= CurrentTargetCount;

        /// <summary>
        /// 퀘스트 내용 데이터와 퀘스트 진행도 데이터를 조합하여 내부적으로 사용할 퀘스트 진행도 데이터를 만듭니다.
        /// </summary>
        /// <param name="rawContentData">퀘스트 내용 데이터</param>
        /// <param name="rawProgressData">퀘스트 진행도 데이터</param>
        public QuestContentProgressData(string id, string parentPath = null) : base(id, parentPath)
        {
            /*foreach (KeyValuePair<string, QuestContentStepDataCsv> i in Manager.data.QuestContentStep.Values)
            {
                if (i.Value.QuestContentId == Id)
                {
                    this._questContentStep.Add(new QuestContentStepData(i.Key));
                }
            }*/

            ProgressdProdsCount = new FirebaseProperty<int>("ProgressdCount", Path);
            InitList.Add(ProgressdProdsCount);

            ProgressdIndex = new FirebaseProperty<int>("ProgressIndex", Path);
            InitList.Add(ProgressdProdsCount);
        }

        /// <summary>
        /// 진행 수량 및 진행 상태를 업데이트합니다.
        /// </summary>
        /// <param name="addCount">진행 수량</param>
        public void UpdateData(int addCount)
        {
            if (addCount == 0 )
            {
                // 진행도를 더할 수량이 0이거나 이미 완료된 상태면 업데이트하지 않음.
                Debug.Log($"[QuestProgressData.cs] {ContentTargetId}를 업데이트할 수 없습니다.");
                return;
            }
            if (Count == 0)
            {
                // 만일 현재 수량이 0이면서 상태가 BeforeStart면 값 업데이트시 상태를 진행중(InProgress)으로 변경함.
                // State = QuestProgressState.InProgress;
                UpdateProgressState(QuestProgressState.InProgress);
            }

            // Count += addCount;
            UpdateProgressCount(addCount);

            if (Count == CurrentTargetCount)
            {
                // 만일 현재 수량이 목표 수량에 도달했을 경우 상태를 완료(Complete)로 변경함.
                // State = QuestProgressState.Completed;
                // GetCurrentContentStep().UpdateCompletedState(true);
                ProgressdProdsCount.Value = 0;
                // UpdateProgressState(QuestProgressState.Completed);
            }
            if (true/*GetCurrentContentStep() == null*/)
            { 
                UpdateProgressState(QuestProgressState.Completed);
            }
        }

        private void UpdateProgressCount(int addCount)
        {
            ProgressdProdsCount.Value = (ProgressdProdsCount.Value + addCount) >= CurrentTargetCount ? CurrentTargetCount : (ProgressdProdsCount.Value + addCount);
        }
        private void UpdateProgressState(QuestProgressState nextState)
        {
            //ProgressState.Value = (int)nextState;
        }
        public int GetCurrentStepTargetCount()
        {
            foreach (var kvp in Manager.data.QuestContentStep.Values)
            {
                if (kvp.Value.QuestContentId == Id && ProgressdIndex.Value == kvp.Value.ContentOrder)
                {
                    return kvp.Value.TargetAmount;
                }
            }
            return -1;
        }
        /*private QuestContentStepData GetCurrentContentStep()
        {
            foreach (QuestContentStepData item in _questContentStep)
            {
                if (!item.IsCompleted)
                {
                    return item;
                    // return item.TargetAmount;
                }
            }
            return null;
        }*/
    }
    

    public class QuestContentStepData : FirebaseData
    {
        private QuestContentStepDataCsv _questContentStepCsv => Manager.data.QuestContentStep.Values[Id];
        public string QuestContentId => _questContentStepCsv.QuestContentId;
        public int TargetAmount => _questContentStepCsv.TargetAmount;
        public string RewardId => _questContentStepCsv.RewardId;
        public int RewardAmount => _questContentStepCsv.RewardAmount;
        public int ContentOrder => _questContentStepCsv.ContentOrder;
        public FirebaseProperty<bool> IsCompletedProp;
        public bool IsCompleted => IsCompletedProp.Value;
        public FirebaseProperty<bool> IsProvidedProp;
        public bool IsProvided => IsProvidedProp.Value;

        public QuestContentStepData(string id, string parentPath = null) : base(id, parentPath)
        {
            IsCompletedProp = new FirebaseProperty<bool>("IsCompleted", Path);
            InitList.Add(IsCompletedProp);

            IsProvidedProp = new FirebaseProperty<bool>("IsProvided", Path);
            InitList.Add(IsProvidedProp);
        }

        public void UpdateCompletedState(bool isCompleted)
        {
            IsCompletedProp.Value = isCompleted;
        }
        public void UpdateProvidedState(bool isProvided)
        {
            IsProvidedProp.Value = isProvided;
        }
    }
}