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
    private void Awake()
    {
        base.SingletonInit();
    }
   
    public void CurStageQuestDataInit() // 스테이지를 불러올 때 마다 호출
    {
        Debug.LogWarning("퀘스트 데이터 초기화 진행됨");
        Manager.firebase.UserData.CurStageData.Npc.QuestList.OnAdded?.RemoveListener(QuestDataInitEvent);
        Manager.firebase.UserData.CurStageData.Npc.QuestList.OnAdded.AddListener(QuestDataInitEvent);

        string npcDataId = Manager.data.Npc.Values.FirstOrDefault(item => item.Value.StageId == Manager.firebase.UserData.CurStage.Value).Key;

        // 현재 스테이지의 NPC가 보유한 퀘스트 데이터
        var curStageQuestDatas = Manager.data.Quest.Values.Where(item => item.Value.NpcId == npcDataId);
        //Debug.LogError($"curStageQuestDatas개수: {curStageQuestDatas.Count()}");

        foreach (var questDataKVP in curStageQuestDatas)
        {
            // 퀘스트 데이터 초기화
            QuestBaseData questData = Manager.firebase.UserData.CurStageData.Npc.QuestList.Get(questDataKVP.Key);
            if (questData == null)
            {
                //Manager.firebase.UserData.CurStageData.Npc.QuestList.OnAdded.AddListener(QuestDataInitEvent);
                Manager.firebase.UserData.CurStageData.Npc.QuestList.Add(questDataKVP.Key);
            }
        }
    }

    public void QuestDataInitEvent(QuestBaseData questBaseData)
    {
        //Debug.LogError($"{questBaseData.Id}, {questBaseData.QuestId}");
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
                var npc = Manager.firebase.UserData.CurStageData.Npc;
                var data = npc.QuestList.Get(QCDataKVP.Value.QuestId).QuestContentList.Get(QCSDataKVP.Value.QuestContentId);
                if (data == null)
                {
                    npc.QuestList.Get(QCDataKVP.Value.QuestId).QuestContentList.Add(QCSDataKVP.Value.QuestContentId);
                }
            }
        }
    }

    // 모든 Content의 클리어 여부 판단
    public void CheckCurQuestCleared()
    {
        var contentList = Manager.firebase.UserData.CurStageData.Npc.CurQuestData.QuestContentList.List;

        bool allContentCleard = true;
        foreach (var value in contentList)
        {
            if (value.StepIndexForClearContent >= value.ProgressdIndex.Value)
            {
                allContentCleard = false;
            }
        }

        // 모든 Content가 클리어된 상황이라면 => 퀘스트 클리어 판정
        if (allContentCleard)
        {
            Manager.firebase.UserData.CurStageData.Npc.CurQuestData.QuestState.Value = 3; // Completed
            Debug.LogWarning($"퀘스트(id: {Manager.firebase.UserData.CurStageData.Npc.CurrentQuestID.Value})의 모든 Content 클리어");

            // 현재 퀘스트 클리어 이벤트 (QuestState에 구독해두기)
            // 현재 퀘스트 Id에 대한 대화 이벤트 시작
            // 대화 이벤트 종료 후, 다음 퀘스트로 업데이트

            // 대화 했다 치고
            StartCoroutine(Temp_MoveToNextQuest());
        }
    }

    IEnumerator Temp_MoveToNextQuest()  // 임시 코루틴, 대화가 종료된 후 다음 퀘스트로 업데이트 되는걸 잠깐 딜레이되는 시간으로 표현함
    {
        int t = 0;
        while(t > 2)
        {
            yield return new WaitForSeconds(1f);
            t += 1;
        }
        MoveToNextQuest();
    }

    // 다음 순서의 퀘스트를 불러옴
    void MoveToNextQuest()
    {
        var npc = Manager.firebase.UserData.CurStageData.Npc;
        var questList = npc.QuestList.List;

        for (int i = 0; i < questList.Count; i++)
        {
            // 현재 퀘스트 Id의 순서 & 현재 퀘스트가 마지막 퀘스트가 아닌 경우에만 다음으로 이동
            if (npc.CurrentQuestID.Value == questList[i].Id && i < questList.Count - 1)
            {
                npc.CurrentQuestID.Value = questList[i + 1].Id;
                break;
            }
            // 마지막 스테이지가 완료되었다?
            else if (i == questList.Count - 1) { Debug.LogWarning("현재 스테이지의 모든 퀘스트를 완료함!"); }
        }
    }

    public int GetCurStageQuestIndex()
    {
        var questList = Manager.firebase.UserData.CurStageData.Npc.QuestList.List;
        for (int i = 0; i < questList.Count; i++)
        {
            // 현재 퀘스트의 순서
            if (Manager.firebase.UserData.CurStage.Value == questList[i].Id)
            {
                return i;
            }
        }
        return -99;
    }

    public int GetCurStageQuestCount()
    {
        var questList = Manager.firebase.UserData.CurStageData.Npc.QuestList.List;
        return questList.Count;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
