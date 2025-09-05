using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening; // DoTween 추가
using KYS.DialogueSystem; // DialogueGraph, DialogueNode, NodeType 사용을 위한 using 추가

namespace KYS
{
    public class StoryPanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string chatWindowAreaName = "ChatWindowArea";
        [SerializeField] private string choicePanelName = "ChoicePanel";
        [SerializeField] private string storyModePanelName = "StoryModePanel";
        [SerializeField] private string dialogueModePanelName = "DialogueModePanel";
        [SerializeField] private string storyTopAreaName = "StoryTopArea";
        [SerializeField] private string backgroundImageName = "BackgroundImage"; // 배경 이미지 추가
        [SerializeField] private string constellationImageName = "ConstellationImage"; // 별자리 이미지 추가

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
        private TextMeshProUGUI storyLCharacterNameText => GetUI<TextMeshProUGUI>("RunLStoryCaracterNameText");
        private Image storyRCharacterImage => GetUI<Image>("RStoryCharacterImage");
        private GameObject storyBottomArrow => GetUI("BottomArrow");
        private TextMeshProUGUI storyChatText => GetUI<TextMeshProUGUI>("RunStoryChatWindowText");
        
        // DialogueModePanel 요소들
        private GameObject dialogueModePanel => GetUI(dialogueModePanelName);
        private GameObject RDialoguCharacternameArea => GetUI("RDialogueCharacterNameArea");
        private TextMeshProUGUI dialogueRCharacterNameText => GetUI<TextMeshProUGUI>("RunRDialogueCharacterNameText");
        private Image dialogueLCharacterImage => GetUI<Image>("LDialogueCharacterImage");
        private GameObject LDialoguCharacternameArea => GetUI("LDialogueCharacterNameArea");
        private TextMeshProUGUI dialogueLCharacterNameText => GetUI<TextMeshProUGUI>("RunLDialogueCharacterNameText");
        private Image dialogueRCharacterImage => GetUI<Image>("RDialogueCharacterImage");
        private GameObject dialogueBottomArrow => GetUI("BottomArrow");
        private TextMeshProUGUI dialogueChatText => GetUI<TextMeshProUGUI>("RunDialogueChatWindowText");
        
        // StoryTopArea 요소들
        private GameObject storyTopArea => GetUI(storyTopAreaName);
        private Button logButton => GetUI<Button>("LogButton");
        private Button skipButton => GetUI<Button>("SkipButton");
        
        // 배경 이미지
        private Image backgroundImage => GetUI<Image>(backgroundImageName);
        
        // 별자리 이미지
        private Image constellationImage => GetUI<Image>(constellationImageName);

        [Header("Story Settings")]
        [SerializeField] private string[] storyPages = new string[0];
        [SerializeField] private int currentPage = 0;
        
        [Header("타이핑 효과")]
        [SerializeField] private TypingEffectManager typingEffectManager;
        
        // 멀티 캐릭터 대화 관련 변수
        private int currentMultiDialogueIndex = 0;
        private string[] currentCharacterNames;
        private string[] currentDialogueTexts;
        private bool[] currentCharacterPositions;
        
        // 노드 기반 대화 시스템 관련 변수
        private DialogueGraph currentDialogueGraph;
        private DialogueNode currentNode;
        private bool isNodeBasedDialogueActive = false;
        private bool isUpdatingUI = false; // UI 업데이트 중복 방지 플래그
        
        // 초기화 완료 이벤트
        public System.Action OnInitializationCompleted;
        
        // 초기화 상태 추적
        public bool IsInitialized { get; private set; } = false;
        

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
            
