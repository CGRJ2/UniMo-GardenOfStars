using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class StoryPanel : BaseUI
    {
            [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
    [SerializeField] private string chatWindowAreaName = "ChatWindowArea";
    [SerializeField] private string choicePanelName = "ChoicePanel";
    [SerializeField] private string storyModePanelName = "StroyModePanel";
    [SerializeField] private string dialogueModePanelName = "DialogueModePanel";
    [SerializeField] private string storyTopAreaName = "StoryTopArea";

    // ChatWindowArea (NextButton 역할)
    private GameObject chatWindowArea => GetUI(chatWindowAreaName);
    
    // ChoicePanel 요소들
    private GameObject choicePanel => GetUI(choicePanelName);
    private Image choiceLCharacterImage => GetUI<Image>("LChoiceCharacterImage");
    private TextMeshProUGUI choiceRCharacterNameText => GetUI<TextMeshProUGUI>("RunRCharacterNameText");
    private Transform choiceContent => GetUI<Transform>("ChoiceContent");
    private TextMeshProUGUI choiceQuestionText => GetUI<TextMeshProUGUI>("RunChoiceQuestionText");
    
    // StoryModePanel 요소들
    private GameObject storyModePanel => GetUI(storyModePanelName);
    private TextMeshProUGUI storyLCharacterNameText => GetUI<TextMeshProUGUI>("RunLStroyCaracterNameText");
    private Image storyRCharacterImage => GetUI<Image>("RStroyCharacterImage");
    private GameObject storyBottomArrow => GetUI("BottomArrow");
    private TextMeshProUGUI storyChatText => GetUI<TextMeshProUGUI>("RunStroyChatWindowText");
    
    // DialogueModePanel 요소들
    private GameObject dialogueModePanel => GetUI(dialogueModePanelName);
        private GameObject RDialoguCharacternameArea => GetUI("RDialogueCharacterNameArea");
        private TextMeshProUGUI dialogueRCharacterNameText => GetUI<TextMeshProUGUI>("RunRDialogueCharacterNameText");
    private Image dialogueLCharacterImage => GetUI<Image>("LDialogueCharacterImage");
        private GameObject LDialoguCharacternameArea => GetUI("LDialogueCharacterNameArea");
        private TextMeshProUGUI dialogueLCharacterNameText => GetUI<TextMeshProUGUI>("RunLDialogueCaracterNameText");
    private Image dialogueRCharacterImage => GetUI<Image>("RDialogueCharacterImage");
    private GameObject dialogueBottomArrow => GetUI("BottomArrow");
    private TextMeshProUGUI dialogueChatText => GetUI<TextMeshProUGUI>("RunDialogueChatWindowText");
    
    // StoryTopArea 요소들
    private GameObject storyTopArea => GetUI(storyTopAreaName);
    private Button logButton => GetUI<Button>("LogButton");
    private Button skipButton => GetUI<Button>("SkipButton");

    [Header("Story Settings")]
    [SerializeField] private string[] storyPages = new string[0];
    [SerializeField] private int currentPage = 0;

        protected override void Awake()
        {
            base.Awake();
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_story_title",
                "ui_choice_question",
                "ui_character_name_default",
                "ui_story_text_default",
                "ui_dialogue_text_default",
                "ui_log_button",
                "ui_skip_button"
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
            UpdateUI();
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        private void SetupButtons()
        {
            // ChatWindowArea 전체가 NextButton 역할
            var chatAreaHandler = GetEventWithSFX(chatWindowAreaName, "SFX_ButtonClick");
            if (chatAreaHandler != null)
            {
                chatAreaHandler.Click += (data) => OnChatWindowClicked();
            }

            // 로그 버튼
            if (logButton != null)
            {
                var logHandler = GetEventWithSFX("LogButton", "SFX_ButtonClick");
                if (logHandler != null)
                {
                    logHandler.Click += (data) => OnLogButtonClicked();
                }
            }

            // 스킵 버튼
            if (skipButton != null)
            {
                var skipHandler = GetEventWithSFX("SkipButton", "SFX_ButtonClick");
                if (skipHandler != null)
                {
                    skipHandler.Click += (data) => OnSkipButtonClicked();
                }
            }
        }

        private void UpdateUI()
        {
            // 현재 모드에 따라 UI 업데이트
            if (IsStoryMode())
            {
                UpdateStoryModeUI();
            }
            else if (IsDialogueMode())
            {
                UpdateDialogueModeUI();
            }
        }

        private void UpdateStoryModeUI()
        {
            if (storyChatText != null && storyPages.Length > 0)
                storyChatText.text = storyPages[currentPage];

            if (storyBottomArrow != null)
                storyBottomArrow.SetActive(currentPage < storyPages.Length - 1);
        }

        private void UpdateDialogueModeUI()
        {
            if (dialogueChatText != null && storyPages.Length > 0)
                dialogueChatText.text = storyPages[currentPage];

            if (dialogueBottomArrow != null)
                dialogueBottomArrow.SetActive(currentPage < storyPages.Length - 1);
        }

        public void SetStoryData(string[] pages)
        {
            storyPages = pages;
            currentPage = 0;
            UpdateUI();
        }



        private void NextPage()
        {
            if (currentPage < storyPages.Length - 1)
            {
                currentPage++;
                UpdateUI();
                Debug.Log($"[StoryPanel] 다음 페이지로 이동: {currentPage + 1}");
            }
        }

        private void OnChatWindowClicked()
        {
            NextPage();
        }

        private void OnLogButtonClicked()
        {
            Debug.Log("[StoryPanel] 로그 버튼 클릭");
            // 로그 기능 구현
        }

        private void OnSkipButtonClicked()
        {
            Debug.Log("[StoryPanel] 스킵 버튼 클릭");
            // 스킵 기능 구현
        }

        #region 모드 전환 및 UI 제어
        
        /// <summary>
        /// 대화 모드로 전환 (왼쪽 캐릭터가 말하는 경우)
        /// </summary>
        [ContextMenu("대화 모드로 전환 (왼쪽 캐릭터)")]
        public void SwitchToDialogueModeLeft()
        {
            if (dialogueModePanel != null)
                dialogueModePanel.SetActive(true);
            if (storyModePanel != null)
                storyModePanel.SetActive(false);
            if (choicePanel != null)
                choicePanel.SetActive(false);
            
            // 왼쪽 캐릭터 이미지와 오른쪽 캐릭터 이름 활성화
            if (dialogueLCharacterImage != null)
                dialogueLCharacterImage.gameObject.SetActive(false);
            if (dialogueRCharacterImage != null)
                dialogueRCharacterImage.gameObject.SetActive(true);
            if (RDialoguCharacternameArea != null)
                RDialoguCharacternameArea.SetActive(false);
            if (LDialoguCharacternameArea != null)
                LDialoguCharacternameArea.SetActive(true);
        }

        /// <summary>
        /// 대화 모드로 전환 (오른쪽 캐릭터가 말하는 경우)
        /// </summary>
        [ContextMenu("대화 모드로 전환 (오른쪽 캐릭터)")]
        public void SwitchToDialogueModeRight()
        {
            if (dialogueModePanel != null)
                dialogueModePanel.SetActive(true);
            if (storyModePanel != null)
                storyModePanel.SetActive(false);
            if (choicePanel != null)
                choicePanel.SetActive(false);
            
            // 오른쪽 캐릭터 이미지와 왼쪽 캐릭터 이름 활성화
            if (dialogueLCharacterImage != null)
                dialogueLCharacterImage.gameObject.SetActive(true);
            if (dialogueRCharacterImage != null)
                dialogueRCharacterImage.gameObject.SetActive(false);
            if (RDialoguCharacternameArea != null)
                RDialoguCharacternameArea.gameObject.SetActive(true);
            if (LDialoguCharacternameArea != null)
                LDialoguCharacternameArea.SetActive(false);
        }

        /// <summary>
        /// 대화 모드로 전환 (기본 - 왼쪽 캐릭터)
        /// </summary>
        [ContextMenu("대화 모드로 전환")]
        public void SwitchToDialogueMode()
        {
            SwitchToDialogueModeLeft(); // 기본적으로 왼쪽 캐릭터 모드
        }
        
        /// <summary>
        /// 스토리 모드로 전환
        /// </summary>
        [ContextMenu("스토리 모드로 전환")]
        public void SwitchToStoryMode()
        {
            if (dialogueModePanel != null)
                dialogueModePanel.SetActive(false);
            if (storyModePanel != null)
                storyModePanel.SetActive(true);
            if (choicePanel != null)
                choicePanel.SetActive(false);
        }

        /// <summary>
        /// 선택지 모드로 전환
        /// </summary>
        [ContextMenu("선택지 모드로 전환")]
        public void SwitchToChoiceMode()
        {
            if (dialogueModePanel != null)
                dialogueModePanel.SetActive(false);
            if (storyModePanel != null)
                storyModePanel.SetActive(false);
            if (choicePanel != null)
                choicePanel.SetActive(true);
        }


[ContextMenu("패널 닫기")]
public void ClosePanel()
{
    Manager.ui.ClosePanel();
}



        
        /// <summary>
        /// 스토리 모드인지 확인
        /// </summary>
        public bool IsStoryMode()
        {
            return storyModePanel != null && storyModePanel.activeInHierarchy;
        }

        /// <summary>
        /// 대화 모드인지 확인
        /// </summary>
        public bool IsDialogueMode()
        {
            return dialogueModePanel != null && dialogueModePanel.activeInHierarchy;
        }

        /// <summary>
        /// 선택지 모드인지 확인
        /// </summary>
        public bool IsChoiceMode()
        {
            return choicePanel != null && choicePanel.activeInHierarchy;
        }
        
        /// <summary>
        /// 캐릭터 이름 설정 (스토리 모드)
        /// </summary>
        public void SetStoryCharacterName(string name)
        {
            if (storyLCharacterNameText != null)
                storyLCharacterNameText.text = name;
        }

        /// <summary>
        /// 캐릭터 이름 설정 (대화 모드 - 왼쪽 캐릭터)
        /// </summary>
        public void SetDialogueLeftCharacterName(string name)
        {
            if (dialogueLCharacterNameText != null)
                dialogueLCharacterNameText.text = name;
        }

        /// <summary>
        /// 캐릭터 이름 설정 (대화 모드 - 오른쪽 캐릭터)
        /// </summary>
        public void SetDialogueRightCharacterName(string name)
        {
            if (dialogueRCharacterNameText != null)
                dialogueRCharacterNameText.text = name;
        }



        /// <summary>
        /// 선택지 질문 설정
        /// </summary>
        public void SetChoiceQuestion(string question)
        {
            if (choiceQuestionText != null)
                choiceQuestionText.text = question;
        }

        /// <summary>
        /// 캐릭터 이름 설정 (기본 메서드 - SimpleDialogueController 호환)
        /// </summary>
        public void SetCharacterName(string name)
        {
            // 현재 모드에 따라 적절한 메서드 호출
            if (IsStoryMode())
            {
                SetStoryCharacterName(name);
            }
            else if (IsDialogueMode())
            {
                // 대화 모드에서는 기본적으로 왼쪽 캐릭터 이름 설정
                SetDialogueLeftCharacterName(name);
            }
        }

        /// <summary>
        /// 대화 모드에서 캐릭터 이름 설정 (위치 지정, 호환성 유지)
        /// </summary>
        public void SetDialogueCharacterName(string name, bool isLeftCharacter = true)
        {
            if (isLeftCharacter)
            {
                // 왼쪽 캐릭터가 말하는 경우: 왼쪽 이미지 + 오른쪽 이름
                SetDialogueLeftCharacterName(name);
            }
            else
            {
                // 오른쪽 캐릭터가 말하는 경우: 오른쪽 이미지 + 왼쪽 이름
                SetDialogueRightCharacterName(name);
            }
        }

        /// <summary>
        /// 스토리 텍스트 설정 (기본 메서드 - SimpleDialogueController 호환)
        /// </summary>
        public void SetStoryText(string text)
        {
            // 현재 모드에 따라 적절한 텍스트 설정
            if (IsStoryMode())
            {
                if (storyChatText != null)
                    storyChatText.text = text;
            }
            else if (IsDialogueMode())
            {
                if (dialogueChatText != null)
                    dialogueChatText.text = text;
            }
        }
        
        #endregion

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[StoryPanel] ChatWindowArea: {chatWindowArea != null}");
            Debug.Log($"[StoryPanel] ChoicePanel: {choicePanel != null}");
            Debug.Log($"[StoryPanel] StoryModePanel: {storyModePanel != null}");
            Debug.Log($"[StoryPanel] DialogueModePanel: {dialogueModePanel != null}");
            Debug.Log($"[StoryPanel] StoryTopArea: {storyTopArea != null}");
            
            Debug.Log($"[StoryPanel] ChoiceLCharacterImage: {choiceLCharacterImage != null}");
            Debug.Log($"[StoryPanel] ChoiceRCharacterNameText: {choiceRCharacterNameText != null}");
            Debug.Log($"[StoryPanel] ChoiceQuestionText: {choiceQuestionText != null}");
            
            Debug.Log($"[StoryPanel] StoryLCharacterNameText: {storyLCharacterNameText != null}");
            Debug.Log($"[StoryPanel] StoryRCharacterImage: {storyRCharacterImage != null}");
            Debug.Log($"[StoryPanel] StoryChatText: {storyChatText != null}");
            
            Debug.Log($"[StoryPanel] DialogueLCharacterNameText: {dialogueLCharacterNameText != null}");
            Debug.Log($"[StoryPanel] DialogueRCharacterNameText: {dialogueRCharacterNameText != null}");
            Debug.Log($"[StoryPanel] DialogueChatText: {dialogueChatText != null}");
            
            Debug.Log($"[StoryPanel] === 대화 모드 전환 메서드 ===");
            Debug.Log($"[StoryPanel] SwitchToDialogueModeLeft() - 왼쪽 캐릭터 대화");
            Debug.Log($"[StoryPanel] SwitchToDialogueModeRight() - 오른쪽 캐릭터 대화");
            Debug.Log($"[StoryPanel] SetDialogueLeftCharacterName() - 왼쪽 캐릭터 이름");
            Debug.Log($"[StoryPanel] SetDialogueRightCharacterName() - 오른쪽 캐릭터 이름");
        }
    }
}
