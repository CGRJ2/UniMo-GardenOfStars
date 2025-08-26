using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameQuest
{
    public class QuestProgressData
    {
        public int _questId;
        // public Dictionary<string, int> QuestContents { get { return _questContents; } }
        public string _targetId;
        // public string TargetId { get { return _targetId; } }
        public int _targetCount;
        // public int TargetCount { get { return _targetCount; } }
        public int _currentCount;
        public QuestProgressState _currentState;
        // public QuestProgressState CurrentState { get { return _currentState; } }

        // public QuestProgressData(string targetId, int TargetCount)
        // {
        //     this._targetId = targetId;
        //     this._targetCount = TargetCount;
        //     this._currentCount = 0;
        // }
        public QuestProgressData(CYETestQuestContentDataSO rawContentData, CYETestQuestProgressDataSO rawProgressData)
        {
            this._targetId = rawContentData._targetId;
            this._targetCount = rawContentData._targetCount;
            this._currentCount = rawProgressData._currentCount;
            this._currentState = rawProgressData._currentState;
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
                // 만일 현재 수량이 0이면서 상태가 BeforeStart면 상태를 진행중(InProgress)으로 변경함.
                _currentState = QuestProgressState.InProgress;
            }
            _currentCount += addCount;
            if (_currentCount >= _targetCount)
            {
                // 만일 현재 수량이 목표 수량에 도달했을 경우 초과값을 삭제하고 상태를 완료(Complete)로 변경함.
                _currentCount = _targetCount;
                _currentState = QuestProgressState.Completed;
            }
        }
        
        // /// <summary>
        // /// 업데이트 가능한 수량인지 확인합니다.
        // /// </summary>
        // /// <param name="addCount">추가하려는 수량</param>
        // /// <param name="leftover">초과된 값(out)</param>
        // /// <returns>true=업데이트 가능, false=업데이트 불가</returns>
        // public bool CheckUpdatableCount(int addCount, out int leftover)
        // {
        //     if (_currentState != QuestProgressState.InProgress)
        //     {
        //         // 진행중이 아닌 항목은 수량을 업데이트할 수 없음.
        //         leftover = 0;
        //         return false;
        //     }
        //     leftover = _currentCount + addCount - _targetCount;
        //     return true;
        // }
    }

}