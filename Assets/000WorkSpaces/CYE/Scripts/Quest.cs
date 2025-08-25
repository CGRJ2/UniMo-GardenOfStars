using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameQuest
{
    public enum QuestState
    {
        // 수락전, 진행중, 완료됨
        BeforeStart = 1, InProgress, Completed
    }
    public enum QuestProgressState
    {
        // 시작전, 진행중, 완료됨
        BeforeStart = 1, InProgress, Completed
    }
    public class QuestProgress
    {
        private string _targetId;
        public string TargetId { get { return _targetId; } }
        private int _targetCount;
        public int TargetCount { get { return _targetCount; } }
        public int _currentCount;
        private QuestProgressState _currentState;
        public QuestProgressState CurrentState { get { return _currentState; } }

        public QuestProgress(string targetId, int TargetCount)
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
    public class Quest //: MonoBehaviour
    {
        // 퀘스트 기본 데이터
        private QuestBaseData _baseData;
        public QuestBaseData BaseData { get { return _baseData; } }
        // 퀘스트 진행도
        private List<QuestProgress> _questProgresses;
        public List<QuestProgress> QuestProgresses { get { return _questProgresses; } }
        // 퀘스트 상태    
        private QuestState _questState;
        public QuestState QuestState { get { return _questState; } }

        public void AcceptQuest()
        {
            UpdateQuestState(QuestState.InProgress);
        }
        // 퀘스트 진행도 업데이트
        public bool TryUpdateProgress(string targetId, int insertCount, out int leftover)
        {
            if (_questState != QuestState.InProgress)
            {
                // 퀘스트가 진행중이 아니면 진행도를 업데이트하지 않음.
                leftover = 0;
                return false;
            }
            QuestProgress _targetProgress = _questProgresses.Find(item => item.TargetId == targetId);
            if (_targetProgress == null)
            {
                Debug.LogError($"[Quest.cs] {targetId}에 해당하는 퀘스트 내용이 없습니다.");
                leftover = 0;
                return false;
            }
            if (_targetProgress.CheckUpdatableCount(insertCount, out int checkedLeftover))
            {
                leftover = checkedLeftover;
                _targetProgress.UpdateCurrentCount(insertCount);
                // if (_targetProgress.CurrentState == QuestProgressState.Completed)
                // {
                //     UpdateQuestState(QuestState.Completed);
                // }
                if (CheckProgressComplete())
                { 
                    UpdateQuestState(QuestState.Completed);
                }
                return true;
            }
            leftover = 0;
            return false;
        }

        // 퀘스트의 상태 업데이트
        private void UpdateQuestState(QuestState nextState)
        {
            switch (_questState)
            {
                case QuestState.BeforeStart:
                    _questState = (nextState == QuestState.InProgress) ? QuestState.InProgress : _questState;
                    break;
                case QuestState.InProgress:
                    _questState = (nextState == QuestState.Completed) ? QuestState.Completed : _questState;
                    break;
                case QuestState.Completed:
                default:
                    // 이미 완료했거나 그 외의 상태일 경우 상태를 업데이트 하지 않음.
                    break;
            }
        }
        // 퀘스트 완료 여부 확인
        public bool CheckProgressComplete()
        {
            // bool isComplete = true;
            // isComplete = (_questProgresses.Find(item => item.CurrentState == QuestProgressState.BeforeStart || item.CurrentState == QuestProgressState.InProgress) == null);
            return _questProgresses.Find(item => item.CurrentState == QuestProgressState.BeforeStart || item.CurrentState == QuestProgressState.InProgress) == null;
        }
    }
}

