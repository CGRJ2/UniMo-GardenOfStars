// System 
// Custom
using GameQuest;
using KYS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// Unity
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 현 스테이지의 퀘스트 목록 및 진행도를 관리하는 Singleton Class
/// </summary>
public class QuestManager : Singleton<QuestManager>
{
    public Action QuestClearAction;

    private void Awake()
    {
        base.SingletonInit();
    }
   
    public void CurStageQuestDataInit() // 스테이지를 불러올 때 마다 호출
    {
        //Debug.LogWarning("퀘스트 데이터 초기화 진행됨");
        Manager.firebase.UserData.CurStageData.Npc.QuestList.OnAdded?.RemoveListener(QuestDataInitEvent);
        Manager.firebase.UserData.CurStageData.Npc.QuestList.OnAdded.AddListener(QuestDataInitEvent);

        string npcDataId = Manager.data.Npc.Values.FirstOrDefault(item => item.Value.StageId == Manager.firebase.UserData.CurStage.Value).Key;

        // 현재 스테이지의 NpcID 등록
        var npc = Manager.firebase.UserData.CurStageData.Npc;
        npc.NpcID.Value = npcDataId;

        // 현재 스테이지의 NPC가 보유한 퀘스트 데이터
        var curStageQuestDatas = Manager.data.Quest.Values.Where(item => item.Value.NpcId == npcDataId);
        //Debug.LogWarning($"curStageQuestDatas개수: {curStageQuestDatas.Count()}");

        foreach (var questDataKVP in curStageQuestDatas)
        {
            //Debug.LogWarning("QuestList 초기화 시작");
            // 퀘스트 데이터 초기화
            QuestBaseData questData = Manager.firebase.UserData.CurStageData.Npc.QuestList.Get(questDataKVP.Key);
            if (questData == null)
            {
                //Manager.firebase.UserData.CurStageData.Npc.QuestList.OnAdded.AddListener(QuestDataInitEvent);
                //Debug.LogError($"QuestData({questDataKVP.Key}) 추가");
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
            //Debug.LogError($"QuestContent 데이터 추가 실행");

            // Qc리스트 초기화
            var npc = Manager.firebase.UserData.CurStageData.Npc;
            var data = npc.QuestList.Get(QCDataKVP.Value.QuestId).QuestContentList.Get(QCDataKVP.Value.Id);
            if (data == null)
            {
                npc.QuestList.Get(QCDataKVP.Value.QuestId).QuestContentList.Add(QCDataKVP.Value.Id);
                //Debug.LogError($"QuestContent({QCDataKVP.Value.QuestId}/{QCDataKVP.Value.Id}) 추가");
            }
        }
    }

    // 모든 Content의 클리어 여부 판단
    public void CheckCurQuestCleared(out bool isCleared)
    {
        var npc = Manager.firebase.UserData.CurStageData.Npc;
        var contentList = npc.CurQuestData.QuestContentList.List;

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
            isCleared = true;

            npc.CurQuestData.QuestState.Value = 3; // Completed
            //Debug.LogWarning($"퀘스트(id: {npc.CurrentQuestID.Value})의 모든 Content 클리어");

            // 현재 퀘스트 클리어 이벤트 실행
            QuestClearAction?.Invoke();


            // 튜토리얼 스테이지의 퀘스트인 경우
            if (Manager.firebase.UserData.CurStage.Value == "Tutorial")
            {
                // 튜토 퀘01 = 시퀀스01 종료 -> 시퀀스02
                // 튜토 퀘02 = 시퀀스05 종료 -> 시퀀스06
                // 튜토 퀘03 = 시퀀스08 종료 -> 시퀀스09
                TutorialManager.Instance.arrows[2].SetActive(false);

                Manager.dialogue.OnDialogueCompleted += TutorialManager.Instance.SequenceEnd;
                Manager.dialogue.OnDialogueCompleted += SetNextQuestAfterDialogEnd;
                
                // 튜토 퀘스트 대화 종료 시 마다 100원씩 보상으로 지급
                Manager.dialogue.OnDialogueCompleted += (dialogData) => Manager.firebase.UserData.Player.Money.Value += 100;
                
                Manager.dialogue.StartDialogueWithPanel(npc.NpcID.Value, Manager.firebase.UserData.CurStage.Value, $"{npc.NpcID.Value}_{npc.CurrentQuestID.Value}");

                TutorialManager.Instance.tutorialNPC.HideQuestTiles();

                return;
            }

            // 현재 퀘스트 Id에 대한 대화 이벤트 시작
            // 대화 이벤트 종료 후, 다음 퀘스트로 업데이트
            Manager.dialogue.OnDialogueCompleted += SetNextQuestAfterDialogEnd;
            Manager.dialogue.StartDialogueWithPanel(npc.NpcID.Value, Manager.firebase.UserData.CurStage.Value, $"{npc.NpcID.Value}_{npc.CurrentQuestID.Value}");
        }
        else isCleared = false;
    }

    void SetNextQuestAfterDialogEnd(DialogueData dialogueData)
    {
        MoveToNextQuest();
        Manager.dialogue.OnDialogueCompleted -= SetNextQuestAfterDialogEnd;
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
            else if (i == questList.Count - 1) 
            {
                Debug.LogWarning("현재 스테이지의 모든 퀘스트를 완료함!");

                foreach(var kvp in Manager.data.Stage.Values)
                {
                    if (kvp.Value.Id == Manager.firebase.UserData.CurStage.Value)
                    {
                        Manager.firebase.UserData.StageList.Add(kvp.Value.NextStageId);
                        break;
                    }
                }
            }
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