            // 언어 변경 이벤트 구독
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }
            
            // DialogueManager 이벤트 구독
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueStarted += OnDialogueStarted;
                DialogueManager.Instance.OnDialogueNodeChanged += OnDialogueNodeChanged;
            }
            
            // 초기화 상태 설정
            IsInitialized = true;
            
            // 초기화 완료 이벤트 호출
            OnInitializationCompleted?.Invoke();
            Debug.Log("[StoryPanel] 초기화 완료 - OnInitializationCompleted 이벤트 호출");
        }

        public override void Cleanup()
        {
            base.Cleanup();
            // 타이핑 효과 정리
            if (typingEffectManager != null)
            {
                typingEffectManager.StopTyping();
            }
            
            // 언어 변경 이벤트 구독 해제
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
            
            // DialogueManager 이벤트 구독 해제
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueStarted -= OnDialogueStarted;
                DialogueManager.Instance.OnDialogueNodeChanged -= OnDialogueNodeChanged;
            }
            
            // 초기화 완료 이벤트 정리
            OnInitializationCompleted = null;
            IsInitialized = false;
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
            {
                if (typingEffectManager != null)
                {
                    typingEffectManager.StartTypingEffect(storyPages[currentPage], storyChatText);
                }
                else
                {
                    storyChatText.text = storyPages[currentPage];
                }
            }

            if (storyBottomArrow != null)
                storyBottomArrow.SetActive(currentPage < storyPages.Length - 1);
        }

        private void UpdateDialogueModeUI()
        {
            if (dialogueChatText != null && storyPages.Length > 0)
            {
                if (typingEffectManager != null)
                {
                    typingEffectManager.StartTypingEffect(storyPages[currentPage], dialogueChatText);
                }
                else
                {
                    dialogueChatText.text = storyPages[currentPage];
                }
            }

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

        // 외부 콜백 설정을 위한 변수
        private System.Action onChatWindowClickedCallback;
        
        /// <summary>
        /// ChatWindowArea 클릭 콜백 설정
        /// </summary>
        public void SetChatWindowClickCallback(System.Action callback)
        {
            onChatWindowClickedCallback = callback;
        }
        
        private void OnChatWindowClicked()
        {
            if (typingEffectManager == null)
            {
                Debug.LogWarning("[StoryPanel] TypingEffectManager가 설정되지 않았습니다.");
                return;
            }

            Debug.Log($"[StoryPanel] OnChatWindowClicked 호출됨 - isTyping: {typingEffectManager.IsTyping}, isTypingCompleted: {typingEffectManager.IsTypingCompleted}");
            
            // 타이핑 중이면 즉시 완료하고 종료
            if (typingEffectManager.IsTyping)
            {
                Debug.Log("[StoryPanel] 타이핑 중 - 즉시 완료 후 종료");
                CompleteTyping();
                return; // 타이핑 완료 후 즉시 종료
            }
            
            // 타이핑이 방금 완료된 상태라면 바로 다음 노드로 이동
            if (typingEffectManager.IsTypingCompleted)
            {
                Debug.Log("[StoryPanel] 타이핑 완료됨 - 바로 다음 노드로 이동");
                typingEffectManager.ResetTypingCompleted(); // 다음 클릭을 위해 초기화
                // return 제거 - 바로 다음 노드로 이동
            }
            
            // CSV 기반 대화가 진행 중이면 처리
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            {
                Debug.Log("[StoryPanel] CSV 기반 대화 진행 중 - 다음 노드로 이동");
                OnCSVDialogueClicked();
                return;
            }
            
            // 노드 기반 대화가 진행 중이면 처리
            if (IsNodeBasedDialogueActive())
            {
                Debug.Log("[StoryPanel] 노드 기반 대화 진행 중");
                OnNodeBasedDialogueClicked();
                return;
            }
            
            // 멀티 캐릭터 대화가 진행 중이면 처리
            if (IsMultiCharacterDialogueActive())
            {
                Debug.Log("[StoryPanel] 멀티 캐릭터 대화 진행 중");
                OnMultiDialogueClicked();
                return;
            }
            
            // 외부 콜백이 설정되어 있으면 호출
            if (onChatWindowClickedCallback != null)
            {
                Debug.Log("[StoryPanel] 외부 콜백 호출");
                onChatWindowClickedCallback.Invoke();
                return;
            }
            
            // 기본 동작: 다음 페이지로 진행
            Debug.Log("[StoryPanel] 기본 동작 - 다음 페이지로 진행");
            NextPage();
        }
        
        /// <summary>
        /// CSV 기반 대화 클릭 처리
        /// </summary>
        private void OnCSVDialogueClicked()
        {
            if (DialogueManager.Instance == null || !DialogueManager.Instance.IsDialogueActive)
            {
                Debug.LogWarning("[StoryPanel] CSV 대화가 활성화되지 않았습니다.");
                return;
            }
            
            var currentDialogueData = DialogueManager.Instance.CurrentDialogueData;
            if (currentDialogueData == null)
            {
                Debug.LogWarning("[StoryPanel] 현재 대화 데이터가 null입니다.");
                return;
            }
            
            Debug.Log($"[StoryPanel] CSV 대화 클릭 - 현재 노드: {currentDialogueData.Id}, 타입: {currentDialogueData.NodeType}");
            
            // 노드 타입에 따른 처리
            switch (currentDialogueData.NodeType.ToLower())
            {
                case "start":
                case "dialogue":
                    // 대화 노드는 다음 노드로 이동
                    if (!string.IsNullOrEmpty(currentDialogueData.NextNodeId))
                    {
                        Debug.Log($"[StoryPanel] 다음 노드로 이동: {currentDialogueData.NextNodeId}");
                        DialogueManager.Instance.MoveToNextNode();
                        // UpdateUIFromDialogueData()는 OnDialogueNodeChanged 이벤트에서 자동 호출됨
                    }
                    else
                    {
                        Debug.Log("[StoryPanel] 다음 노드가 없음 - 대화 종료");
                        EndDialogue();
                    }
                    break;
                    
                case "story":
                    // 스토리 노드는 다음 페이지로 이동
                    NextPage();
                    break;
                    
                case "choice":
                    // 선택지 노드는 클릭 무시 (선택지 버튼으로 처리)
                    Debug.Log("[StoryPanel] 선택지 노드 - 클릭 무시");
                    break;
                    
                case "end":
                    Debug.Log("[StoryPanel] 종료 노드 - 대화 종료");
                    EndDialogue();
                    break;
                    
                default:
                    Debug.LogWarning($"[StoryPanel] 알 수 없는 노드 타입: {currentDialogueData.NodeType}");
                    break;
            }
        }
        
        /// <summary>
        /// 노드 기반 대화 클릭 처리
        /// </summary>
        private void OnNodeBasedDialogueClicked()
        {
            if (typingEffectManager == null) return;
            
            Debug.Log($"[StoryPanel] OnNodeBasedDialogueClicked 호출됨 - isTyping: {typingEffectManager.IsTyping}");
            
            // 현재 타이핑이 진행 중이면 즉시 완료하고 종료
            if (typingEffectManager.IsTyping)
            {
                Debug.Log("[StoryPanel] 노드 기반 대화에서 타이핑 중 - 즉시 완료 후 종료");
                CompleteTyping();
                return; // 타이핑 완료 후 즉시 종료
            }
            
            Debug.Log($"[StoryPanel] 노드 타입: {currentNode.nodeType}");
            
            // 노드 타입에 따른 처리
            switch (currentNode.nodeType)
            {
                case NodeType.Dialogue:
                case NodeType.Story:
                    Debug.Log("[StoryPanel] 대화/스토리 노드 - 다음 노드로 진행");
                    // 다음 노드로 진행
                    if (currentNode.nextNodeIds.Length > 0)
                    {
                        MoveToNextNode(currentNode.nextNodeIds[0]);
                    }
                    else
                    {
                        EndNodeBasedDialogue();
                    }
                    break;
                    
                case NodeType.Choice:
                    Debug.Log("[StoryPanel] 선택지 노드 - 클릭으로 진행하지 않음");
                    // 선택지 노드에서는 클릭으로 진행하지 않음
                    break;
                    
                case NodeType.Event:
                case NodeType.End:
                    Debug.Log("[StoryPanel] 이벤트/종료 노드 - 이미 처리됨");
                    // 이미 처리됨
                    break;
            }
        }
        
        /// <summary>
        /// 멀티 캐릭터 대화 클릭 처리
        /// </summary>
        private void OnMultiDialogueClicked()
        {
            if (typingEffectManager == null) return;
            
            Debug.Log($"[StoryPanel] OnMultiDialogueClicked 호출됨 - isTyping: {typingEffectManager.IsTyping}");
            
            // 현재 타이핑이 진행 중이면 즉시 완료하고 종료
            if (typingEffectManager.IsTyping)
            {
                Debug.Log("[StoryPanel] 멀티 대화에서 타이핑 중 - 즉시 완료 후 종료");
                CompleteTyping();
                return; // 타이핑 완료 후 즉시 종료
            }
            
            Debug.Log("[StoryPanel] 멀티 대화 - 다음 대화로 진행");
            // 다음 대화로 진행
            StartNextMultiDialogue();
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

        #region 타이핑 효과 관련 메서드



        /// <summary>
        /// 타이핑 효과 즉시 완료
        /// </summary>
        public void CompleteTyping()
        {
            if (typingEffectManager == null) return;
            
            // 현재 모드에 따라 적절한 텍스트 컴포넌트 선택
            TextMeshProUGUI targetText = null;
            if (IsStoryMode() && storyChatText != null)
            {
                targetText = storyChatText;
            }
            else if (IsDialogueMode() && dialogueChatText != null)
            {
                targetText = dialogueChatText;
            }
            
            if (targetText != null)
            {
                typingEffectManager.CompleteTyping(targetText);
                Debug.Log("[StoryPanel] 타이핑 효과 즉시 완료됨");
            }
        }

        /// <summary>
        /// 타이핑 효과 중지
        /// </summary>
        public void StopTyping()
        {
            if (typingEffectManager != null)
            {
                typingEffectManager.StopTyping();
            }
        }

        /// <summary>
        /// 타이핑 중인지 확인
        /// </summary>
        public bool IsTyping()
        {
            return typingEffectManager != null && typingEffectManager.IsTyping;
        }

        #endregion

        #region 멀티 캐릭터 대화 시스템
        
        /// <summary>
        /// 멀티 캐릭터 대화 시작 (좌우 캐릭터 변경)
        /// </summary>
        public void StartMultiCharacterDialogue(string[] characterNames, string[] dialogueTexts, bool[] characterPositions)
        {
            if (characterNames.Length == dialogueTexts.Length && dialogueTexts.Length == characterPositions.Length)
            {
                // 멀티 대화 변수 초기화
                currentMultiDialogueIndex = 0;
                currentCharacterNames = characterNames;
                currentDialogueTexts = dialogueTexts;
                currentCharacterPositions = characterPositions;
                
                SwitchToDialogueMode();
                Show();
                
                // 첫 번째 대화 시작
                StartNextMultiDialogue();
            }
        }
        
        /// <summary>
        /// 다음 멀티 대화로 진행
        /// </summary>
        private void StartNextMultiDialogue()
        {
            if (currentMultiDialogueIndex >= currentDialogueTexts.Length)
            {
                // 모든 대화 완료
                EndMultiCharacterDialogue();
                return;
            }
            
            string characterName = currentCharacterNames[currentMultiDialogueIndex];
            string dialogueText = currentDialogueTexts[currentMultiDialogueIndex];
            bool isRightCharacter = currentCharacterPositions[currentMultiDialogueIndex];
            
            // 캐릭터 위치에 따라 대화 모드 설정
            if (isRightCharacter)
            {
                SwitchToDialogueModeRight();
                SetDialogueRightCharacterName(characterName);
            }
            else
            {
                SwitchToDialogueModeLeft();
                SetDialogueLeftCharacterName(characterName);
            }
            
            // 대화 텍스트 설정
            SetStoryTextWithTyping(dialogueText, true);
            
            // 다음 대화 인덱스 저장
            currentMultiDialogueIndex++;
        }
        
        /// <summary>
        /// 멀티 캐릭터 대화 종료
        /// </summary>
        private void EndMultiCharacterDialogue()
        {
            // 멀티 대화 변수 정리
            currentMultiDialogueIndex = 0;
            currentCharacterNames = null;
            currentDialogueTexts = null;
            currentCharacterPositions = null;
            
            // 패널 숨기기
            Hide();
        }
        
        /// <summary>
        /// 멀티 캐릭터 대화가 활성화되어 있는지 확인
        /// </summary>
        public bool IsMultiCharacterDialogueActive()
        {
            return currentCharacterNames != null && currentDialogueTexts != null && currentCharacterPositions != null;
        }
        
        #endregion
        
        #region 노드 기반 대화 시스템 (CSV + Firebase 연동)
        
        /// <summary>
        /// CSV 기반 노드 대화 시작 (새로운 방식)
        /// </summary>
        public void StartCSVDialogue(string npcId, string stageId, string startNodeId = null)
        {
            Debug.Log($"[StoryPanel] StartCSVDialogue 호출: {npcId}, {stageId}, {startNodeId}");
            
            if (DialogueManager.Instance.StartDialogue(npcId, stageId, startNodeId))
            {
                Debug.Log("[StoryPanel] 대화 시작 성공, Show() 호출");
                Show();
                // UpdateUIFromDialogueData()는 OnDialogueStarted 이벤트에서 자동 호출됨
            }
            else
            {
                Debug.LogError($"[StoryPanel] 대화 시작 실패: NPC {npcId}, Stage {stageId}");
            }
        }

        /// <summary>
        /// DialogueData로부터 UI 업데이트
        /// </summary>
        private void UpdateUIFromDialogueData()
        {
            if (isUpdatingUI)
            {
                Debug.Log("[StoryPanel] UpdateUIFromDialogueData 중복 호출 방지");
                return;
            }

            isUpdatingUI = true;
            Debug.Log("[StoryPanel] UpdateUIFromDialogueData 호출됨");

            var dialogueData = DialogueManager.Instance.CurrentDialogueData;
            if (dialogueData == null)
            {
                Debug.LogWarning("[StoryPanel] CurrentDialogueData가 null입니다.");
                Debug.LogWarning("[StoryPanel] DialogueManager.Instance: " + (DialogueManager.Instance != null ? "존재" : "null"));
                isUpdatingUI = false;
                return;
            }

            Debug.Log($"[StoryPanel] UI 업데이트: {dialogueData.Id} - {dialogueData.NodeType} - {dialogueData.Speaker}");

            // 현재 언어 설정 가져오기
            var currentLanguage = LocalizationManager.Instance?.CurrentLanguage ?? SystemLanguage.Korean;

            // 타이핑 효과 사용 여부 결정
            bool useTypingEffect = true; // 기본값
            if (!string.IsNullOrEmpty(dialogueData.UseTypingEffect))
            {
                useTypingEffect = dialogueData.UseTypingEffect.ToLower() == "true";
            }
            Debug.Log($"[StoryPanel] 타이핑 효과 사용: {useTypingEffect}");

            // Speaker와 NodeType에 따른 UI 모드 설정 (먼저 텍스트 표시)
            if (dialogueData.Speaker.ToLower() == "story")
            {
                // 스토리 메시지는 스토리 모드로 표시 (이미지 없음)
                SwitchToStoryMode();
                SetStoryTextWithTyping(dialogueData.GetLocalizedDialogueText(currentLanguage), useTypingEffect);
            }
            else
            {
                switch (dialogueData.NodeType.ToLower())
                {
                    case "start":
                    case "dialogue":
                        // CharacterImagePosition에 따라 올바른 모드로 전환
                        string imagePosition = dialogueData.CharacterImagePosition?.ToLower() ?? "left";
                        Debug.Log($"[StoryPanel] CharacterImagePosition: '{dialogueData.CharacterImagePosition}' -> 파싱된 위치: '{imagePosition}'");
                        
                        if (imagePosition == "right")
                        {
                            SwitchToDialogueModeRight();
                        }
                        else if (imagePosition == "center")
                        {
                            // center는 story 모드로 처리
                            SwitchToStoryMode();
                        }
                        else
                        {
                            // left 또는 기타 값들은 왼쪽 모드로 처리
                            SwitchToDialogueModeLeft();
                        }
                        
                        string localizedSpeaker = dialogueData.GetLocalizedSpeaker(currentLanguage);
                        Debug.Log($"[StoryPanel] 스피커 이름 - 원본: '{dialogueData.Speaker}', 로컬라이즈: '{localizedSpeaker}', 언어: {currentLanguage}");
                        
                        // CharacterImagePosition에 따라 올바른 이름 위치 설정
                        bool isLeftCharacter = (imagePosition == "left");
                        Debug.Log($"[StoryPanel] 이름 위치 설정 - isLeftCharacter: {isLeftCharacter} (imagePosition: '{imagePosition}')");
                        SetDialogueCharacterName(localizedSpeaker, isLeftCharacter);
                        SetStoryTextWithTyping(dialogueData.GetLocalizedDialogueText(currentLanguage), useTypingEffect);
                        break;

                    case "choice":
                        SwitchToChoiceMode();
                        string choiceLocalizedSpeaker = dialogueData.GetLocalizedSpeaker(currentLanguage);
                        Debug.Log($"[StoryPanel] 선택창 스피커 이름 - 원본: '{dialogueData.Speaker}', 로컬라이즈: '{choiceLocalizedSpeaker}', 언어: {currentLanguage}");
                        SetChoiceCharacterName(choiceLocalizedSpeaker);
                        SetChoiceQuestion(dialogueData.GetLocalizedDialogueText(currentLanguage));

                        // 선택지 데이터 디버그 출력
                        var choiceTexts = dialogueData.GetLocalizedChoiceTexts(currentLanguage);
                        var choiceNextIds = dialogueData.GetChoiceNextIds();
                        Debug.Log($"[StoryPanel] 선택지 노드: {dialogueData.Id}");
                        Debug.Log($"[StoryPanel] ChoiceText1: '{dialogueData.ChoiceText1}' -> ChoiceNext1: '{dialogueData.ChoiceNext1}'");
                        Debug.Log($"[StoryPanel] ChoiceText2: '{dialogueData.ChoiceText2}' -> ChoiceNext2: '{dialogueData.ChoiceNext2}'");
                        Debug.Log($"[StoryPanel] ChoiceText3: '{dialogueData.ChoiceText3}' -> ChoiceNext3: '{dialogueData.ChoiceNext3}'");
                        Debug.Log($"[StoryPanel] ChoiceText4: '{dialogueData.ChoiceText4}' -> ChoiceNext4: '{dialogueData.ChoiceText4}'");
                        Debug.Log($"[StoryPanel] 로컬라이즈된 선택지 텍스트: [{string.Join(", ", choiceTexts)}]");
                        Debug.Log($"[StoryPanel] 선택지 다음 노드 ID: [{string.Join(", ", choiceNextIds)}]");

                        SetupChoices(choiceTexts, OnCSVChoiceSelected);
                        break;

                    case "story":
                        SwitchToStoryMode();
                        SetStoryTextWithTyping(dialogueData.GetLocalizedDialogueText(currentLanguage), useTypingEffect);
                        break;

                    case "end":
                        EndDialogue();
                        return;

                    default:
                        Debug.LogWarning($"[StoryPanel] 알 수 없는 노드 타입: {dialogueData.NodeType}");
                        // 기본적으로 대화 모드로 처리
                        SwitchToDialogueMode();
                        string defaultLocalizedSpeaker = dialogueData.GetLocalizedSpeaker(currentLanguage);
                        Debug.Log($"[StoryPanel] 기본 스피커 이름 - 원본: '{dialogueData.Speaker}', 로컬라이즈: '{defaultLocalizedSpeaker}', 언어: {currentLanguage}");
                        SetDialogueCharacterName(defaultLocalizedSpeaker, dialogueData.Speaker == "player");
                        SetStoryTextWithTyping(dialogueData.GetLocalizedDialogueText(currentLanguage), useTypingEffect);
                        break;
                }

            }

            // 자동 진행 설정
            if (dialogueData.IsAutoAdvance)
            {
                StartCoroutine(AutoAdvanceCoroutine(dialogueData.AutoAdvanceDelay));
            }

            // 텍스트 표시 후 이미지 로드 (비동기) - 시스템 메시지가 아닌 경우에만
            if (dialogueData.Speaker.ToLower() != "system")
            {
                // 이미지가 비어있으면 모든 캐릭터 이미지 숨기기
                if (string.IsNullOrEmpty(dialogueData.CharacterImage))
                {
                    Debug.Log("[StoryPanel] CharacterImage가 비어있어서 모든 캐릭터 이미지를 숨깁니다.");
                    HideAllCharacterImages();
                }
                else
                {
                    LoadAndSetCharacterImage(dialogueData.CharacterImage);
                }
            }
            else
            {
                Debug.Log("[StoryPanel] 시스템 메시지이므로 이미지를 로드하지 않습니다.");
            }

            // UI 업데이트 완료 (모든 경우에 실행)
            isUpdatingUI = false;
        }

        /// <summary>
        /// CSV 선택지 선택 처리
        /// </summary>
        private void OnCSVChoiceSelected(int choiceIndex)
        {
            Debug.Log($"[StoryPanel] OnCSVChoiceSelected 호출됨 - 선택지 인덱스: {choiceIndex}");
            
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[StoryPanel] DialogueManager.Instance가 null입니다!");
                return;
            }
            
            if (DialogueManager.Instance.CurrentDialogueData == null)
            {
                Debug.LogError("[StoryPanel] CurrentDialogueData가 null입니다!");
                return;
            }
            
            var currentData = DialogueManager.Instance.CurrentDialogueData;
            var choiceNextIds = currentData.GetChoiceNextIds();
            Debug.Log($"[StoryPanel] 현재 노드: {currentData.Id}");
            Debug.Log($"[StoryPanel] 선택 가능한 다음 노드들: [{string.Join(", ", choiceNextIds)}]");
            
            if (choiceIndex < choiceNextIds.Length)
            {
                Debug.Log($"[StoryPanel] 선택된 다음 노드: {choiceNextIds[choiceIndex]}");
            }
            else
            {
                Debug.LogError($"[StoryPanel] 잘못된 선택지 인덱스: {choiceIndex} (최대: {choiceNextIds.Length - 1})");
            }
            
            bool result = DialogueManager.Instance.SelectChoice(choiceIndex);
            Debug.Log($"[StoryPanel] SelectChoice 결과: {result}");
            
            // UpdateUIFromDialogueData()는 OnDialogueNodeChanged 이벤트에서 자동 호출됨
        }

        /// <summary>
        /// 언어 변경 시 대화 UI 업데이트
        /// </summary>
        private void OnLanguageChanged(SystemLanguage newLanguage)
        {
            // CSV 기반 대화가 진행 중이면 UI 업데이트
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            {
                UpdateUIFromDialogueData();
            }
        }
        
        /// <summary>
        /// DialogueManager에서 대화 시작 이벤트 처리
        /// </summary>
        private void OnDialogueStarted(DialogueData dialogueData)
        {
            Debug.Log($"[StoryPanel] OnDialogueStarted 이벤트 수신: {dialogueData?.Id}");
            UpdateUIFromDialogueData();
        }
        
        /// <summary>
        /// DialogueManager에서 노드 변경 이벤트 처리
        /// </summary>
        private void OnDialogueNodeChanged(string nodeId)
        {
            Debug.Log($"[StoryPanel] OnDialogueNodeChanged 이벤트 수신: {nodeId}");
            UpdateUIFromDialogueData();
        }

        /// <summary>
        /// 자동 진행 코루틴
        /// </summary>
        private System.Collections.IEnumerator AutoAdvanceCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (DialogueManager.Instance.IsDialogueActive)
            {
                DialogueManager.Instance.MoveToNextNode();
                // UpdateUIFromDialogueData()는 OnDialogueNodeChanged 이벤트에서 자동 호출됨
            }
        }

        /// <summary>
        /// 대화 종료
        /// </summary>
        private void EndDialogue()
        {
            DialogueManager.Instance.EndDialogue();
            Hide();
        }
        
        /// <summary>
        /// 노드 기반 대화 시작 (기존 방식 - 호환성 유지)
        /// </summary>
        public void StartNodeBasedDialogue(DialogueGraph dialogueGraph)
        {
            if (dialogueGraph == null)
            {
                Debug.LogError("[StoryPanel] 대화 그래프가 null입니다.");
                return;
            }
            
            // 그래프 초기화
            dialogueGraph.Initialize();
            
            // 시작 노드 가져오기
            DialogueNode startNode = dialogueGraph.GetStartNode();
            if (startNode == null)
            {
                Debug.LogError("[StoryPanel] 시작 노드를 찾을 수 없습니다.");
                return;
            }
            
            // 노드 기반 대화 변수 설정
            currentDialogueGraph = dialogueGraph;
            currentNode = startNode;
            isNodeBasedDialogueActive = true;
            
            // 패널 표시 및 첫 번째 노드 실행
            Show();
            ExecuteCurrentNode();
        }
        
        /// <summary>
        /// 현재 노드 실행
        /// </summary>
        private void ExecuteCurrentNode()
        {
            if (currentNode == null || !isNodeBasedDialogueActive)
                return;
            
            // 노드 조건 검사
            if (!currentDialogueGraph.CheckNodeConditions(currentNode))
            {
                Debug.Log($"[StoryPanel] 노드 조건 불충족: {currentNode.nodeId}");
                // 조건 불충족 시 다음 노드로 진행 (기본 분기)
                if (currentNode.nextNodeIds.Length > 0)
                {
                    MoveToNextNode(currentNode.nextNodeIds[0]);
                }
                else
                {
                    EndNodeBasedDialogue();
                }
                return;
            }
            
            // 노드 효과 실행
            currentDialogueGraph.ExecuteNodeEffects(currentNode);
            
            // 노드 타입에 따른 처리
            switch (currentNode.nodeType)
            {
                case NodeType.Dialogue:
                    ExecuteDialogueNode();
                    break;
                    
                case NodeType.Choice:
                    ExecuteChoiceNode();
                    break;
                    
                case NodeType.Story:
                    ExecuteStoryNode();
                    break;
                    
                case NodeType.Event:
                    ExecuteEventNode();
                    break;
                    
                case NodeType.End:
                    ExecuteEndNode();
                    break;
                    
                default:
                    Debug.LogWarning($"[StoryPanel] 알 수 없는 노드 타입: {currentNode.nodeType}");
                    break;
            }
        }
        
        /// <summary>
        /// 대화 노드 실행
        /// </summary>
        private void ExecuteDialogueNode()
        {
            // 대화 모드로 전환
            if (currentNode.isRightCharacter)
            {
                SwitchToDialogueModeRight();
                SetDialogueRightCharacterName(currentNode.characterName);
            }
            else
            {
                SwitchToDialogueModeLeft();
                SetDialogueLeftCharacterName(currentNode.characterName);
            }
            
            // 대화 텍스트 설정
            SetStoryTextWithTyping(currentNode.dialogueText, currentNode.useTypingEffect);
            
            // 타이핑 속도 설정
            if (currentNode.typingSpeed > 0)
            {
                // TODO: 개별 노드별 타이핑 속도 설정 구현
            }
        }
        
        /// <summary>
        /// 선택지 노드 실행
        /// </summary>
        private void ExecuteChoiceNode()
        {
            // 선택지 모드로 전환
            SwitchToChoiceMode();
            
            // 선택지 질문 설정 (대화 텍스트를 질문으로 사용)
            SetChoiceQuestion(currentNode.dialogueText);
            
            // 선택지 설정
            SetupChoices(currentNode.choiceTexts, OnNodeChoiceSelected);
        }
        
        /// <summary>
        /// 스토리 노드 실행
        /// </summary>
        private void ExecuteStoryNode()
        {
            // 스토리 모드로 전환
            SwitchToStoryMode();
            
            // 스토리 텍스트 설정
            SetStoryTextWithTyping(currentNode.dialogueText, currentNode.useTypingEffect);
        }
        
        /// <summary>
        /// 이벤트 노드 실행
        /// </summary>
        private void ExecuteEventNode()
        {
            // 이벤트 실행 (효과는 이미 실행됨)
            Debug.Log($"[StoryPanel] 이벤트 노드 실행: {currentNode.nodeId}");
            
            // 이벤트 노드는 즉시 다음 노드로 진행
            if (currentNode.nextNodeIds.Length > 0)
            {
                MoveToNextNode(currentNode.nextNodeIds[0]);
            }
            else
            {
                EndNodeBasedDialogue();
            }
        }
        
        /// <summary>
        /// 종료 노드 실행
        /// </summary>
        private void ExecuteEndNode()
        {
            Debug.Log($"[StoryPanel] 대화 종료 노드: {currentNode.nodeId}");
            EndNodeBasedDialogue();
        }
        
        /// <summary>
        /// 노드 선택지 선택 처리
        /// </summary>
        private void OnNodeChoiceSelected(int choiceIndex)
        {
            if (currentNode == null || choiceIndex >= currentNode.nextNodeIds.Length)
            {
                Debug.LogError("[StoryPanel] 잘못된 선택지 인덱스입니다.");
                return;
            }
            
            // 선택된 분기로 이동
            string nextNodeId = currentNode.nextNodeIds[choiceIndex];
            MoveToNextNode(nextNodeId);
        }
        
        /// <summary>
        /// 다음 노드로 이동
        /// </summary>
        private void MoveToNextNode(string nextNodeId)
        {
            if (string.IsNullOrEmpty(nextNodeId))
            {
                EndNodeBasedDialogue();
                return;
            }
            
            DialogueNode nextNode = currentDialogueGraph.GetNode(nextNodeId);
            if (nextNode == null)
            {
                Debug.LogError($"[StoryPanel] 다음 노드를 찾을 수 없습니다: {nextNodeId}");
                EndNodeBasedDialogue();
                return;
            }
            
            currentNode = nextNode;
            ExecuteCurrentNode();
        }
        
        /// <summary>
        /// 노드 기반 대화 종료
        /// </summary>
        private void EndNodeBasedDialogue()
        {
            // 변수 정리
            currentDialogueGraph = null;
            currentNode = null;
            isNodeBasedDialogueActive = false;
            
            // 패널 숨기기
            Hide();
        }
        
        /// <summary>
        /// 노드 기반 대화가 활성화되어 있는지 확인
        /// </summary>
        public bool IsNodeBasedDialogueActive()
        {
            return isNodeBasedDialogueActive;
        }
        
        #endregion
        
        #region 모드 전환 및 UI 제어
        
        /// <summary>
        /// 대화 모드로 전환 (왼쪽 캐릭터가 말하는 경우)
        /// </summary>
        public void SwitchToDialogueModeLeft()
        {
            if (storyModePanel != null)
                storyModePanel.SetActive(false);
            if (choicePanel != null)
                choicePanel.SetActive(false);
            if (dialogueModePanel != null)
                dialogueModePanel.SetActive(true);

            // 왼쪽 캐릭터 이미지와 오른쪽 캐릭터 이름 활성화
            if (dialogueLCharacterImage != null)
                dialogueLCharacterImage.gameObject.SetActive(true);  // ✅ 왼쪽 이미지 활성화
            if (dialogueRCharacterImage != null)
                dialogueRCharacterImage.gameObject.SetActive(false); // ✅ 오른쪽 이미지 비활성화
            if (RDialoguCharacternameArea != null)
                RDialoguCharacternameArea.SetActive(true);  // ✅ 오른쪽 이름 활성화
            if (LDialoguCharacternameArea != null)
                LDialoguCharacternameArea.SetActive(false); // ✅ 왼쪽 이름 비활성화
        }

        /// <summary>
        /// 대화 모드로 전환 (오른쪽 캐릭터가 말하는 경우)
        /// </summary>
        public void SwitchToDialogueModeRight()
        {
            if (storyModePanel != null)
                storyModePanel.SetActive(false);
            if (choicePanel != null)
                choicePanel.SetActive(false);
            if (dialogueModePanel != null)
                dialogueModePanel.SetActive(true);

            // 오른쪽 캐릭터 이미지와 왼쪽 캐릭터 이름 활성화
            if (dialogueLCharacterImage != null)
                dialogueLCharacterImage.gameObject.SetActive(false); // ✅ 왼쪽 이미지 비활성화
            if (dialogueRCharacterImage != null)
                dialogueRCharacterImage.gameObject.SetActive(true);  // ✅ 오른쪽 이미지 활성화
            if (RDialoguCharacternameArea != null)
                RDialoguCharacternameArea.SetActive(false); // ✅ 오른쪽 이름 비활성화
            if (LDialoguCharacternameArea != null)
                LDialoguCharacternameArea.SetActive(true);  // ✅ 왼쪽 이름 활성화
        }

        /// <summary>
        /// 대화 모드로 전환 (기본 - 왼쪽 캐릭터)
        /// </summary>
        public void SwitchToDialogueMode()
        {
            SwitchToDialogueModeLeft(); // 기본적으로 왼쪽 캐릭터 모드
        }
        
        /// <summary>
        /// 스토리 모드로 전환
        /// </summary>
        public void SwitchToStoryMode()
        {
            if (dialogueModePanel != null)
                dialogueModePanel.SetActive(false);
            if (choicePanel != null)
                choicePanel.SetActive(false);
            if (storyModePanel != null)
                storyModePanel.SetActive(true);
        }

        /// <summary>
        /// 선택지 모드로 전환
        /// </summary>
        public void SwitchToChoiceMode()
        {
            if (dialogueModePanel != null)
                dialogueModePanel.SetActive(false);
            if (storyModePanel != null)
                storyModePanel.SetActive(false);
            if (choicePanel != null)
                choicePanel.SetActive(true);
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
            Debug.Log($"[StoryPanel] SetDialogueLeftCharacterName - 이름: '{name}', UI 요소 존재: {dialogueLCharacterNameText != null}");
            if (dialogueLCharacterNameText != null)
            {
                dialogueLCharacterNameText.text = name;
                Debug.Log($"[StoryPanel] 왼쪽 이름 설정 완료: '{dialogueLCharacterNameText.text}'");
            }
            else
            {
                Debug.LogError("[StoryPanel] dialogueLCharacterNameText가 null입니다!");
            }
        }

        /// <summary>
        /// 캐릭터 이름 설정 (대화 모드 - 오른쪽 캐릭터)
        /// </summary>
        public void SetDialogueRightCharacterName(string name)
        {
            Debug.Log($"[StoryPanel] SetDialogueRightCharacterName - 이름: '{name}', UI 요소 존재: {dialogueRCharacterNameText != null}");
            if (dialogueRCharacterNameText != null)
            {
                dialogueRCharacterNameText.text = name;
                Debug.Log($"[StoryPanel] 오른쪽 이름 설정 완료: '{dialogueRCharacterNameText.text}'");
            }
            else
            {
                Debug.LogError("[StoryPanel] dialogueRCharacterNameText가 null입니다!");
            }
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
        /// 선택창 캐릭터 이름 설정
        /// </summary>
        public void SetChoiceCharacterName(string name)
        {
            Debug.Log($"[StoryPanel] SetChoiceCharacterName - 이름: '{name}', UI 요소 존재: {choiceRCharacterNameText != null}");
            if (choiceRCharacterNameText != null)
                choiceRCharacterNameText.text = name;
        }
        
        /// <summary>
        /// 선택지 설정 (ChoicePanel 직접 사용)
        /// </summary>
        public void SetupChoices(string[] choices, System.Action<int> onChoiceSelected)
        {
            if (choicePanel != null)
            {
                ChoicePanel choicePanelComponent = choicePanel.GetComponent<ChoicePanel>();
                if (choicePanelComponent != null)
                {
                    choicePanelComponent.SetupChoices(choices, onChoiceSelected);
                }
            }
        }
        
        /// <summary>
        /// 선택지 숨기기
        /// </summary>
        public void HideChoices()
        {
            if (choicePanel != null)
            {
                ChoicePanel choicePanelComponent = choicePanel.GetComponent<ChoicePanel>();
                if (choicePanelComponent != null)
                {
                    choicePanelComponent.ClearChoices();
                }
            }
        }
        
        /// <summary>
        /// 선택지 표시
        /// </summary>
        public void ShowChoices()
        {
            if (choicePanel != null)
            {
                choicePanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// 선택지 개수 반환
        /// </summary>
        public int GetChoiceCount()
        {
            if (choicePanel != null)
            {
                ChoicePanel choicePanelComponent = choicePanel.GetComponent<ChoicePanel>();
                if (choicePanelComponent != null)
                {
                    return choicePanelComponent.GetActiveChoiceCount();
                }
            }
            return 0;
        }
        
        /// <summary>
        /// 선택지 활성화 상태 확인
        /// </summary>
        public bool HasChoices()
        {
            if (choicePanel != null)
            {
                ChoicePanel choicePanelComponent = choicePanel.GetComponent<ChoicePanel>();
                if (choicePanelComponent != null)
                {
                    return choicePanelComponent.HasActiveChoices();
                }
            }
            return false;
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
        /// <param name="name">캐릭터 이름</param>
        /// <param name="isLeftCharacter">true: 왼쪽 이미지 위치, false: 오른쪽 이미지 위치</param>
        public void SetDialogueCharacterName(string name, bool isLeftCharacter = true)
        {
            Debug.Log($"[StoryPanel] SetDialogueCharacterName - 이름: '{name}', 왼쪽 이미지 위치: {isLeftCharacter}");
            
            if (isLeftCharacter)
            {
                // 왼쪽 이미지 위치: 왼쪽 이미지 + 오른쪽 이름 표시
                Debug.Log($"[StoryPanel] 왼쪽 이미지 위치 → 오른쪽 이름 설정: '{name}'");
                SetDialogueRightCharacterName(name);
            }
            else
            {
                // 오른쪽 이미지 위치: 오른쪽 이미지 + 왼쪽 이름 표시
                Debug.Log($"[StoryPanel] 오른쪽 이미지 위치 → 왼쪽 이름 설정: '{name}'");
                SetDialogueLeftCharacterName(name);
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
                {
                    if (typingEffectManager != null)
                    {
                        typingEffectManager.StartTypingEffect(text, storyChatText);
                    }
                    else
                    {
                        storyChatText.text = text;
                    }
                }
            }
            else if (IsDialogueMode())
            {
                if (dialogueChatText != null)
                {
                    if (typingEffectManager != null)
                    {
                        typingEffectManager.StartTypingEffect(text, dialogueChatText);
                    }
                    else
                    {
                        dialogueChatText.text = text;
                    }
                }
            }
        }

        /// <summary>
        /// 타이핑 효과가 포함된 스토리 텍스트 설정
        /// </summary>
        public void SetStoryTextWithTyping(string text, bool useTyping = true)
        {
            if (useTyping && typingEffectManager != null)
            {
                if (IsStoryMode() && storyChatText != null)
                {
                    typingEffectManager.StartTypingEffect(text, storyChatText);
                }
                else if (IsDialogueMode() && dialogueChatText != null)
                {
                    typingEffectManager.StartTypingEffect(text, dialogueChatText);
                }
            }
            else
            {
                SetStoryText(text);
            }
        }
        
        #region 이미지 설정 메서드들
        
        /// <summary>
        /// 배경 이미지 설정
        /// </summary>
        public void SetBackgroundImage(Sprite sprite)
        {
            if (backgroundImage != null)
            {
                backgroundImage.sprite = sprite;
                backgroundImage.gameObject.SetActive(sprite != null);
            }
        }
        
        /// <summary>
        /// 배경 이미지 숨기기
        /// </summary>
        public void HideBackgroundImage()
        {
            if (backgroundImage != null)
            {
                backgroundImage.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 별자리 이미지 설정
        /// </summary>
        public void SetConstellationImage(Sprite sprite)
        {
            if (constellationImage != null)
            {
                constellationImage.sprite = sprite;
                constellationImage.gameObject.SetActive(sprite != null);
            }
        }
        
        /// <summary>
        /// 별자리 이미지 숨기기
        /// </summary>
        public void HideConstellationImage()
        {
            if (constellationImage != null)
            {
                constellationImage.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 별자리 이미지 표시/숨김 토글
        /// </summary>
        public void ToggleConstellationImage()
        {
            if (constellationImage != null)
            {
                constellationImage.gameObject.SetActive(!constellationImage.gameObject.activeInHierarchy);
            }
        }
        
        /// <summary>
        /// 스토리 모드 캐릭터 이미지 설정
        /// </summary>
        public void SetStoryCharacterImage(Sprite sprite)
        {
            if (storyRCharacterImage != null)
            {
                storyRCharacterImage.sprite = sprite;
                storyRCharacterImage.gameObject.SetActive(sprite != null);
            }
        }
        
        /// <summary>
        /// 대화 모드 왼쪽 캐릭터 이미지 설정
        /// </summary>
        public void SetDialogueLeftCharacterImage(Sprite sprite)
        {
            if (dialogueLCharacterImage != null)
            {
                dialogueLCharacterImage.sprite = sprite;
                dialogueLCharacterImage.gameObject.SetActive(sprite != null);
            }
        }
        
        /// <summary>
        /// 대화 모드 오른쪽 캐릭터 이미지 설정
        /// </summary>
        public void SetDialogueRightCharacterImage(Sprite sprite)
        {
            if (dialogueRCharacterImage != null)
            {
                dialogueRCharacterImage.sprite = sprite;
                dialogueRCharacterImage.gameObject.SetActive(sprite != null);
            }
        }
        
        /// <summary>
        /// 선택지 모드 왼쪽 캐릭터 이미지 설정
        /// </summary>
        public void SetChoiceLeftCharacterImage(Sprite sprite)
        {
            if (choiceLCharacterImage != null)
            {
                choiceLCharacterImage.sprite = sprite;
                choiceLCharacterImage.gameObject.SetActive(sprite != null);
            }
        }
        
        /// <summary>
        /// 현재 모드에 따른 캐릭터 이미지 설정 (편의 메서드)
        /// </summary>
        public void SetCharacterImage(Sprite sprite, bool isRightCharacter = false)
        {
            if (IsStoryMode())
            {
                SetStoryCharacterImage(sprite);
            }
            else if (IsDialogueMode())
            {
                if (isRightCharacter)
                {
                    SetDialogueRightCharacterImage(sprite);
                }
                else
                {
                    SetDialogueLeftCharacterImage(sprite);
                }
            }
            else if (IsChoiceMode())
            {
                SetChoiceLeftCharacterImage(sprite);
            }
        }

        /// <summary>
        /// 캐릭터 이미지 로드 및 설정 (Addressable 지원)
        /// </summary>
        private async void LoadAndSetCharacterImage(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                Debug.Log("[StoryPanel] CharacterImage가 비어있습니다.");
                return;
            }

            Debug.Log($"[StoryPanel] 캐릭터 이미지 로드 시작: {imageName}");
            Sprite characterSprite = null;

            // 1. DataManager의 이미지 캐시에서 직접 확인 (동기)
            if (Manager.data != null)
            {
                characterSprite = Manager.data.GetCachedCharacterImage(imageName);
                if (characterSprite != null)
                {
                    Debug.Log($"[StoryPanel] DataManager 캐시에서 이미지 발견: {imageName}");
                }
            }

            // 2. 캐시에 없으면 비동기 로드
            if (characterSprite == null && Manager.data != null)
            {
                try
                {
                    characterSprite = await Manager.data.LoadCharacterImageAsync(imageName);
                    if (characterSprite != null)
                    {
                        Debug.Log($"[StoryPanel] DataManager에서 이미지 로드 완료: {imageName}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[StoryPanel] DataManager 이미지 로드 실패: {imageName} - {e.Message}");
                }
            }

            // 3. Addressable 로드 실패 시 Resources 폴더에서 시도
            if (characterSprite == null)
            {
                characterSprite = Resources.Load<Sprite>($"Characters/{imageName}");
                
                if (characterSprite == null)
                {
                    characterSprite = Resources.Load<Sprite>($"UI/Characters/{imageName}");
                }
            }
            
            if (characterSprite == null)
            {
                Debug.LogWarning($"[StoryPanel] 캐릭터 이미지를 찾을 수 없습니다: {imageName}");
                // 이미지가 없을 때 모든 캐릭터 이미지 UI 비활성화
                HideAllCharacterImages();
                return;
            }

            // 현재 모드에 따라 이미지 설정
            if (IsStoryMode())
            {
                SetStoryCharacterImage(characterSprite);
            }
            else if (IsDialogueMode())
            {
                // CharacterImagePosition에 따라 이미지 위치 결정
                var currentDialogueData = DialogueManager.Instance?.CurrentDialogueData;
                if (currentDialogueData != null)
                {
                    string imagePosition = currentDialogueData.CharacterImagePosition?.ToLower() ?? "left";
                    Debug.Log($"[StoryPanel] CharacterImagePosition: {imagePosition}");
                    
                    switch (imagePosition)
                    {
                        case "right":
                            SetDialogueRightCharacterImage(characterSprite);
                            break;
                        case "center":
                            // 중앙 위치는 현재 지원하지 않으므로 왼쪽으로 설정
                            SetDialogueLeftCharacterImage(characterSprite);
                            Debug.LogWarning("[StoryPanel] center 위치는 아직 지원하지 않습니다. left로 설정합니다.");
                            break;
                        case "left":
                        default:
                            SetDialogueLeftCharacterImage(characterSprite);
                            break;
                    }
                }
                else
                {
                    // 기본값: 왼쪽
                    SetDialogueLeftCharacterImage(characterSprite);
                }
            }
            else if (IsChoiceMode())
            {
                SetChoiceLeftCharacterImage(characterSprite);
            }

            Debug.Log($"[StoryPanel] 캐릭터 이미지 설정 완료: {imageName}");
        }
        
        /// <summary>
        /// 모든 캐릭터 이미지 숨기기
        /// </summary>
        public void HideAllCharacterImages()
        {
            if (storyRCharacterImage != null)
                storyRCharacterImage.gameObject.SetActive(false);
            if (dialogueLCharacterImage != null)
                dialogueLCharacterImage.gameObject.SetActive(false);
            if (dialogueRCharacterImage != null)
                dialogueRCharacterImage.gameObject.SetActive(false);
            if (choiceLCharacterImage != null)
                choiceLCharacterImage.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 이미지 색상 설정 (투명도 조절 등)
        /// </summary>
        public void SetImageColor(Color color, ImageType imageType = ImageType.All)
        {
            switch (imageType)
            {
                case ImageType.Background:
                    if (backgroundImage != null)
                        backgroundImage.color = color;
                    break;
                    
                case ImageType.Constellation:
                    if (constellationImage != null)
                        constellationImage.color = color;
                    break;
                    
                case ImageType.StoryCharacter:
                    if (storyRCharacterImage != null)
                        storyRCharacterImage.color = color;
                    break;
                    
                case ImageType.DialogueLeftCharacter:
                    if (dialogueLCharacterImage != null)
                        dialogueLCharacterImage.color = color;
                    break;
                    
                case ImageType.DialogueRightCharacter:
                    if (dialogueRCharacterImage != null)
                        dialogueRCharacterImage.color = color;
                    break;
                    
                case ImageType.ChoiceLeftCharacter:
                    if (choiceLCharacterImage != null)
                        choiceLCharacterImage.color = color;
                    break;
                    
                case ImageType.All:
                    if (backgroundImage != null)
                        backgroundImage.color = color;
                    if (constellationImage != null)
                        constellationImage.color = color;
                    if (storyRCharacterImage != null)
                        storyRCharacterImage.color = color;
                    if (dialogueLCharacterImage != null)
                        dialogueLCharacterImage.color = color;
                    if (dialogueRCharacterImage != null)
                        dialogueRCharacterImage.color = color;
                    if (choiceLCharacterImage != null)
                        choiceLCharacterImage.color = color;
                    break;
            }
        }
        
        /// <summary>
        /// 이미지 타입 열거형
        /// </summary>
        public enum ImageType
        {
            Background,
            Constellation,
            StoryCharacter,
            DialogueLeftCharacter,
            DialogueRightCharacter,
            ChoiceLeftCharacter,
            All
        }
        
        #endregion
        
        #endregion
    }
}