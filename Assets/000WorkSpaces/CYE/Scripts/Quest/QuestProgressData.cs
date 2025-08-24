using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameQuest
{
    public class QuestProgressData
    {
        private string _targetId;
        public string TargetId { get { return _targetId; } }
        private int _targetCount;
        public int TargetCount { get { return _targetCount; } }
        public int _currentCount;
        private QuestProgressState _currentState;
        public QuestProgressState CurrentState { get { return _currentState; } }

        public QuestProgressData(string targetId, int TargetCount)
        {
            this._targetId = targetId;
            this._targetCount = TargetCount;
            this._currentCount = 0;
        }
        public void UpdateCurrentCount(int addCount)
        {
            if (_currentState != QuestProgressState.InProgress)
            {
                // 진행중이 아닌 항목은 수량을 업데이트 하지 않음
                return;
            }
            _currentCount += addCount;
            if (_currentCount >= _targetCount)
            {
                _currentCount = TargetCount;
                _currentState = QuestProgressState.Completed;
            }
        }
        public bool CheckUpdatableCount(int addCount, out int leftover)
        {
            if (_currentState != QuestProgressState.InProgress)
            {
                // 진행중이 아닌 항목은 수량을 업데이트할 수 없음.
                leftover = 0;
                return false;
            }
            leftover = (_currentCount + addCount) - _targetCount;
            return true;
        }
    }

}