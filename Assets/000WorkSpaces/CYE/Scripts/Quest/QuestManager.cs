// System 
// Custom
using GameQuest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// Unity
using UnityEngine;

/// <summary>
/// 현 스테이지의 퀘스트 목록 및 진행도를 관리하는 Singleton Class
/// </summary>
public class QuestManager : Singleton<QuestManager>
{
    #region >>> For Test Data(추후 교체 혹은 삭제 예정)
    // [SerializeField] private CYETestQuestDataSO[] _questDataList;
    // [SerializeField] private CYETestQuestContentDataSO[] _questContentDataList;
    // [SerializeField] private CYETestQuestProgressDataSO[] _questProgressDataList;
    // /// <summary>
    // /// NpcId를 통해 해당 Npc에 해당하는 퀘스트 목록을 가져와 _currentQuestList에 지정합니다.
    // /// </summary>
    // /// <param name="npcId">현재 Npc의 Id</param>
    // public void SetQuestsOnNpc(string npcId)
    // {
    //     // TO DO: DB에서 Npc Id를 통해 해당 Npc의 퀘스트 목록을 가져와야 한다.
    //     CYETestQuestDataSO[] temp = Array.FindAll(_questDataList, item => item._npcId == npcId);
    //     // 퀘스트 리스트 초기화
    //     _currentQuestList = new Quest[temp.Length];
    //     // 퀘스트 데이터 정렬(퀘스트 진행 순서(_questOrder) 오름차순)
    //     Array.Sort(temp);
    //     // DataSo를 Quest array에 대입
    //     for (int idx = 0; idx < temp.Length; idx++)
    //     {
    //         _currentQuestList[idx]
    //          = new Quest(
    //             temp[idx],
    //             Array.FindAll(_questContentDataList, item => item._questId == temp[idx]._id),
    //             Array.FindAll(_questProgressDataList, item => item._questId == temp[idx]._id)
    //             );
    //     }
    //     // 현재 진행중인 퀘스트 진행도의 index를 가져옴
    //     CurrentQuestIndex.Value = GetCurrentQuestIndex();
    // }
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
    #endregion

    #region >>> Class Variables
    // 현재 지역 퀘스트 목록
    // public Quest[] _currentQuestList;
    public List<Quest> _currentQuestList = new();
    // 현재 진행중인 퀘스트 index
    public ObservableProperty<int> CurrentQuestIndex = new();
    // 현재 진행중인 퀘스트
    public Quest CurrentQuest => _currentQuestList[CurrentQuestIndex.Value];
    // 현재 퀘스트 목록의 목표 완료 수치(*임시*: 모든 퀘스트를 완료하도록 지정하였으며, 추후 데이터를 통해 받아올 예정)
    public int TargetCompleteCount => _currentQuestList.Count;
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
        //Init();

