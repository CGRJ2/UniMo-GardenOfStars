using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameQuest
{
    /// <summary>
    /// 퀘스트 내부 클래스(MonoBehaviour 상속 안함)
    /// </summary>
    public class Quest
    {
        // 퀘스트 기본 데이터
        public QuestBaseData _data; // = new();
        
        // 퀘스트 진행도
        public List<QuestContentProgressData> _progresses = new();
        

        /// <summary>
        /// 퀘스트 수락(불필요시 삭제예정)
        /// </summary>
        public void AcceptQuest()
        {
            if (_data.State == QuestState.BeforeStart)
            {
                UpdateQuestState(QuestState.InProgress);
                // TO DO: _questProgresses의 항목들 상태를 진행중(InProgress)으로 변경해야함.
                // TO DO: _questProgresses에 해당하는 내용을 DB 업로드 해야함.
            }
        }

        /// <summary>
        /// 퀘스트의 상태를 업데이트합니다.
        /// </summary>
        /// <param name="nextState">변경하려는 상태</param>
        public void UpdateQuestState(QuestState nextState)
        {
            switch (_data.State)
            {
                case QuestState.BeforeStart:
                    if (nextState == QuestState.InProgress)
                    {
                        _data.UpdateQuestState(nextState);
                    }
                    else
                    { 
                        Debug.Log($"[Quest.cs] 업데이트하려는 상태값을 확인해주세요. => {nextState}");
                    }
                    break;
                case QuestState.InProgress:
                    if (nextState == QuestState.Completed)
                    {
                        _data.UpdateQuestState(nextState);
                    }
                    else
                    { 
                        Debug.Log($"[Quest.cs] 업데이트하려는 상태값을 확인해주세요. => {nextState}");
                    }
                    break;
                case QuestState.Completed:
                    Debug.Log($"[Quest.cs] 이미 완료된 퀘스트입니다.");
                    break;
                default:
                    Debug.LogWarning($"[Quest.cs] 비정상적인 업데이트입니다.");
                    break;
            }
        }

        
    }
}

