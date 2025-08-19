using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameQuest
{
    public enum QuestState
    {
        // 수락전, 진행중, 완료됨
        BeforeAccept, InProgress, Completed
    }
    public class QuestProgress
    {
        private string _targetId;
        public string TargetId { get { return _targetId; } }
        private int _targetCount;
        public int TargetCount { get { return _targetCount; } }
        public int _currentCount;

        public QuestProgress(string targetId, int TargetCount)
        {
            this._targetId = targetId;
            this._targetCount = TargetCount;
            this._currentCount = 0;
        }
    }
    public class Quest : MonoBehaviour
    {
        private QuestDataSO _data;
        public QuestDataSO Data { get { return _data; } }
        private List<QuestProgress> _questProgresses;
        private QuestState _questState;
        public QuestState QuestState { get { return _questState; } }

        // 퀘스트 달성치를 올리는 기능
        // 퀘스트의 상태 업데이트
    }
}

