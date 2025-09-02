using UnityEngine;
using System;

namespace KYS.DialogueSystem
{
    /// <summary>
    /// 간단한 대화 컨트롤러 - UI 패널 전환만 담당
    /// </summary>
    public class SimpleDialogueController : MonoBehaviour
    {
        [Header("UI Panel References")]
        [SerializeField] private StoryPanel storyPanel;
        [SerializeField] private ChoicePanel choicePanel;
        
        [Header("Test Settings")]
        [SerializeField] private bool enableTestMode = true;
        
        private void Awake()
        {
            // 자동으로 패널들 찾기
            if (storyPanel == null)
                storyPanel = FindObjectOfType<StoryPanel>();
            
            if (choicePanel == null)
                choicePanel = FindObjectOfType<ChoicePanel>();
        }
        
        private void Start()
        {
            if (enableTestMode)
            {
                // 테스트 모드: 3초 후 대화 시작
                Invoke(nameof(StartTestDialogue), 3f);
            }
        }
        
        /// <summary>
        /// 대화 시작
        /// </summary>
        public void StartDialogue()
        {
            if (storyPanel != null)
            {
                storyPanel.SwitchToDialogueMode();
                storyPanel.Show();
                storyPanel.SetCharacterName(GetLocalizedText("npc_name"));
                storyPanel.SetStoryText(GetLocalizedText("dialogue_greeting"));
            }
            
            if (choicePanel != null)
            {
                choicePanel.Show();
                SetupTestChoices();
            }
        }
        
        /// <summary>
        /// 대화 종료
        /// </summary>
        public void EndDialogue()
        {
            if (storyPanel != null)
            {
                storyPanel.SwitchToStoryMode();
                storyPanel.Hide();
            }
            
            if (choicePanel != null)
            {
                choicePanel.Hide();
            }
        }
        
        /// <summary>
        /// 테스트 선택지 설정
        /// </summary>
        private void SetupTestChoices()
        {
            // 번역 키를 사용한 선택지
            string[] choiceKeys = new string[]
            {
                "choice_info_request",
                "choice_shop_purchase", 
                "choice_nothing_needed"
            };
            
            // 번역된 텍스트로 변환
            string[] choices = new string[choiceKeys.Length];
            for (int i = 0; i < choiceKeys.Length; i++)
            {
                choices[i] = GetLocalizedText(choiceKeys[i]);
            }
            
            choicePanel.SetupChoices(choices, OnChoiceSelected);
        }
        
        /// <summary>
        /// 로컬라이제이션된 텍스트 가져오기
        /// </summary>
        private string GetLocalizedText(string key)
        {
            // LocalizationManager를 통해 번역된 텍스트 가져오기
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetText(key);
            }
            
            // 폴백 텍스트
            return GetFallbackText(key);
        }
        
        /// <summary>
        /// 폴백 텍스트 반환
        /// </summary>
        private string GetFallbackText(string key)
        {
            switch (key)
            {
                case "choice_info_request":
                    return "정보를 얻고 싶어요";
                case "choice_shop_purchase":
                    return "아이템을 구매하고 싶어요";
                case "choice_nothing_needed":
                    return "아무것도 필요 없어요";
                case "dialogue_greeting":
                    return "안녕하세요! 무엇을 도와드릴까요?";
                case "dialogue_village_info":
                    return "이 마을은 평화로운 곳입니다. 많은 모험가들이 찾아오죠.";
                case "dialogue_shop_open":
                    return "상점을 열어드리겠습니다. (상점 UI 열기)";
                case "dialogue_goodbye":
                    return "알겠습니다. 언제든 다시 오세요!";
                case "dialogue_confused":
                    return "무슨 말씀이신지...";
                case "npc_name":
                    return "NPC";
                default:
                    return key;
            }
        }
        
        /// <summary>
        /// 선택지 선택 처리
        /// </summary>
        private void OnChoiceSelected(int choiceIndex)
        {
            Debug.Log($"[SimpleDialogueController] 선택됨: {choiceIndex}");
            
            // 선택에 따른 응답
            string response = GetResponseForChoice(choiceIndex);
            storyPanel.SetStoryText(response);
            
            // 2초 후 대화 종료
            Invoke(nameof(EndDialogue), 2f);
        }
        
        /// <summary>
        /// 선택에 따른 응답 반환
        /// </summary>
        private string GetResponseForChoice(int choiceIndex)
        {
            switch (choiceIndex)
            {
                case 0:
                    return GetLocalizedText("dialogue_village_info");
                case 1:
                    return GetLocalizedText("dialogue_shop_open");
                case 2:
                    return GetLocalizedText("dialogue_goodbye");
                default:
                    return GetLocalizedText("dialogue_confused");
            }
        }
        
        #region 테스트 메서드
        [ContextMenu("테스트 - 대화 시작")]
        public void StartTestDialogue()
        {
            StartDialogue();
        }
        
        [ContextMenu("테스트 - 대화 종료")]
        public void EndTestDialogue()
        {
            EndDialogue();
        }
        
        [ContextMenu("테스트 - 스토리 모드로 전환")]
        public void TestSwitchToStoryMode()
        {
            if (storyPanel != null)
            {
                storyPanel.SwitchToStoryMode();
                storyPanel.Show();
            }
        }
        
        [ContextMenu("테스트 - 대화 모드로 전환")]
        public void TestSwitchToDialogueMode()
        {
            if (storyPanel != null)
            {
                storyPanel.SwitchToDialogueMode();
                storyPanel.Show();
                storyPanel.SetCharacterName("테스트 캐릭터");
                storyPanel.SetStoryText("테스트 대화입니다.");
            }
        }
        #endregion
    }
}
