using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace KYS
{
    /// <summary>
    /// Addressables를 사용한 씬 로딩과 LoadingScreen을 연동하는 매니저
    /// </summary>
    public class AddressableSceneLoadingManager : MonoBehaviour
    {

        [ContextMenu("UIMnagerLoadingScreen Method 실행")]
        public void UIManagerLoadingScreen()
        {
            // LoadingScreen 테스트용 메서드
            UIManager.Instance.ShowLoadingScreenByKey("loading_data", "데이터를 불러오는 중...");

            UIManager.Instance.HideLoadingScreen();
        }


    


        [ContextMenu("대화 시스템 로드 테스트 tutorial_scn001")]
        public void Temp_DialogueSystemTest()
        {


            //Manager.dialogue.StartDialogueWithPanel("narration", "scn_01", "npc000_quest_Tuto01");
        }


        [ContextMenu("대화 시스템 로드 테스트 NPC002")]
        public void Temp_DialogueSystemTest2()
        {
            //Manager.dialogue.StartDialogueWithPanel("npc002", "stage_01", "npc000_quest_Tuto03");
        }

        [ContextMenu("대화 시스템 로드 테스트 NPC003")]
        public void Temp_DialogueSystemTest3()
        {
            Manager.dialogue.StartDialogueWithPanel("npc003", "stage_02");
        }

        [ContextMenu("대화 시스템 로드 테스트 NPC004")]
        public void Temp_DialogueSystemTest4()
        {
            Manager.dialogue.StartDialogueWithPanel("npc004", "", "npc004_start");
        }

        [ContextMenu("대화 시스템 로드 테스트 NPC001")]
        public void Temp_DialogueSystemTest5()
        {
            Manager.dialogue.StartDialogueWithPanel("npc001", "stage_01", "npc001_start");
        }



        [ContextMenu("ShowHUDUI 활용 기본 UI 활성화")]
        public void Temp_ShowHUDUI()
        {

            UIManager.Instance.ShowHUDUI<HUDAllPanel>();
        }

        [ContextMenu("ShowHUDUI 활용 Toturial UI 활성화")]
        public void Temp_ShowTutorialHUD()
        {
            // HUDAllPanel 활성화
            UIManager.Instance.ShowHUDUI<HUDAllPanel>();

            // HUDAllPanel 찾기 (GetHUDUI 대신 직접 찾기)
            HUDAllPanel hudAllPanel = UIManager.Instance.HUDCanvas.GetComponentInChildren<HUDAllPanel>();

            if (hudAllPanel != null)
            {
                hudAllPanel.SwitchToTutorialProgressMode();
            }
            else
            {
                Debug.LogError("[AddressableSceneLoadingManager] HUDAllPanel을 찾을 수 없습니다.");
            }
        }

        [ContextMenu("ShowHUDUI 활용 일반 UI 활성화")]
        public void Temp_HideHUDUI()
        {
            // HUDAllPanel 활성화
            UIManager.Instance.ShowHUDUI<HUDAllPanel>();

            // HUDAllPanel 찾기 (GetHUDUI 대신 직접 찾기)
            HUDAllPanel hudAllPanel = UIManager.Instance.HUDCanvas.GetComponentInChildren<HUDAllPanel>();

            if (hudAllPanel != null)
            {
                hudAllPanel.SwitchToNormalMode();
            }
            else
            {
                Debug.LogError("[AddressableSceneLoadingManager] HUDAllPanel을 찾을 수 없습니다.");
            }
        }



        [ContextMenu("CheckPopUp 메시지 주입 확인")]
        public void OnSaveButtonClicked()
        {
            Manager.ui.ShowConfirmPopUpAsync("저장하시겠습니까?", "저장", "취소",
                () =>
                {
                    Debug.Log("저장 실행");
                    // 저장 로직
                },
                () =>
                {
                    Debug.Log("저장 취소");
                });
        }

        [ContextMenu("MessagePopUp 메시지 주입 확인")]
        public void OnMessagePopUpTest()
        {
            // MessagePopUp 테스트용 메서드
            UIManager.Instance.ShowTutorialPopUpAsync("메시지 내용", () =>
            {
                Debug.Log("팝업이 닫혔습니다.");
            });


        }

        [ContextMenu("ShowPopUpAsync 메시지 키로 주입 확인")]
        public void OnMessagePopUpTest2()
        {
            // MessagePopUp 테스트용 메서드
            Manager.ui.ShowTutorialPopUpWithKeyAsync("stage_prepare", () =>
            {
                Debug.Log("이게 되네");
            });
        }

        [ContextMenu("ShowPanel PlayerUpgradePanel")]
        public void OnPlayerUpgradePanelTest()
        {
            // PlayerUpgradePanel 테스트용 메서드
            UIManager.Instance.ShowPanelAsync<PlayerUpgradePanel>();
        }


        [ContextMenu("ShowDialogue UpgradeButton")]
        public void OnDialogueUpgradeButtonTest()
        {
            // DialoguePanel의 UpgradeButton 테스트용 메서드
            Manager.dialogue.StartDialogueWithPanel("", "", "Test_upgrade");
        }
        [ContextMenu("ShowDialogue Test_upgrade_choice_1")]
        public void OnDialogueUpgradeChoice1Test()
        {
            // DialoguePanel의 UpgradeButton 테스트용 메서드
            Manager.dialogue.StartDialogueWithPanel("", "", "Test_upgrade_choice_1");
        }

        [ContextMenu("PropertyPanelCheck")]
        private void PropertyPanelCheck()
        {
            Manager.ui.ShowPanelAsync<PropertyPanel>();
        }

        [ContextMenu("HRRoomPanelCheck")]
        private void HRRoomPanelCheck()
        {
            Manager.ui.ShowPanelAsync<HRRoomPanel>();

        }

        [ContextMenu("PlayerUpgradeContentCheck")]
        private void PlayerUpgradeContentCheck()
        {
            Manager.ui.ShowPanelAsync<PlayerUpgradePanel>();
        }

        [ContextMenu("MoneyAdd")]
        private void MoneyAdd()
        {
            Manager.player.Data.Money.Value += 100000;
        }

        [ContextMenu("현재 퀘스트 인덱스 확인")]
        private void CheckCurrentQuestIndex()
        {
            if (Manager.firebase?.UserData?.CurStageData?.Npc?.CurQuestData != null)
            {
                string currentQuestId = Manager.firebase.UserData.CurStageData.Npc.CurQuestData.Id;
                int questIndex = ExtractQuestNumber(currentQuestId);
                
                Debug.Log($"[AddressableSceneLoadingManager] 현재 퀘스트 ID: {currentQuestId}");
                Debug.Log($"[AddressableSceneLoadingManager] 현재 퀘스트 인덱스: {questIndex}");
                
                // 스테이지 정보도 함께 출력
                if (Manager.firebase.UserData.CurStageData != null)
                {
                    Debug.Log($"[AddressableSceneLoadingManager] 현재 스테이지: {Manager.firebase.UserData.CurStageData.Id}");
                }
            }
            else
            {
                Debug.LogWarning("[AddressableSceneLoadingManager] 현재 퀘스트 데이터를 찾을 수 없습니다.");
            }
        }

        // 퀘스트 ID에서 숫자 추출하는 헬퍼 메서드
        private int ExtractQuestNumber(string questId)
        {
            if (string.IsNullOrEmpty(questId))
                return 0;
                
            string numberPart = questId.Replace("quest", "").TrimStart('0');
            return int.TryParse(numberPart, out int number) ? number : 0;
        }

        #region Quest Clear Context Menu

        [ContextMenu("현재 퀘스트 즉시 클리어")]
        public void ClearCurrentQuest()
        {
            if (Manager.quest == null)
            {
                Debug.LogError("[AddressableSceneLoadingManager] QuestManager를 찾을 수 없습니다.");
                return;
            }

            var currentQuest = Manager.quest.CurrentQuest;
            if (currentQuest == null)
            {
                Debug.LogWarning("[AddressableSceneLoadingManager] 현재 퀘스트가 없습니다.");
                return;
            }

            Debug.Log($"[AddressableSceneLoadingManager] 퀘스트 '{currentQuest.Id}' 즉시 클리어!");
            
            // 퀘스트의 모든 Content를 클리어 상태로 설정
            if (currentQuest.QuestContentList != null && currentQuest.QuestContentList.List != null)
            {
                foreach (var content in currentQuest.QuestContentList.List)
                {
                    try
                    {
                        // ProgressdIndex를 StepIndexForClearContent보다 크게 설정하여 클리어 상태로 만듦
                        int stepIndexForClear = content.StepIndexForClearContent;
                        // FirebaseProperty는 += 연산자로 안전하게 증가
                        int currentValue = content.ProgressdIndex.Value;
                        int targetValue = stepIndexForClear + 1;
                        content.ProgressdIndex.Value = targetValue;
                        Debug.Log($"  - Content '{content.Id}' 클리어 (Progress: {currentValue} -> {targetValue}/{stepIndexForClear})");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"  - Content '{content.Id}' 클리어 실패: {e.Message}");
                        // 안전하게 큰 값으로 설정 (FirebaseProperty 방식)
                        content.ProgressdIndex.Value = 10; // 999 대신 적당한 큰 값
                        Debug.Log($"  - Content '{content.Id}' 안전 모드로 클리어 (Progress: {content.ProgressdIndex.Value})");
                    }
                }
            }
            
            // 퀘스트 클리어 체크를 수동으로 실행
            bool isCleared;
            Manager.quest.CheckCurQuestCleared(out isCleared);
            
            if (isCleared)
            {
                Debug.Log("[AddressableSceneLoadingManager] 퀘스트 클리어 완료!");
            }
            else
            {
                Debug.LogWarning("[AddressableSceneLoadingManager] 퀘스트 클리어 체크 실패!");
            }
        }

        [ContextMenu("모든 퀘스트 즉시 클리어")]
        public void ClearAllQuests()
        {
            if (Manager.quest == null)
            {
                Debug.LogError("[AddressableSceneLoadingManager] QuestManager를 찾을 수 없습니다.");
                return;
            }

            var questList = Manager.quest.CurrentQuestList;
            if (questList == null || questList.List == null)
            {
                Debug.LogWarning("[AddressableSceneLoadingManager] 퀘스트 목록이 없습니다.");
                return;
            }

            Debug.Log($"[AddressableSceneLoadingManager] {questList.List.Count}개 퀘스트 즉시 클리어!");
            
            foreach (var quest in questList.List)
            {
                // 퀘스트의 모든 Content를 클리어 상태로 설정
                if (quest.QuestContentList != null && quest.QuestContentList.List != null)
                {
                    foreach (var content in quest.QuestContentList.List)
                    {
                        try
                        {
                            // ProgressdIndex를 StepIndexForClearContent보다 크게 설정하여 클리어 상태로 만듦
                            int stepIndexForClear = content.StepIndexForClearContent;
                            int targetValue = stepIndexForClear + 1;
                            content.ProgressdIndex.Value = targetValue;
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"  - Content '{content.Id}' 클리어 실패: {e.Message}");
                            // 안전하게 적당한 큰 값으로 설정
                            content.ProgressdIndex.Value = 10;
                        }
                    }
                }
                
                Debug.Log($"  - 퀘스트 '{quest.Id}' Content 클리어 완료");
            }
            
            // 현재 퀘스트 클리어 체크를 수동으로 실행
            bool isCleared;
            Manager.quest.CheckCurQuestCleared(out isCleared);
            
            if (isCleared)
            {
                Debug.Log("[AddressableSceneLoadingManager] 모든 퀘스트 클리어 완료!");
            }
            else
            {
                Debug.LogWarning("[AddressableSceneLoadingManager] 퀘스트 클리어 체크 실패!");
            }
        }

        [ContextMenu("대기 보상 번역 확인")]
        public void CheckWaitingRewardsTranslation()
        {

Manager.ui.ShowPopUpAsync<OfflineRewardPopup>();
        }

        #endregion
    }
}