using UnityEngine;
using System;

namespace KYS
{
    /// <summary>
    /// ChoicePanel 사용 예제 - 새로운 풀링 시스템과 SetActive(false) 기능 활용
    /// </summary>
    public class ChoicePanel_Usage_Examples : MonoBehaviour
    {
        [Header("ChoicePanel References")]
        [SerializeField] private ChoicePanel choicePanel;
        [SerializeField] private StoryPanel storyPanel;
        
        [Header("Test Settings")]
        [SerializeField] private string[] testChoices = new string[]
        {
            "첫 번째 선택지",
            "두 번째 선택지", 
            "세 번째 선택지",
            "네 번째 선택지"
        };
        
        private void Start()
        {
            // 컴포넌트 자동 찾기
            if (choicePanel == null)
                choicePanel = FindObjectOfType<ChoicePanel>();
            if (storyPanel == null)
                storyPanel = FindObjectOfType<StoryPanel>();
        }
        
        #region 기본 사용법 예제
        
        /// <summary>
        /// 기본 선택지 설정 예제
        /// </summary>
        [ContextMenu("테스트 - 기본 선택지 설정")]
        public void TestBasicChoiceSetup()
        {
            if (choicePanel != null)
            {
                choicePanel.SetupChoices(testChoices, OnChoiceSelected);
                Debug.Log("[ChoicePanel] 기본 선택지가 설정되었습니다.");
            }
        }
        
        /// <summary>
        /// StoryPanel을 통한 선택지 설정 예제
        /// </summary>
        [ContextMenu("테스트 - StoryPanel을 통한 선택지 설정")]
        public void TestStoryPanelChoiceSetup()
        {
            if (storyPanel != null)
            {
                storyPanel.SetupChoices(testChoices, OnChoiceSelected);
                Debug.Log("[StoryPanel] 선택지가 설정되었습니다.");
            }
        }
        
        /// <summary>
        /// 선택지 숨기기 예제 (SetActive(false) 사용)
        /// </summary>
        [ContextMenu("테스트 - 선택지 숨기기")]
        public void TestHideChoices()
        {
            if (choicePanel != null)
            {
                choicePanel.ClearChoices();
                Debug.Log("[ChoicePanel] 선택지가 숨겨졌습니다. (SetActive(false) 사용)");
            }
        }
        
        /// <summary>
        /// 선택지 다시 표시 예제
        /// </summary>
        [ContextMenu("테스트 - 선택지 다시 표시")]
        public void TestShowChoices()
        {
            if (choicePanel != null)
            {
                choicePanel.TestShowChoices();
            }
        }
        
        #endregion
        
        #region 고급 기능 예제
        
        /// <summary>
        /// 선택지 개별 제어 예제
        /// </summary>
        [ContextMenu("테스트 - 선택지 개별 제어")]
        public void TestIndividualChoiceControl()
        {
            if (choicePanel != null)
            {
                // 선택지 설정
                choicePanel.SetupChoices(testChoices, OnChoiceSelected);
                
                // 특정 선택지 비활성화
                choicePanel.SetChoiceButtonActive(1, false);
                Debug.Log("[ChoicePanel] 두 번째 선택지가 비활성화되었습니다.");
                
                // 선택지 상호작용 비활성화
                choicePanel.SetChoiceButtonsInteractable(false);
                Debug.Log("[ChoicePanel] 모든 선택지 상호작용이 비활성화되었습니다.");
            }
        }
        
        /// <summary>
        /// 선택지 상태 확인 예제
        /// </summary>
        [ContextMenu("테스트 - 선택지 상태 확인")]
        public void TestChoiceStatus()
        {
            if (choicePanel != null)
            {
                int choiceCount = choicePanel.GetActiveChoiceCount();
                bool hasChoices = choicePanel.HasActiveChoices();
                
                Debug.Log($"[ChoicePanel] 활성 선택지 개수: {choiceCount}");
                Debug.Log($"[ChoicePanel] 선택지 존재 여부: {hasChoices}");
                
                // 풀 상태도 확인
                choicePanel.CheckPoolStatus();
            }
        }
        
        /// <summary>
        /// 동적 선택지 변경 예제
        /// </summary>
        [ContextMenu("테스트 - 동적 선택지 변경")]
        public void TestDynamicChoiceChange()
        {
            if (choicePanel != null)
            {
                // 첫 번째 선택지 세트
                string[] firstChoices = { "A", "B", "C" };
                choicePanel.SetupChoices(firstChoices, OnChoiceSelected);
                Debug.Log("[ChoicePanel] 첫 번째 선택지 세트가 설정되었습니다.");
                
                // 2초 후 다른 선택지로 변경
                Invoke(nameof(ChangeToSecondChoiceSet), 2f);
            }
        }
        
        private void ChangeToSecondChoiceSet()
        {
            if (choicePanel != null)
            {
                string[] secondChoices = { "X", "Y", "Z" };
                choicePanel.SetupChoices(secondChoices, OnChoiceSelected);
                Debug.Log("[ChoicePanel] 두 번째 선택지 세트로 변경되었습니다.");
            }
        }
        
        #endregion
        
        #region 성능 최적화 예제
        
