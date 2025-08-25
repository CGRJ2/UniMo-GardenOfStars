using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameQuest
{
    public class Quest //: MonoBehaviour
    {
        // 퀘스트 기본 데이터
        public QuestBaseData _baseData = new();
        // public QuestBaseData BaseData { get { return _baseData; } }
        // 퀘스트 진행도
        public List<QuestProgressData> _questProgresses = new();
        // public List<QuestProgressData> QuestProgresses { get { return _questProgresses; } }
        // 퀘스트 상태
        public QuestState _questState;
        // public QuestState QuestState { get { return _questState; } }

        public Quest(CYETestQuestDataSO rawData, CYETestQuestContentDataSO[] rawContentData, CYETestQuestProgressDataSO[] rawProgressData)
        {
            this._baseData._id = rawData._id;
            this._baseData._name = rawData._name;
            this._baseData._questType = rawData._questType;
            this._baseData._description = rawData._description;

            _questProgresses = this.InitProgress(rawContentData, rawProgressData);
        }

        /// <summary>
        /// 퀘스트 수락(불필요시 삭제예정)
        /// </summary>
        public void AcceptQuest()
        {
            if (_questState == QuestState.BeforeStart)
            {
                UpdateQuestState(QuestState.InProgress);
            }
        }

        /// <summary>
        /// 퀘스트의 상태를 업데이트합니다.
        /// </summary>
        /// <param name="nextState">변경하려는 상태</param>
        public void UpdateQuestState(QuestState nextState)
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
                    Debug.Log($"[Quest.cs] 이미 완료된 퀘스트입니다.");
                    break;
                default:
                    Debug.LogWarning($"[Quest.cs] 비정상적인 업데이트입니다.");
                    break;
            }
        }

        /// <summary>
        /// 퀘스트의 진행도를 업데이트합니다.
        /// </summary>
        /// <param name="targetId">업데이트하려는 항목의 목표 object ID 값</param>
        /// <param name="insertCount">업데이트하려는 항목의 갯수(숫자)</param>
        /// <returns>true=업데이트 완료, false=업데이트 실패</returns>
        public bool UpdateProgress(string targetId, int insertCount)
        {
            // if (_questState != QuestState.InProgress)
            // {
            //     Debug.LogWarning($"[Quest.cs] {targetId}에 해당하는 퀘스트 내용이 현재 진행중이 아닙니다.");
            //     return false;
            // }
            QuestProgressData _targetProgress = _questProgresses.Find(item => item._targetId == targetId);
            if (_targetProgress == null)
            {
                Debug.LogWarning($"[Quest.cs] {targetId}에 해당하는 퀘스트 내용이 없습니다.");
                return false;
            }
            // if (_targetProgress.CheckUpdatableCount(insertCount, out int checkedLeftover))
            {
                _targetProgress.UpdateCurrentCount(insertCount);
                if (CheckProgressComplete())
                {
                    UpdateQuestState(QuestState.Completed);
                }
                return true;
            }
            // return false;
        }

        /// <summary>
        /// 현재 퀘스트의 완료 여부를 확인합니다.
        /// </summary>
        /// <returns>true=완료, false=미완</returns>
        public bool CheckProgressComplete()
        {
            // _questProgresses에 QuestProgressState가 BeforeStart거나 Inprogress인 아이템이 없는가?
            return _questProgresses.Find(item => item._currentState == QuestProgressState.BeforeStart || item._currentState == QuestProgressState.InProgress) == null;
        }

        public List<QuestProgressData> InitProgress(CYETestQuestContentDataSO[] rawContentData, CYETestQuestProgressDataSO[] rawProgressData)
        {
            List<QuestProgressData> resultList = new();
            foreach (CYETestQuestContentDataSO contentItem in rawContentData)
            {
                resultList.Add(new QuestProgressData(contentItem, Array.Find(rawProgressData, item => item._targetId == contentItem._targetId)));
            }
            return resultList;
        }
    }
}

