using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameQuest
{
    /// <summary>
    /// 퀘스트 진행도 데이터 클래스
    /// </summary>
    public class QuestProgressData
    {
        public int _questId;
        // public Dictionary<string, int> QuestContents { get { return _questContents; } }
        public string _targetId;
        // public string TargetId { get { return _targetId; } }
        public int _targetCount;
        // public int TargetCount { get { return _targetCount; } }
        public int _currentCount;
        // public int CurrentCount { get { return _currentCount; } }
        public QuestProgressState _currentState;
        // public QuestProgressState CurrentState { get { return _currentState; } }

        /// <summary>
        /// 퀘스트 내용 데이터와 퀘스트 진행도 데이터를 조합하여 내부적으로 사용할 퀘스트 진행도 데이터를 만듭니다.
        /// </summary>
        /// <param name="rawContentData">퀘스트 내용 데이터</param>
        /// <param name="rawProgressData">퀘스트 진행도 데이터</param>
        public QuestProgressData(CYETestQuestContentDataSO rawContentData, CYETestQuestProgressDataSO rawProgressData = null)
        {
            this._questId = rawContentData._questId;
            this._targetId = rawContentData._targetId;
            this._targetCount = rawContentData._targetCount;
            if (rawProgressData == null)
            {
                this._currentCount = 0;
                this._currentState = QuestProgressState.BeforeStart;
            }
            else
            {
                this._currentCount = rawProgressData._currentCount;
                this._currentState = (rawContentData._targetCount == rawProgressData._currentCount) ? QuestProgressState.Completed : QuestProgressState.InProgress;
            }
            Debug.Log($"{nameof(QuestProgressData)} -> {_questId}/{_targetId}/{_currentState}");
        }

        /// <summary>
        /// 진행 수량 및 진행 상태를 업데이트합니다.
        /// </summary>
        /// <param name="addCount">진행 수량</param>
        public void UpdateData(int addCount)
        {
            if (addCount == 0 || _currentState == QuestProgressState.Completed)
            {
                // 진행도를 더할 수량이 0이거나 이미 완료된 상태면 업데이트하지 않음.
                Debug.Log($"[QuestProgressData.cs] {_targetId}를 업데이트할 수 없습니다.");
                return;
            }
            if (_currentCount == 0 && _currentState == QuestProgressState.BeforeStart)
            {
                // 만일 현재 수량이 0이면서 상태가 BeforeStart면 값 업데이트시 상태를 진행중(InProgress)으로 변경함.
                _currentState = QuestProgressState.InProgress;

                // TO DO: 이때 해당하는 퀘스트 진행도 상태 데이터를 업데이트해야함.(Firebase 연동)
            }
            _currentCount += addCount;
            if (_currentCount >= _targetCount)
            {
                // 만일 현재 수량이 목표 수량에 도달했을 경우 초과값을 삭제하고 상태를 완료(Complete)로 변경함.
                _currentCount = _targetCount;
                _currentState = QuestProgressState.Completed;
            }
        }
    }

}