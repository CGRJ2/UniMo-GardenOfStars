using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameQuest;
using System;

public class QuestManager : Singleton<QuestManager>
{
    // for test
    [SerializeField] private CYETestQuestDataSO[] _curruntQuestDataList;
    private Quest[] _currentQuestList;
    private int _currentQuestIndex;
    // public int CurrentQuestIndex { get { return _currentQuestIndex; } } // 옵저버블프로퍼티
    public ObservableProperty<int> CurrentQuestIndex;
    // private int _targetCompleteCount;
    public int TargetCompleteCount { get { return _currentQuestList.Length; } }
    public event Action OnQuestProgressUpdate;
    public Quest CurrentQuest { get { return _currentQuestList[_currentQuestIndex]; } }

    private void Awake()
    {
        base.SingletonInit();
        Init();
    }

    private void Init()
    {
        // 초기화
        SetQuestsInRegion("temp");
    }

    public void SetQuestsInRegion(string regionId)
    {
        // DB에서 regionId를 통해 해당 지역의 퀘스트 목록을 가져온다
        // for test
        InitQuestList(_curruntQuestDataList.Length);
        ConvertDataSOToClass();
        // 가져온 목록을 "완료해야하는 순서대로 정렬 후" _currentQuestList에 지정한다

        // _currentQuestIndex = GetCurrentQuestIndex();
        CurrentQuestIndex.Value = GetCurrentQuestIndex();
    }

    private void InitQuestList(int listCount)
    {
        _currentQuestList = new Quest[listCount];
        // _currentQuestIndex = 0;
        // _targetCompleteCount = listCount;
    }

    private void ConvertDataSOToClass()
    {
        for (int idx = 0; idx < _curruntQuestDataList.Length; idx++)
        {
            _currentQuestList[idx] = new Quest(_curruntQuestDataList[idx]);
        }
    }

    private int GetCurrentQuestIndex()
    {
        int questIdx = -1;
        for (int idx = 0; idx < _currentQuestList.Length; idx++)
        {
            if (_currentQuestList[idx].QuestState == QuestState.InProgress)
            {
                questIdx = idx;
                break;
            }
        }
        if (_currentQuestList.Length != 0 && questIdx < 0)
        {
            if (!CheckQuestsCompleted())
            {
                questIdx = 0;
                _currentQuestList[questIdx].AcceptQuest();
            }
            else
            {
                questIdx = _currentQuestList.Length;
            }
        }
        return questIdx;
    }

    public void UpdateCurrentQuestProgress(string targetId, int count)
    {
        bool isUpdateSuccess = _currentQuestList[_currentQuestIndex].TryUpdateProgress(targetId, count);
        OnQuestProgressUpdate.Invoke();
        Debug.Log($"{isUpdateSuccess}");
    }

    private bool CheckQuestsCompleted()
    {
        bool isCompleted = true;
        foreach (Quest quest in _currentQuestList)
        {
            if (quest.QuestState != QuestState.Completed)
            {
                isCompleted = false;
                break;
            }
        }
        return isCompleted;
    }
}