        /// <summary>
        /// 선택지 풀링 성능 테스트
        /// </summary>
        [ContextMenu("테스트 - 풀링 성능 테스트")]
        public void TestPoolingPerformance()
        {
            if (choicePanel != null)
            {
                Debug.Log("[ChoicePanel] 풀링 성능 테스트 시작...");
                
                // 여러 번 선택지 설정/해제 반복
                for (int i = 0; i < 10; i++)
                {
                    string[] choices = new string[UnityEngine.Random.Range(2, 5)];
                    for (int j = 0; j < choices.Length; j++)
                    {
                        choices[j] = $"선택지 {j + 1} (반복 {i + 1})";
                    }
                    
                    choicePanel.SetupChoices(choices, OnChoiceSelected);
                    choicePanel.ClearChoices();
                }
                
                Debug.Log("[ChoicePanel] 풀링 성능 테스트 완료. 풀 상태를 확인하세요.");
                choicePanel.CheckPoolStatus();
            }
        }
        
        /// <summary>
        /// 메모리 정리 예제 (완전 제거)
        /// </summary>
        [ContextMenu("테스트 - 메모리 정리")]
        public void TestMemoryCleanup()
        {
            if (choicePanel != null)
            {
                choicePanel.DestroyAllChoices();
                Debug.Log("[ChoicePanel] 모든 선택지가 완전히 제거되었습니다. (메모리 정리)");
            }
        }
        
        #endregion
        
        #region 통합 사용 예제
        
        /// <summary>
        /// 대화 시스템 통합 예제
        /// </summary>
        [ContextMenu("테스트 - 대화 시스템 통합")]
        public void TestDialogueSystemIntegration()
        {
            if (storyPanel != null)
            {
                // 스토리 모드로 시작
                storyPanel.SwitchToStoryMode();
                storyPanel.SetStoryText("안녕하세요! 무엇을 도와드릴까요?");
                
                // 3초 후 선택지 표시
                Invoke(nameof(ShowChoicesInDialogue), 3f);
            }
        }
        
        private void ShowChoicesInDialogue()
        {
            if (storyPanel != null)
            {
                storyPanel.SwitchToChoiceMode();
                storyPanel.SetChoiceQuestion("어떤 것을 원하시나요?");
                storyPanel.SetupChoices(testChoices, OnChoiceSelected);
                Debug.Log("[StoryPanel] 대화 중 선택지가 표시되었습니다.");
            }
        }
        
        /// <summary>
        /// 선택지 완료 후 처리 예제
        /// </summary>
        [ContextMenu("테스트 - 선택지 완료 처리")]
        public void TestChoiceCompletion()
        {
            if (storyPanel != null)
            {
                // 선택지 설정
                storyPanel.SetupChoices(testChoices, OnChoiceSelected);
                
                // 5초 후 자동으로 선택지 숨기기
                Invoke(nameof(AutoHideChoices), 5f);
            }
        }
        
        private void AutoHideChoices()
        {
            if (storyPanel != null)
            {
                storyPanel.HideChoices();
                storyPanel.SwitchToStoryMode();
                storyPanel.SetStoryText("선택이 완료되었습니다.");
                Debug.Log("[StoryPanel] 선택지가 자동으로 숨겨졌습니다.");
            }
        }
        
        #endregion
        
        #region 이벤트 핸들러
        
        /// <summary>
        /// 선택지 선택 처리
        /// </summary>
        private void OnChoiceSelected(int choiceIndex)
        {
            Debug.Log($"[ChoicePanel] 선택됨: {choiceIndex} - {testChoices[choiceIndex]}");
            
            // 선택에 따른 처리
            ProcessChoice(choiceIndex);
        }
        
        /// <summary>
        /// 선택지 처리 로직
        /// </summary>
        private void ProcessChoice(int choiceIndex)
        {
            switch (choiceIndex)
            {
                case 0:
                    Debug.Log("첫 번째 선택지 처리 중...");
                    break;
                case 1:
                    Debug.Log("두 번째 선택지 처리 중...");
                    break;
                case 2:
                    Debug.Log("세 번째 선택지 처리 중...");
                    break;
                case 3:
                    Debug.Log("네 번째 선택지 처리 중...");
                    break;
            }
            
            // 선택 완료 후 선택지 숨기기
            if (choicePanel != null)
            {
                choicePanel.ClearChoices();
            }
        }
        
        #endregion
        
        #region 유틸리티 메서드
        
        /// <summary>
        /// 모든 테스트 실행
        /// </summary>
        [ContextMenu("테스트 - 모든 기능 테스트")]
        public void RunAllTests()
        {
            Debug.Log("[ChoicePanel] 모든 기능 테스트를 시작합니다...");
            
            TestBasicChoiceSetup();
            Invoke(nameof(TestHideChoices), 1f);
            Invoke(nameof(TestShowChoices), 2f);
            Invoke(nameof(TestIndividualChoiceControl), 3f);
            Invoke(nameof(TestChoiceStatus), 4f);
            Invoke(nameof(TestDynamicChoiceChange), 5f);
            Invoke(nameof(TestPoolingPerformance), 7f);
            
            Debug.Log("[ChoicePanel] 모든 테스트가 예약되었습니다.");
        }
        
        /// <summary>
        /// 현재 상태 출력
        /// </summary>
        [ContextMenu("현재 상태 출력")]
        public void PrintCurrentStatus()
        {
            if (choicePanel != null)
            {
                choicePanel.PrintUIElementInfo();
            }
            
            if (storyPanel != null)
            {
                Debug.Log($"[StoryPanel] 현재 모드: {(storyPanel.IsChoiceMode() ? "선택지" : storyPanel.IsDialogueMode() ? "대화" : "스토리")}");
            }
        }
        
        #endregion
    }
}
