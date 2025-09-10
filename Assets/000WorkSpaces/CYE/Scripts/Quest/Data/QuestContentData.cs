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
        public int ContentTargetCount => _questContentCsv.ContentTargetCount;
        public FirebaseProperty<long> ProgressCount;
        public int Count => (int)ProgressCount.Value;
        public FirebaseProperty<long> ProgressState;
        public QuestProgressState State => (QuestProgressState)(int)ProgressState.Value;

        /// <summary>
        /// 퀘스트 내용 데이터와 퀘스트 진행도 데이터를 조합하여 내부적으로 사용할 퀘스트 진행도 데이터를 만듭니다.
        /// </summary>
        /// <param name="rawContentData">퀘스트 내용 데이터</param>
        /// <param name="rawProgressData">퀘스트 진행도 데이터</param>
        public QuestContentProgressData(string id, string parentPath = null) : base(id, parentPath)
        {
            ProgressCount = new FirebaseProperty<long>("ProgressCount", Path);
            InitList.Add(ProgressCount);

            ProgressState = new FirebaseProperty<long>("ProgressState", Path);
            InitList.Add(ProgressState);
        }

        /// <summary>
        /// 진행 수량 및 진행 상태를 업데이트합니다.
        /// </summary>
        /// <param name="addCount">진행 수량</param>
        public void UpdateData(int addCount)
        {
            if (addCount == 0 || State == QuestProgressState.Completed)
            {
                // 진행도를 더할 수량이 0이거나 이미 완료된 상태면 업데이트하지 않음.
                Debug.Log($"[QuestProgressData.cs] {ContentTargetId}를 업데이트할 수 없습니다.");
                return;
            }
            if (Count == 0 && State == QuestProgressState.BeforeStart)
            {
                // 만일 현재 수량이 0이면서 상태가 BeforeStart면 값 업데이트시 상태를 진행중(InProgress)으로 변경함.
                // State = QuestProgressState.InProgress;
                UpdateProgressState(QuestProgressState.InProgress);
            }

            // Count += addCount;
            UpdateProgressCount(addCount);

            if (Count == ContentTargetCount)
            {
                // 만일 현재 수량이 목표 수량에 도달했을 경우 상태를 완료(Complete)로 변경함.
                // State = QuestProgressState.Completed;
                UpdateProgressState(QuestProgressState.Completed);
            }
        }

        private void UpdateProgressCount(int addCount)
        {
            ProgressCount.Value = (ProgressCount.Value + addCount) >= ContentTargetCount ? ContentTargetCount : (ProgressCount.Value + addCount);
        }
        private void UpdateProgressState(QuestProgressState nextState)
        {
            ProgressState.Value = (int)nextState;
        }
    }
}