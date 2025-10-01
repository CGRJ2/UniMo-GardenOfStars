// Custom
using GameQuest;
using KYS;
// System 
using System;
using System.Linq;
// Unity
using UnityEngine;

/// <summary>
/// 현 스테이지의 퀘스트 목록 및 진행도를 관리하는 Singleton Class
/// </summary>
public class QuestManager : Singleton<QuestManager>
{
    public FirebaseDataList<QuestBaseData> CurrentQuestList => Manager.firebase.UserData.CurStageData.Npc.QuestList;
    public QuestBaseData CurrentQuest => Manager.firebase.UserData.CurStageData.Npc.CurQuestData;
    public Action QuestClearAction;

    private void Awake()
    {
        base.SingletonInit();
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
   
    public void CurStageQuestDataInit() // 스테이지를 불러올 때 마다 호출
    {
        if (CurrentQuestList == null)
        {
            Debug.LogWarning("[QuestManager] 현재 스테이지 퀘스트 목록 변수 선언 안됨.");
            return;
        }

        CurrentQuestList.OnAdded?.RemoveListener(QuestDataInitEvent);
        CurrentQuestList.OnAdded.AddListener(QuestDataInitEvent);

        string curStageID = Manager.firebase.UserData.CurStage.Value;
        string npcId = Manager.data.Stage.Values[curStageID].NpcID;
        Manager.firebase.UserData.CurStageData.Npc.NpcID.Value = npcId;

        // 현재 스테이지의 NPC가 보유한 퀘스트 데이터
        var curStageQuestDatas = Manager.data.Quest.Values.Where(item => item.Value.NpcId == npcId).OrderBy(i => i.Value.QuestOrder);
        foreach (var questDataKVP in curStageQuestDatas)
        {
            // 퀘스트 데이터 초기화
            QuestBaseData questData = CurrentQuestList.Get(questDataKVP.Key);
            if (questData == null)
            {
                CurrentQuestList.Add(questDataKVP.Key);
            }
        }
    }

    public void QuestDataInitEvent(QuestBaseData questBaseData)
    {
        // 현재 퀘스트 데이터의 QC들
        var QCParsedData = Manager.data.QuestContent.Values.Where(item => item.Value.QuestId == questBaseData.Id);
        foreach (var QCDataKVP in QCParsedData)
        {
            // Qc리스트 초기화
            var data = CurrentQuestList.Get(QCDataKVP.Value.QuestId).QuestContentList.Get(QCDataKVP.Value.Id);
            if (data == null)
            {
                CurrentQuestList.Get(QCDataKVP.Value.QuestId).QuestContentList.Add(QCDataKVP.Value.Id);
            }
        }
    }

    // 모든 Content의 클리어 여부 판단
    public void CheckCurQuestCleared(out bool isCleared)
    {
        bool allContentCleard = true;
        foreach (var value in CurrentQuest.QuestContentList.List)
        {
            if (value.StepIndexForClearContent >= value.ProgressdIndex.Value)
            {
                allContentCleard = false;
            }
        }

        // 모든 Content가 클리어된 상황이라면 => 퀘스트 클리어 판정
        if (allContentCleard)
        {
            var npcId = Manager.npc.CurStageNpc.NpcID;
            var curQuestId = Manager.npc.CurStageNpc.CurrentQuestID;
            
            isCleared = true;

            CurrentQuest.QuestState.Value = 2; // Completed

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

                // Manager.dialogue.StartDialogueWithPanel(npcId.Value, Manager.firebase.UserData.CurStage.Value, $"{npcId.Value}_{curQuestId.Value}");
                
                TutorialManager.Instance.tutorialNPC.HideQuestTiles();

                if (Manager.firebase.UserData.TutorialSequence.Value == 8) 
                    StartCoroutine(TutorialManager.Instance.TutoQuest03ClearCutScene());
                else
                    Manager.dialogue.StartDialogueWithPanel(npcId.Value, Manager.firebase.UserData.CurStage.Value, 
                        $"Quest_{npcId.Value}_{curQuestId.Value}");

                return;
            }

            // 현재 퀘스트 Id에 대한 대화 이벤트 시작
            // 대화 이벤트 종료 후, 다음 퀘스트로 업데이트
            Manager.dialogue.OnDialogueCompleted += SetNextQuestAfterDialogEnd;
            Manager.dialogue.StartDialogueWithPanel(npcId.Value, Manager.firebase.UserData.CurStage.Value, 
                $"Quest_{npcId.Value}_{curQuestId.Value}");
            Debug.LogError($"Quest_{npcId.Value}_{curQuestId.Value} 대화 진행");

        }
        else isCleared = false;
    }

    void SetNextQuestAfterDialogEnd(DialogueData dialogueData)
    {
        // // 튜토 퀘스트 대화 종료 시 마다 100원씩 보상으로 지급
        // if (Manager.firebase.UserData.CurStage.Value == "Tutorial")
        //     Manager.firebase.UserData.Player.Money.Value += 100;

        // TO DO: 보상 수령
        switch (CurrentQuest.RewardId)
        {
            case "Coin":
                Manager.firebase.UserData.Player.Money.Value += CurrentQuest.RewardAmount;
                break;
            case "Gem":
                Manager.firebase.UserData.Player.Gem.Value += CurrentQuest.RewardAmount;
                break;
            default:
                Debug.LogWarning($"[QuestManager] 보상 지급 실패. 올바르지 않은 보상 형식입니다.({nameof(CurrentQuest.RewardId)} => {CurrentQuest.RewardId})");
                break;
        }
        Debug.Log($"[QuestManager] {Manager.firebase.UserData.Player.Money}");

        MoveToNextQuest();
        CurrentQuest.QuestState.Value = 3; // TalkEnd
        Manager.dialogue.OnDialogueCompleted -= SetNextQuestAfterDialogEnd;
    }

    // 다음 순서의 퀘스트를 불러옴
    void MoveToNextQuest()
    {
        var curQuestId = Manager.npc.CurStageNpc.CurrentQuestID;
        var questList = CurrentQuestList.List;
        int stageClearTargetQuestIndex = Manager.data.Stage.Values[Manager.firebase.UserData.CurStage.Value].RequiredQuestIndex - 1;

        for (int i = 0; i < questList.Count; i++)
        {
            // 현재 퀘스트 Id의 순서 & 현재 퀘스트가 마지막 퀘스트가 아닌 경우에만 다음으로 이동
            if (curQuestId.Value == questList[i].Id && i < questList.Count - 1)
            {
                curQuestId.Value = questList[i + 1].Id;

                if (i == stageClearTargetQuestIndex)
                {
                    Debug.LogError("현재 스테이지의 목표 퀘스트까지 완료함! 다음 스테이지 언락");

                    foreach (var kvp in Manager.data.Stage.Values)
                    {
                        if (kvp.Value.Id == Manager.firebase.UserData.CurStage.Value)
                        {
                            Manager.firebase.UserData.StageList.Add(kvp.Value.NextStageId);
                            break;
                        }
                    }
                }
                break;
            }
            // 마지막 스테이지가 완료되었다
            else if (i == questList.Count - 1)
            {
                Debug.LogWarning("마지막 퀘스트까지 완료함!");
                break;
            }
        }
    }

    public int GetCurStageQuestIndex()
    {
        var questList = CurrentQuestList.List;
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
        return CurrentQuestList.List.Count;
    }
}
