// System 
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
// Unity
using UnityEngine;
// Custom
using GameQuest;

/// <summary>
/// 현 스테이지의 퀘스트 목록 및 진행도를 관리하는 Singleton Class
/// </summary>
public class QuestManager : Singleton<QuestManager>
{
    #region >>> For Test Data(추후 교체 혹은 삭제 예정)
    [SerializeField] private CYETestQuestDataSO[] _questDataList;
    [SerializeField] private CYETestQuestContentDataSO[] _questContentDataList;
    [SerializeField] private CYETestQuestProgressDataSO[] _questProgressDataList;
    #endregion

    #region >>> Class Variables
    // 현재 퀘스트 목록
    public Quest[] _currentQuestList;
    // 현재 진행중인 퀘스트 index
    public ObservableProperty<int> CurrentQuestIndex = new();
    // 현재 진행중인 퀘스트
    public Quest CurrentQuest { get { return _currentQuestList[CurrentQuestIndex.Value]; } }
    // 현재 퀘스트 목록의 목표 완료 수치(*임시*: 모든 퀘스트를 완료하도록 지정하였으며, 추후 데이터를 통해 받아올 예정)
    public int TargetCompleteCount { get { return _currentQuestList.Length; } }
    #endregion

    #region >>> Class Events
    // 현재 진행중인 퀘스트의 진행도를 업데이트할 시 실행되는 event
    public event Action OnQuestProgressUpdate;
    #endregion

    // ===== ===== ===== ===== //

    #region >>> Unity Message Function
    private void Awake()
    {
        base.SingletonInit();
        Init();
    }
    private void Start()
    {
        SetQuestsOnNpc("test");
    }
    #endregion

    #region >>> Class Functions
    /// <summary>
    /// QuestManager Awake시 실행되는 최초 초기화 함수입니다.
    /// </summary>
    private void Init()
    {
        // 초기화
    }

    /// <summary>
    /// NpcId를 통해 해당 Npc에 해당하는 퀘스트 목록을 가져와 _currentQuestList에 지정합니다.
    /// </summary>
    /// <param name="npcId">현재 Npc의 Id</param>
    public void SetQuestsOnNpc(string npcId)
    {
        // TO DO: DB에서 Npc Id를 통해 해당 Npc의 퀘스트 목록을 가져와야 한다.
        CYETestQuestDataSO[] temp = Array.FindAll(_questDataList, item => item._npcId == npcId);
        // 퀘스트 리스트 초기화
        _currentQuestList = new Quest[temp.Length];
        // 퀘스트 데이터 정렬(퀘스트 진행 순서(_questOrder) 오름차순)
        Array.Sort(temp);
        // DataSo를 Quest array에 대입
        for (int idx = 0; idx < temp.Length; idx++)
        {
            _currentQuestList[idx]
             = new Quest(
                temp[idx],
                Array.FindAll(_questContentDataList, item => item._questId == temp[idx]._id),
                Array.FindAll(_questProgressDataList, item => item._questId == temp[idx]._id)
                );
        }
        // // DataSo를 Quest array에 대입
        // ConvertDataSOToClass(_questDataList);
        // 현재 진행중인 퀘스트 진행도의 index를 가져옴
        CurrentQuestIndex.Value = GetCurrentQuestIndex();
    }

    // /// <summary>
    // /// (*임시*) 현재 퀘스트 데이터 SO 목록
    // /// </summary>
    // /// <param name="temp"></param>
    // private void ConvertDataSOToClass(CYETestQuestDataSO[] rawDataList)
    // {
    //     CYETestQuestDataSO[] temp = Array.FindAll(_questDataList, item => item._npcId == npcId);
    //     // 퀘스트 리스트 초기화
    //     _currentQuestList = new Quest[temp.Length];
    //     // 퀘스트 데이터 정렬(퀘스트 진행 순서(_questOrder) 오름차순)
    //     Array.Sort(temp);

    //     for (int idx = 0; idx < temp.Length; idx++)
    //     {
    //         _currentQuestList[idx]
    //          = new Quest(
    //             temp[idx],
    //             Array.FindAll(_questContentDataList, item => item._questId == temp[idx]._id),
    //             Array.FindAll(_questProgressDataList, item => item._questId == temp[idx]._id)
    //             );
    //     }
    // }

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
    #endregion
}