        StartCoroutine(WaitAndInit());
    }
    #endregion

    #region >>> Class Functions
    /// <summary>
    /// QuestManager Awake시 실행되는 최초 초기화 함수입니다.
    /// </summary>
    /// 

    IEnumerator WaitAndInit()
    {
        /*yield return new WaitUntil(() => Manager.firebase.IsFirebaseInit);
        yield return new WaitUntil(() => Manager.firebase.UserData != null);
        yield return new WaitUntil(() => Manager.firebase.UserData.IsInit);
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStage.IsInit);*/

        yield return new WaitForSeconds(3f);
        Init();
    }

    private void Init()
    {
        string npcDataId = Manager.data.Npc.Values.FirstOrDefault(item => item.Value.StageId == Manager.firebase.UserData.CurStage.Value).Key;

        // 현재 스테이지의 NPC가 보유한 퀘스트 데이터
        var curStageQuestDatas = Manager.data.Quest.Values.Where(item => item.Value.NpcId == npcDataId);
        Debug.LogError($"curStageQuestDatas개수: {curStageQuestDatas.Count()}");

        foreach (var questDataKVP in curStageQuestDatas)
        {
            // 퀘스트 데이터 초기화
            QuestBaseData questData = Manager.firebase.UserData.CurStageData.Npc.QuestList.Get(questDataKVP.Key);
            if (questData == null)
            {
                Manager.firebase.UserData.CurStageData.Npc.QuestList.OnAdded.AddListener(QuestDataInitEvent);
                Manager.firebase.UserData.CurStageData.Npc.QuestList.Add(questDataKVP.Key);
            }
        }
    }

    public void QuestDataInitEvent(QuestBaseData questBaseData)
    {
        Debug.LogError($"{questBaseData.Id}, {questBaseData.QuestId}");
        // 현재 퀘스트 데이터의 QC들
        var QCParsedData = Manager.data.QuestContent.Values.Where(item => item.Value.QuestId == questBaseData.Id);

        //Debug.LogError($"QCParsedData개수: {QCParsedData.Count()}");

        foreach (var QCDataKVP in QCParsedData)
        {
            // 현재 QC의 QCS들
            var QCSParsedData = Manager.data.QuestContentStep.Values.Where(item => item.Value.QuestContentId == QCDataKVP.Key);


            //Debug.LogError($"QCSParsedData개수: {QCSParsedData.Count()}");
            foreach (var QCSDataKVP in QCSParsedData)
            {
                // QCS 초기화

                var data = Manager.firebase.UserData.CurStageData.Npc.QuestList.Get(QCDataKVP.Value.QuestId).QuestContentList.Get(QCSDataKVP.Value.QuestContentId);
                if (data == null)
                {
                    Debug.LogError(333);
                    Manager.firebase.UserData.CurStageData.Npc.QuestList.Get(QCDataKVP.Value.QuestId).QuestContentList.Add(QCSDataKVP.Value.QuestContentId);
                }
            }
        }
    }


    public void SetQuestsOnRegion(string stageId)
    {
        string npcDataId = Manager.data.Npc.Values.FirstOrDefault(item => item.Value.StageId == stageId).Key;
        if (npcDataId == default)
        {
            Debug.LogWarning($"[QuestManager] 현재 지역 {stageId}의 Npc 데이터 Id를 가져오는데 실패하였습니다.");
            return;
        }
        var questData = Manager.data.Quest.Values.Where(item => item.Value.NpcId == npcDataId);
        foreach (var i in questData)
        {
            _currentQuestList.Add(new Quest(i.Key));
        }
    }


    /// <summary>
    /// 현재 진행중인 퀘스트의 index를 가져옵니다.
    /// </summary>
    /// <returns>현재 진행중인 퀘스트 index</returns>
    private int GetCurrentQuestIndex()
    {
        int questIdx = -1;
        for (int idx = 0; idx < _currentQuestList.Count; idx++)
        {
            if (_currentQuestList[idx]._data.State == QuestState.InProgress)
            {
                questIdx = idx;
                break;
            }
        }
        if (_currentQuestList.Count != 0 && questIdx < 0)
        {
            if (!CheckQuestsCompleted())
            {
                questIdx = 0;
                _currentQuestList[questIdx].AcceptQuest();
            }
            else
            {
                questIdx = _currentQuestList.Count - 1;
            }
        }
        return questIdx;
    }
    /// <summary>
    /// 현재 퀘스트의 진행도를 갱신합니다.
    /// </summary>
    /// <param name="targetId">갱신하려는 목표 데이터 id</param>
    /// <param name="count">갱신하려는 수치</param>
    public void UpdateCurrentQuestProgress(string targetId, int count)
    {
        // 퀘스트 진행도 갱신
        bool isUpdateSuccess = CurrentQuest.UpdateProgress(targetId, count);
        // 퀘스트 진행도 갱신시 발생하는 event 실행
        OnQuestProgressUpdate?.Invoke();


        // 현재 퀘스트의 진행도가 모두 완료되었다면
        if (true)
        {
            // 현재 퀘스트의 상태를 완료로 변경
            CurrentQuest.UpdateQuestState(QuestState.Completed);
            // 현재 퀘스트를 다음 퀘스트로 변경
            ChangeToNextQuest();
        }
    }

    /// <summary>
    /// 퀘스트 목록의 퀘스트가 전부 완료되었는지 확인합니다.
    /// </summary>
    /// <returns>true=완료, false=미완</returns>
    private bool CheckQuestsCompleted()
    {
        bool isCompleted = true;
        foreach (Quest quest in _currentQuestList)
        {
            if (quest._data.State != QuestState.Completed)
            {
                isCompleted = false;
                break;
            }
        }
        return isCompleted;
    }

    /// <summary>
    /// 현재 퀘스트를 다음 퀘스트로 변경합니다.
    /// </summary>
    public void ChangeToNextQuest()
    {
        // 다음 퀘스트 index를 가져옴
        int nextQuestIndex = CurrentQuestIndex.Value + 1;
        // 만일 다음 퀘스트 index가 오버되지 않았다면
        if (nextQuestIndex < _currentQuestList.Count)
        {
            // 현재 퀘스트의 index를 다음 퀘스트의 index로 지정
            CurrentQuestIndex.Value = nextQuestIndex;
            // 현재 퀘스트를 수락상태로 변경
            CurrentQuest.AcceptQuest();
        }
        // QuestEventBus를 통해 다음 퀘스트 index를 넘겨줌
        QuestEventBus.Publish(0, nextQuestIndex);
    }
    #endregion

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
