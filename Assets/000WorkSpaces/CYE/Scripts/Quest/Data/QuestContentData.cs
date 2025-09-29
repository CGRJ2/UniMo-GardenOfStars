using KYS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameQuest
{
    /// <summary>
    /// 퀘스트 진행도 데이터 클래스
    /// </summary>
    [Serializable]
    public class QuestContentProgressData : FirebaseData
    {
        private QuestContentDataCsv _questContentCsv => Manager.data.QuestContent.Values[Id];
        public string QuestId => _questContentCsv.QuestId;
        public string ContentTargetId => _questContentCsv.ContentTargetId;
        public ContentStep[] ContentSteps => _questContentCsv.ContentSteps;
        public int CurrentTargetCount => GetCurrentStepRequireCount(); // Step 클리어를 위한 재료 개수
        public int StepIndexForClearContent => GetStepIndexForContentClear(); // Content 클리어를 위한 최대 단계의 Index

        // ProgressdIndex가 CSV의 최대Index를 넘어갈 때 클리어 판정.
        public FirebaseProperty<int> ProgressdIndex;
        public FirebaseProperty<int> ProgressdProdsCount;
        // 이벤트로 `Step 클리어`, `Content 클리어` 구분
        // `Step 클리어` 시, ProgressdIndex += 1, 램프 불빛 하나 추가, 보상 지급
        // `Content 클리어` 시, ProgressdIndex = 0, 해당 발판 `완료` 표기, 다른 퀘스트들 클리어 여부 판단,

        public bool IsContentClear => ProgressdIndex.Value > StepIndexForClearContent;

        public QuestContentProgressData(string id, string parentPath = null) : base(id, parentPath)
        {
            ProgressdProdsCount = new FirebaseProperty<int>("ProgressdCount", Path);
            ProgressdIndex = new FirebaseProperty<int>("ProgressIndex", Path);

            ProgressdIndex.Subscribe(CheckContentClear);
            ProgressdProdsCount.Subscribe(CheckStepClear);

            InitList.Add(ProgressdIndex);
            InitList.Add(ProgressdProdsCount);
        }

        void CheckContentClear(int progressIndex)
        {
            // 현재 Content 클리어 시 (모든 Step 클리어 완료)
            if (StepIndexForClearContent < progressIndex)
            {
                Debug.LogWarning($"QC(id:{Id}) 클리어");

                // 클리어 SFX 실행 
                // Manager.Audio.SfxPlay("SFX_QuestClear", Manager.player.PlayerObj.transform);

                bool questCleared;

                // 현재 퀘스트의 모든 Content가 Clear상태인지 체크
                Manager.quest.CheckCurQuestCleared(out questCleared);
            }
        }

        void CheckStepClear(int curProdsCount)
        {
            if (curProdsCount == 0) return;
            if (CurrentTargetCount <= curProdsCount)
            {
                // 스텝 클리어 이벤트 실행(보상, 이펙트)
                Debug.LogWarning("스텝 클리어, 보상 수령");
                
                // TO DO: 보상 수령
                switch (ContentSteps[ProgressdIndex.Value].RewardId)
                {
                    case "Coin":
                        Manager.firebase.UserData.Player.Money.Value += ContentSteps[ProgressdIndex.Value].RewardAmount;
                        break;
                    case "Gem":
                        Manager.firebase.UserData.Player.Gem.Value += ContentSteps[ProgressdIndex.Value].RewardAmount;
                        break;
                    default:
                        Debug.LogWarning($"[QuestManager] 보상 지급 실패. 올바르지 않은 보상 형식입니다.({ContentSteps[ProgressdIndex.Value].RewardId})");
                        break;
                }
                Debug.Log($"[QuestManager] {Manager.firebase.UserData.Player.Money.Value}");

                // 클리어 SFX 실행
                Manager.Audio.SfxPlay("SFX_QuestClear", Manager.player.PlayerObj.transform);

                ///.../// 스텝 완료 이펙트 종료 후에


                // 다음 Step 인덱스로 넘어가기 & 개수 정보 초기화
                ProgressdIndex.Value += 1;

                // 마지막 스텝인 경우 Value 안바꿔줌
                if (CurrentTargetCount > 0)  
                    ProgressdProdsCount.Value = 0;
            }
        }


        public int GetCurrentStepRequireCount()
        {
            // foreach (var kvp in Manager.data.QuestContentStep.Values)
            for (int cnt = 0; cnt < ContentSteps.Length; cnt++)
            {
                // if (kvp.Value.QuestContentId == Id && ProgressdIndex.Value == kvp.Value.ContentOrder)
                if (ProgressdIndex.Value == cnt)
                {
                    Debug.Log($"[QuestContentData] {ContentSteps[cnt].TargetAmount}");
                    return ContentSteps[cnt].TargetAmount;
                }
            }
            return -99;
        }
        
        public int GetStepIndexForContentClear()
        {
            int maxIndex = 0;
            // foreach (var kvp in Manager.data.QuestContentStep.Values)
            for (int cnt = 0; cnt < ContentSteps.Length; cnt++)
            {
                // if (kvp.Value.QuestContentId == Id)
                if (!ContentSteps[cnt].IsEmpty() && maxIndex < cnt)
                {
                    maxIndex = cnt;
                }
                Debug.Log($"[QuestContentData] maxIndex => {maxIndex}");
            }
            return maxIndex;
        }
    }
}