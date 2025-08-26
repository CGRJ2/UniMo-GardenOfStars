using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameQuest;
using System;
using System.Linq;

public class QuestManager : Singleton<QuestManager>
{
    // for test
    [SerializeField] private CYETestQuestDataSO[] _questDataList;
    [SerializeField] private CYETestQuestContentDataSO[] _questContentDataList;
    [SerializeField] private CYETestQuestProgressDataSO[] _questProgressDataList;


    public Quest[] _currentQuestList;
    // private int _currentQuestIndex;
    public ObservableProperty<int> CurrentQuestIndex = new();
    public int TargetCompleteCount { get { return _currentQuestList.Length; } }
    public event Action OnQuestProgressUpdate;
    public Quest CurrentQuest { get { return _currentQuestList[CurrentQuestIndex.Value]; } }

    private void Awake()
    {
        base.SingletonInit();
        Init();
    }

    private void Init()
    {
        // 초기화
    }

    private void Start()
    {
        SetQuestsOnNpc("test");
    }

    public void SetQuestsOnNpc(string npcId)
    {
        // DB에서 regionId를 통해 해당 지역의 퀘스트 목록을 가져온다
        CYETestQuestDataSO[] temp = Array.FindAll(_questDataList, item => item._npcId == npcId);
        Array.Sort(temp);
        InitQuestList(temp.Length);
        ConvertDataSOToClass(temp);

        CurrentQuestIndex.Value = GetCurrentQuestIndex();
    }

    private void InitQuestList(int listCount)
    {
        _currentQuestList = new Quest[listCount];
        // _currentQuestIndex = 0;
        // _targetCompleteCount = listCount;
    }

    private void ConvertDataSOToClass(CYETestQuestDataSO[] temp)
    {
        for (int idx = 0; idx < temp.Length; idx++)
        {
            _currentQuestList[idx]
             = new Quest(
                temp[idx],
                Array.FindAll(_questContentDataList, item => item._questId == temp[idx]._id),
                Array.FindAll(_questProgressDataList, item => item._questId == temp[idx]._id)
                );
        }
    }

    private int GetCurrentQuestIndex()
    {
        int questIdx = -1;
        for (int idx = 0; idx < _currentQuestList.Length; idx++)
        {
            if (_currentQuestList[idx]._questState == QuestState.InProgress)
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
                questIdx = _currentQuestList.Length - 1;
            }
        }
        return questIdx;
    }

    public void UpdateCurrentQuestProgress(string targetId, int count)
    {
        bool isUpdateSuccess = CurrentQuest.UpdateProgress(targetId, count);
        OnQuestProgressUpdate?.Invoke();
        if (CurrentQuest.CheckProgressComplete())
        {
            CurrentQuest.UpdateQuestState(QuestState.Completed);
            ChangeToNextQuest();
        }
        Debug.Log($"{isUpdateSuccess}");
    }

    private bool CheckQuestsCompleted()
    {
        bool isCompleted = true;
        foreach (Quest quest in _currentQuestList)
        {
            if (quest._questState != QuestState.Completed)
            {
                isCompleted = false;
                break;
            }
        }
        return isCompleted;
    }

    public void ChangeToNextQuest()
    {
        int nextQuestIndex = CurrentQuestIndex.Value + 1;
        if (nextQuestIndex != _currentQuestList.Length)
        {
            CurrentQuestIndex.Value = nextQuestIndex;
            CurrentQuest.AcceptQuest();
        }
        QuestEventBus.Publish(0, nextQuestIndex);
    }
}
