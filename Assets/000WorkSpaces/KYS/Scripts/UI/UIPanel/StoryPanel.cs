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
        [SerializeField] private string centerImageName = "CenterImage"; // 가운데 이미지 추가
        [SerializeField] private string skipButtonName = "SkipButton"; // 스킵 버튼

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
        
        // 가운데 이미지
        private Image centerImage => GetUI<Image>(centerImageName);

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
        
        // 가운데 이미지 관련 변수
        private Coroutine centerImageCoroutine;
        private bool isCenterImageActive = false;
        private string currentCenterImageKey = ""; // 현재 표시 중인 이미지 키
        
        // 이미지 로딩 상태 추적
        private bool isCharacterImageLoaded = false;
        
        // 스킵 관련 변수
        private bool isSkipMode = false;
        private Coroutine autoAdvanceCoroutine;
        private float skipAutoAdvanceDelay = 0.1f; // 스킵 모드에서 자동 진행 지연 시간
        
        [Header("스킵 설정")]
        [SerializeField] private float[] skipSpeedOptions = { 0.05f, 0.1f, 0.2f, 0.5f, 1.0f }; // 스킵 속도 옵션들 (초)
        [SerializeField] private int currentSkipSpeedIndex = 1; // 현재 선택된 스킵 속도 인덱스 (기본: 0.1초)
        [SerializeField] private bool allowTouchToCancelSkip = true; // 터치로 스킵 취소 허용
        
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
                "ui_log_button"
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
            
            // 초기화 시 모든 캐릭터 이미지 숨기기 (프리팹 기본 이미지 방지)
            HideAllCharacterImages();
            
            // 스킵 속도 초기화
            skipAutoAdvanceDelay = GetCurrentSkipSpeed();
            
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

        public override void Hide()
        {
            // 화면 숨기기 전에 모든 이미지 정리
            HideAllCharacterImages();
            HideCenterImage();
            
            // 스킵 모드 중지
            StopSkipMode();
            
            base.Hide();
        }
        
        /// <summary>
        /// 패널 닫기 (Manager.ui.ClosePanel() 호출 시)
        /// </summary>
        public void ClosePanel()
        {
            Debug.Log("[StoryPanel] ClosePanel 호출 - 이미지 정리 후 패널 닫기");
            
            // 스킵 모드 중지 (먼저 중지)
            StopSkipMode();
            
            // 타이핑 효과 중지
            if (typingEffectManager != null)
            {
                typingEffectManager.StopTyping();
            }
            
            // 이미지 정리 (스프라이트 제거 포함)
            HideAllCharacterImages();
            HideCenterImage();
            
            // 추가로 모든 이미지 UI 강제 비활성화
            ForceHideAllImages();
            
            // UIManager를 통해 패널 닫기
            Manager.ui.ClosePanel();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            
            // 모든 이미지 숨기기 (화면 닫힐 때 이미지가 보이는 문제 방지)
            HideAllCharacterImages();
            HideCenterImage();
            
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
            
            // 스킵 모드 정리
            StopSkipMode();
        }

        private void SetupButtons()
        {
            // ChatWindowArea 전체가 NextButton 역할
            var chatAreaHandler = GetEvent(chatWindowAreaName);
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
            // 스킵 모드일 때 터치로 스킵 취소
            if (isSkipMode && allowTouchToCancelSkip)
            {
                Debug.Log("[StoryPanel] 스킵 모드에서 터치 감지 - 스킵 취소");
                StopSkipMode();
                return;
            }
            
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
            
            if (isSkipMode)
            {
                // 스킵 모드 해제
                StopSkipMode();
            }
            else
            {
                // 스킵 모드 활성화
                StartSkipMode();
            }
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
            
            // 패널 닫기
            ClosePanel();
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
                        
                        // UseTypingEffect를 안전하게 파싱 (기본값: true)
                        bool useTypingEffectDialogue = true;
                        if (!string.IsNullOrEmpty(dialogueData.UseTypingEffect))
                        {
                            if (bool.TryParse(dialogueData.UseTypingEffect, out bool parsedValue))
                            {
                                useTypingEffectDialogue = parsedValue;
                            }
                            else
                            {
                                Debug.LogWarning($"[StoryPanel] UseTypingEffect 파싱 실패: '{dialogueData.UseTypingEffect}', 기본값 true 사용");
                            }
                        }
                        SetStoryTextWithTyping(dialogueData.GetLocalizedDialogueText(currentLanguage), useTypingEffectDialogue);
                        
                        // CharacterImage 처리 (이 case 블록 내에서)
                        if (string.IsNullOrEmpty(dialogueData.CharacterImage))
                        {
                            Debug.Log("[StoryPanel] CharacterImage가 비어있어서 모든 캐릭터 이미지를 숨깁니다.");
                            HideAllCharacterImages();
                        }
                        else
                        {
                            LoadAndSetCharacterImage(dialogueData.CharacterImage);
                        }
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
                        
                        // CharacterImage 처리 (이 case 블록 내에서)
                        if (string.IsNullOrEmpty(dialogueData.CharacterImage))
                        {
                            Debug.Log("[StoryPanel] CharacterImage가 비어있어서 모든 캐릭터 이미지를 숨깁니다.");
                            HideAllCharacterImages();
                        }
                        else
                        {
                            LoadAndSetCharacterImage(dialogueData.CharacterImage);
                        }
                        break;

                    case "story":
                        SwitchToStoryMode();
                        
                        // UseTypingEffect를 안전하게 파싱 (기본값: true)
                        bool useTypingEffectStory = true;
                        if (!string.IsNullOrEmpty(dialogueData.UseTypingEffect))
                        {
                            if (bool.TryParse(dialogueData.UseTypingEffect, out bool parsedValue))
                            {
                                useTypingEffectStory = parsedValue;
                            }
                            else
                            {
                                Debug.LogWarning($"[StoryPanel] UseTypingEffect 파싱 실패: '{dialogueData.UseTypingEffect}', 기본값 true 사용");
                            }
                        }
                        SetStoryTextWithTyping(dialogueData.GetLocalizedDialogueText(currentLanguage), useTypingEffectStory);
                        
                        // CharacterImage 처리 (이 case 블록 내에서)
                        if (string.IsNullOrEmpty(dialogueData.CharacterImage))
                        {
                            Debug.Log("[StoryPanel] CharacterImage가 비어있어서 모든 캐릭터 이미지를 숨깁니다.");
                            HideAllCharacterImages();
                        }
                        else
                        {
                            LoadAndSetCharacterImage(dialogueData.CharacterImage);
                        }
                        break;

                    case "end":
                        // end 노드는 dialogue와 동일하게 처리 (대사 표시 후 클릭 시 종료)
                        // CharacterImagePosition에 따라 올바른 모드로 전환
                        string endImagePosition = dialogueData.CharacterImagePosition?.ToLower() ?? "left";
                        Debug.Log($"[StoryPanel] end 노드 CharacterImagePosition: '{dialogueData.CharacterImagePosition}' -> 파싱된 위치: '{endImagePosition}'");
                        
                        if (endImagePosition == "right")
                        {
                            SwitchToDialogueModeRight();
                        }
                        else if (endImagePosition == "center")
                        {
                            // center는 story 모드로 처리
                            SwitchToStoryMode();
                        }
                        else
                        {
                            // left 또는 기타 값들은 왼쪽 모드로 처리
                            SwitchToDialogueModeLeft();
                        }
                        
                        string endLocalizedSpeaker = dialogueData.GetLocalizedSpeaker(currentLanguage);
                        Debug.Log($"[StoryPanel] end 노드 스피커 이름 - 원본: '{dialogueData.Speaker}', 로컬라이즈: '{endLocalizedSpeaker}', 언어: {currentLanguage}");
                        
                        // CharacterImagePosition에 따라 올바른 이름 위치 설정
                        bool endIsLeftCharacter = (endImagePosition == "left");
                        Debug.Log($"[StoryPanel] end 노드 이름 위치 설정 - isLeftCharacter: {endIsLeftCharacter} (imagePosition: '{endImagePosition}')");
                        SetDialogueCharacterName(endLocalizedSpeaker, endIsLeftCharacter);
                        
                        // UseTypingEffect를 안전하게 파싱 (기본값: true)
                        bool useTypingEffectEnd = true;
                        if (!string.IsNullOrEmpty(dialogueData.UseTypingEffect))
                        {
                            if (bool.TryParse(dialogueData.UseTypingEffect, out bool parsedValue))
                            {
                                useTypingEffectEnd = parsedValue;
                            }
                            else
                            {
                                Debug.LogWarning($"[StoryPanel] UseTypingEffect 파싱 실패: '{dialogueData.UseTypingEffect}', 기본값 true 사용");
                            }
                        }
                        SetStoryTextWithTyping(dialogueData.GetLocalizedDialogueText(currentLanguage), useTypingEffectEnd);
                        
                        // CharacterImage 처리
                        if (string.IsNullOrEmpty(dialogueData.CharacterImage))
                        {
                            Debug.Log("[StoryPanel] end 노드 CharacterImage가 비어있어서 모든 캐릭터 이미지를 숨깁니다.");
                            HideAllCharacterImages();
                        }
                        else
                        {
                            LoadAndSetCharacterImage(dialogueData.CharacterImage);
                        }
                        break;

                    default:
                        Debug.LogWarning($"[StoryPanel] 알 수 없는 노드 타입: {dialogueData.NodeType}");
                        // 기본적으로 대화 모드로 처리
                        SwitchToDialogueMode();
                        string defaultLocalizedSpeaker = dialogueData.GetLocalizedSpeaker(currentLanguage);
                        Debug.Log($"[StoryPanel] 기본 스피커 이름 - 원본: '{dialogueData.Speaker}', 로컬라이즈: '{defaultLocalizedSpeaker}', 언어: {currentLanguage}");
                        SetDialogueCharacterName(defaultLocalizedSpeaker, dialogueData.Speaker == "player");
                        
                        // UseTypingEffect를 안전하게 파싱 (기본값: true)
                        bool useTypingEffectDefault = true;
                        if (!string.IsNullOrEmpty(dialogueData.UseTypingEffect))
                        {
                            if (bool.TryParse(dialogueData.UseTypingEffect, out bool parsedValue))
                            {
                                useTypingEffectDefault = parsedValue;
                            }
                            else
                            {
                                Debug.LogWarning($"[StoryPanel] UseTypingEffect 파싱 실패: '{dialogueData.UseTypingEffect}', 기본값 true 사용");
                            }
                        }
                        SetStoryTextWithTyping(dialogueData.GetLocalizedDialogueText(currentLanguage), useTypingEffectDefault);
                        break;
                }

            }

            // 자동 진행 설정
            if (dialogueData.IsAutoAdvance)
            {
                StartCoroutine(AutoAdvanceCoroutine(dialogueData.AutoAdvanceDelay));
            }

            // CharacterImage 처리는 각 case 블록에서 이미 처리됨

            // 배경 이미지 효과 처리
            if (!string.IsNullOrEmpty(dialogueData.BackgroundImage))
            {
                Debug.Log($"[StoryPanel] 배경 이미지 로드: {dialogueData.BackgroundImage}");
                LoadAndSetBackgroundImage(dialogueData.BackgroundImage);
            }
            else
            {
                Debug.Log("[StoryPanel] 배경 이미지가 비어있어서 배경 이미지를 숨깁니다.");
                HideBackgroundImage();
            }

            // 별자리 이미지 효과 처리
            if (!string.IsNullOrEmpty(dialogueData.ConstellationImage))
            {
                Debug.Log($"[StoryPanel] 별자리 이미지 로드: {dialogueData.ConstellationImage}");
                LoadAndSetConstellationImage(dialogueData.ConstellationImage);
            }
            else
            {
                Debug.Log("[StoryPanel] 별자리 이미지가 비어있어서 별자리 이미지를 숨깁니다.");
                HideConstellationImage();
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
            ClosePanel();
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
            
            // 패널 닫기
            ClosePanel();
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

            // 캐릭터 이미지는 CharacterImage 설정에 따라 나중에 처리됨
            // 여기서는 이름 영역만 설정
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

            // 캐릭터 이미지는 CharacterImage 설정에 따라 나중에 처리됨
            // 여기서는 이름 영역만 설정
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
                if (sprite == null)
                {
                    Debug.Log("[StoryPanel] 스토리 캐릭터 이미지를 숨깁니다.");
                    storyRCharacterImage.gameObject.SetActive(false);
                }
                else
                {
                    storyRCharacterImage.gameObject.SetActive(true);
                }
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
                if (sprite == null)
                {
                    Debug.Log("[StoryPanel] 대화 왼쪽 캐릭터 이미지를 숨깁니다.");
                    dialogueLCharacterImage.gameObject.SetActive(false);
                }
                else
                {
                    dialogueLCharacterImage.gameObject.SetActive(true);
                }
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
                if (sprite == null)
                {
                    Debug.Log("[StoryPanel] 대화 오른쪽 캐릭터 이미지를 숨깁니다.");
                    dialogueRCharacterImage.gameObject.SetActive(false);
                }
                else
                {
                    dialogueRCharacterImage.gameObject.SetActive(true);
                }
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
                if (sprite == null)
                {
                    Debug.Log("[StoryPanel] 선택지 왼쪽 캐릭터 이미지를 숨깁니다.");
                    choiceLCharacterImage.gameObject.SetActive(false);
                }
                else
                {
                    choiceLCharacterImage.gameObject.SetActive(true);
                }
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
                // 이미지 로딩 실패 상태 설정
                isCharacterImageLoaded = false;
                // 이미지가 없을 때 모든 캐릭터 이미지 UI 비활성화
                HideAllCharacterImages();
                // 추가로 null 스프라이트로 설정하여 기본 이미지 제거
                SetStoryCharacterImage(null);
                SetDialogueLeftCharacterImage(null);
                SetDialogueRightCharacterImage(null);
                SetChoiceLeftCharacterImage(null);
                return;
            }
            
            // 이미지 로딩 성공 상태 설정
            isCharacterImageLoaded = true;

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
                // 선택지 모드: 왼쪽 캐릭터 이미지 설정
                SetChoiceLeftCharacterImage(characterSprite);
            }

            Debug.Log($"[StoryPanel] 캐릭터 이미지 설정 완료: {imageName}");
        }
        
        /// <summary>
        /// 모든 캐릭터 이미지 숨기기
        /// </summary>
        public void HideAllCharacterImages()
        {
            Debug.Log("[StoryPanel] HideAllCharacterImages - 모든 캐릭터 이미지 숨김 시작");
            
            if (storyRCharacterImage != null)
            {
                storyRCharacterImage.sprite = null; // 스프라이트 제거
                storyRCharacterImage.gameObject.SetActive(false);
                Debug.Log("[StoryPanel] 스토리 오른쪽 캐릭터 이미지 숨김");
            }
            if (dialogueLCharacterImage != null)
            {
                dialogueLCharacterImage.sprite = null; // 스프라이트 제거
                dialogueLCharacterImage.gameObject.SetActive(false);
                Debug.Log("[StoryPanel] 대화 왼쪽 캐릭터 이미지 숨김");
            }
            if (dialogueRCharacterImage != null)
            {
                dialogueRCharacterImage.sprite = null; // 스프라이트 제거
                dialogueRCharacterImage.gameObject.SetActive(false);
                Debug.Log("[StoryPanel] 대화 오른쪽 캐릭터 이미지 숨김");
            }
            if (choiceLCharacterImage != null)
            {
                choiceLCharacterImage.sprite = null; // 스프라이트 제거
                choiceLCharacterImage.gameObject.SetActive(false);
                Debug.Log("[StoryPanel] 선택지 왼쪽 캐릭터 이미지 숨김");
            }
            
            // 이미지 로딩 상태 초기화
            isCharacterImageLoaded = false;
            
            Debug.Log("[StoryPanel] HideAllCharacterImages - 모든 캐릭터 이미지 숨김 완료");
        }
        
        /// <summary>
        /// 모든 이미지 UI 강제 비활성화 (패널 닫기 시 사용)
        /// </summary>
        private void ForceHideAllImages()
        {
            Debug.Log("[StoryPanel] ForceHideAllImages - 모든 이미지 UI 강제 비활성화");
            
            // 모든 캐릭터 이미지 UI 강제 비활성화
            if (storyRCharacterImage != null)
            {
                storyRCharacterImage.sprite = null;
                storyRCharacterImage.gameObject.SetActive(false);
                storyRCharacterImage.enabled = false; // Image 컴포넌트 비활성화
            }
            if (dialogueLCharacterImage != null)
            {
                dialogueLCharacterImage.sprite = null;
                dialogueLCharacterImage.gameObject.SetActive(false);
                dialogueLCharacterImage.enabled = false;
            }
            if (dialogueRCharacterImage != null)
            {
                dialogueRCharacterImage.sprite = null;
                dialogueRCharacterImage.gameObject.SetActive(false);
                dialogueRCharacterImage.enabled = false;
            }
            if (choiceLCharacterImage != null)
            {
                choiceLCharacterImage.sprite = null;
                choiceLCharacterImage.gameObject.SetActive(false);
                choiceLCharacterImage.enabled = false;
            }
            
            // 가운데 이미지도 강제 비활성화
            if (centerImage != null)
            {
                centerImage.sprite = null;
                centerImage.gameObject.SetActive(false);
                centerImage.enabled = false;
            }
            
            Debug.Log("[StoryPanel] ForceHideAllImages - 모든 이미지 UI 강제 비활성화 완료");
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
        /// <summary>
        /// 배경 이미지 로드 및 설정 (Addressables만 사용)
        /// </summary>
        private async void LoadAndSetBackgroundImage(string imageKey)
        {
            try
            {
                Debug.Log($"[StoryPanel] 배경 이미지 로딩 시작: {imageKey}");
                
                // 먼저 캐시에서 확인
                Sprite cachedSprite = Manager.data.GetCachedCharacterImage(imageKey);
                if (cachedSprite != null)
                {
                    Debug.Log($"[StoryPanel] 배경 이미지 캐시에서 로드: {imageKey}");
                    SetBackgroundImage(cachedSprite);
                    return;
                }
                
                // Addressable에서 로드
                Sprite sprite = await Manager.data.LoadCharacterImageAsync(imageKey);
                if (sprite != null)
                {
                    Debug.Log($"[StoryPanel] 배경 이미지 Addressable에서 로드: {imageKey}");
                    SetBackgroundImage(sprite);
                }
                else
                {
                    Debug.LogWarning($"[StoryPanel] 배경 이미지 로드 실패: {imageKey}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[StoryPanel] 배경 이미지 로드 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 별자리 이미지 로드 및 설정 (Addressables만 사용)
        /// </summary>
        private async void LoadAndSetConstellationImage(string imageKey)
        {
            try
            {
                Debug.Log($"[StoryPanel] 별자리 이미지 로딩 시작: {imageKey}");
                
                // 먼저 캐시에서 확인
                Sprite cachedSprite = Manager.data.GetCachedCharacterImage(imageKey);
                if (cachedSprite != null)
                {
                    Debug.Log($"[StoryPanel] 별자리 이미지 캐시에서 로드: {imageKey}");
                    SetConstellationImage(cachedSprite);
                    return;
                }
                
                // Addressable에서 로드
                Sprite sprite = await Manager.data.LoadCharacterImageAsync(imageKey);
                if (sprite != null)
                {
                    Debug.Log($"[StoryPanel] 별자리 이미지 Addressable에서 로드: {imageKey}");
                    SetConstellationImage(sprite);
                }
                else
                {
                    Debug.LogWarning($"[StoryPanel] 별자리 이미지 로드 실패: {imageKey}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[StoryPanel] 별자리 이미지 로드 중 오류: {ex.Message}");
            }
        }

        public enum ImageType
        {
            Background,
            Constellation,
            StoryCharacter,
            DialogueLeftCharacter,
            DialogueRightCharacter,
            ChoiceLeftCharacter,
            CenterImage,
            All
        }
        
        #endregion
        
        #region 가운데 이미지 관리
        
        /// <summary>
        /// 가운데 이미지 표시 (페이드인/아웃 포함)
        /// </summary>
        public void ShowCenterImage(string imageKey, float duration = 3f, float fadeInTime = 0.0f, float fadeOutTime = 0.0f, bool hideCharacterImages = true, bool infinite = false)
        {
            // 빈 값이거나 null인 경우 기존 이미지 숨기기
            if (string.IsNullOrEmpty(imageKey))
            {
                Debug.Log("[StoryPanel] 가운데 이미지 키가 비어있음 - 기존 이미지 숨김");
                HideCenterImage();
                return;
            }
            
            // 이전 센터 이미지가 활성화되어 있으면 먼저 숨기기
            if (isCenterImageActive)
            {
                Debug.Log("[StoryPanel] 이전 센터 이미지가 활성화되어 있어서 먼저 숨김");
                HideCenterImage();
            }
            
            // 같은 이미지가 이미 표시 중인 경우
            if (isCenterImageActive && currentCenterImageKey == imageKey)
            {
                Debug.Log($"[StoryPanel] 같은 이미지가 이미 표시 중: {imageKey} - 연속 표시 모드");
                
                // 무한 표시가 아닌 경우에만 시간 연장
                if (!infinite)
                {
                    // 기존 코루틴을 중단하고 새로운 시간으로 재시작
                    if (centerImageCoroutine != null)
                    {
                        StopCoroutine(centerImageCoroutine);
                    }
                    centerImageCoroutine = StartCoroutine(ShowCenterImageCoroutine(imageKey, duration, fadeInTime, fadeOutTime, hideCharacterImages, infinite, true)); // 연속 표시 플래그
                }
                return;
            }
            
            // 다른 이미지이거나 처음 표시하는 경우
            if (centerImageCoroutine != null)
            {
                StopCoroutine(centerImageCoroutine);
            }
            
            currentCenterImageKey = imageKey;
            centerImageCoroutine = StartCoroutine(ShowCenterImageCoroutine(imageKey, duration, fadeInTime, fadeOutTime, hideCharacterImages, infinite, false)); // 새 이미지 플래그
        }
        
        /// <summary>
        /// 가운데 이미지 즉시 숨김
        /// </summary>
        public void HideCenterImage()
        {
            if (centerImageCoroutine != null)
            {
                StopCoroutine(centerImageCoroutine);
                centerImageCoroutine = null;
            }
            
            if (centerImage != null)
            {
                centerImage.gameObject.SetActive(false);
                centerImage.color = new Color(1f, 1f, 1f, 0f);
            }
            
            isCenterImageActive = false;
            currentCenterImageKey = ""; // 현재 이미지 키 초기화
            ShowCharacterImages(); // 캐릭터 이미지 다시 표시
        }
        
        /// <summary>
        /// 가운데 이미지 표시 코루틴
        /// </summary>
        private System.Collections.IEnumerator ShowCenterImageCoroutine(string imageKey, float duration, float fadeInTime, float fadeOutTime, bool hideCharacterImages, bool infinite, bool isContinuous = false)
        {
            isCenterImageActive = true;
            
            // 캐릭터 이미지 숨기기 (무한 모드이거나 hideCharacterImages가 true일 때)
            if (infinite || hideCharacterImages)
            {
                HideCharacterImages();
            }
            
            // 연속 표시가 아닌 경우에만 이미지 로드 및 설정
            if (!isContinuous)
            {
                // 이미지 로드
                Sprite sprite = null;
                
                // 먼저 캐시에서 확인
                sprite = Manager.data.GetCachedCharacterImage(imageKey);
                if (sprite == null)
                {
                    // Addressable에서 로드 (동기적으로 처리)
                    var loadTask = Manager.data.LoadCharacterImageAsync(imageKey);
                    yield return new WaitUntil(() => loadTask.IsCompleted);
                    
                    try
                    {
                        sprite = loadTask.Result;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[StoryPanel] 가운데 이미지 로드 중 오류: {ex.Message}");
                        yield break;
                    }
                }
                
                if (sprite == null)
                {
                    Debug.LogWarning($"[StoryPanel] 가운데 이미지 로드 실패: {imageKey}");
                    yield break;
                }
                
                // 이미지 설정
                centerImage.sprite = sprite;
                centerImage.gameObject.SetActive(true);
                centerImage.color = new Color(1f, 1f, 1f, 0f);
                
                // 페이드인
                yield return centerImage.DOFade(1f, fadeInTime).WaitForCompletion();
            }
            else
            {
                // 연속 표시인 경우 - 이미 표시 중이므로 페이드인 생략
                Debug.Log($"[StoryPanel] 연속 표시 모드 - 페이드인 생략: {imageKey}");
            }
            
            if (infinite)
            {
                // 무한 표시 - 수동으로 숨길 때까지 대기
                Debug.Log($"[StoryPanel] 가운데 이미지 무한 표시 모드: {imageKey}");
                yield return new WaitUntil(() => !isCenterImageActive); // 수동으로 숨길 때까지 대기
            }
            else
            {
                // 지정된 시간 동안 표시
                yield return new WaitForSeconds(duration);
            }
            
            // 페이드아웃
            yield return centerImage.DOFade(0f, fadeOutTime).WaitForCompletion();
            
            // 이미지 숨기기
            centerImage.gameObject.SetActive(false);
            isCenterImageActive = false;
            currentCenterImageKey = ""; // 현재 이미지 키 초기화
            
            // 캐릭터 이미지 다시 표시 (무한 모드가 아닐 때)
            if (!infinite)
            {
                ShowCharacterImages();
            }
            
            centerImageCoroutine = null;
        }
        
        /// <summary>
        /// 캐릭터 이미지들 숨기기
        /// </summary>
        private void HideCharacterImages()
        {
            if (storyRCharacterImage != null) storyRCharacterImage.gameObject.SetActive(false);
            if (dialogueLCharacterImage != null) dialogueLCharacterImage.gameObject.SetActive(false);
            if (dialogueRCharacterImage != null) dialogueRCharacterImage.gameObject.SetActive(false);
            if (choiceLCharacterImage != null) choiceLCharacterImage.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 캐릭터 이미지들 다시 표시
        /// </summary>
        private void ShowCharacterImages()
        {
            // 센터 이미지가 활성화되어 있으면 캐릭터 이미지 표시하지 않음
            if (isCenterImageActive)
            {
                Debug.Log("[StoryPanel] 센터 이미지가 활성화되어 있어서 캐릭터 이미지를 표시하지 않습니다.");
                return;
            }
            
            // CharacterImage가 설정되어 있는지 확인
            if (Manager.dialogue?.CurrentDialogueData != null)
            {
                string characterImage = Manager.dialogue.CurrentDialogueData.CharacterImage;
                if (string.IsNullOrEmpty(characterImage))
                {
                    Debug.Log("[StoryPanel] CharacterImage가 비어있어서 캐릭터 이미지를 표시하지 않습니다.");
                    HideAllCharacterImages();
                    return;
                }
                
                // 이미지 로딩이 실패했을 수 있으므로 실제로 이미지가 로드되었는지 확인
                // 이미지가 로딩되지 않았으면 숨김 상태를 유지
                Debug.Log($"[StoryPanel] ShowCharacterImages - CharacterImage: {characterImage}");
            }
            
            // 현재 대화 데이터에서 캐릭터 위치 정보 가져오기
            string characterPosition = "";
            if (Manager.dialogue?.CurrentDialogueData != null)
            {
                characterPosition = Manager.dialogue.CurrentDialogueData.CharacterImagePosition?.ToLower() ?? "";
            }
            
            // 이미지 로딩 상태를 명시적으로 확인
            if (!isCharacterImageLoaded)
            {
                Debug.Log("[StoryPanel] ShowCharacterImages - 이미지가 로딩되지 않았으므로 캐릭터 이미지를 표시하지 않습니다.");
                HideAllCharacterImages();
                return;
            }
            
            Debug.Log("[StoryPanel] ShowCharacterImages - 이미지가 로딩되었으므로 캐릭터 이미지를 표시합니다.");
            
            // 모든 캐릭터 이미지 먼저 숨기기
            if (storyRCharacterImage != null) storyRCharacterImage.gameObject.SetActive(false);
            if (dialogueLCharacterImage != null) dialogueLCharacterImage.gameObject.SetActive(false);
            if (dialogueRCharacterImage != null) dialogueRCharacterImage.gameObject.SetActive(false);
            if (choiceLCharacterImage != null) choiceLCharacterImage.gameObject.SetActive(false);
            
            // 현재 모드에 따라 다른 캐릭터 이미지 표시
            if (IsChoiceMode())
            {
                // 선택지 모드: 왼쪽 캐릭터 이미지 표시
                if (choiceLCharacterImage != null) choiceLCharacterImage.gameObject.SetActive(true);
            }
            else if (IsStoryMode())
            {
                // 스토리 모드: 오른쪽 캐릭터 이미지 표시
                if (storyRCharacterImage != null) storyRCharacterImage.gameObject.SetActive(true);
            }
            else if (IsDialogueMode())
            {
                // 대화 모드: 위치에 따라 해당 캐릭터만 표시
                switch (characterPosition)
                {
                    case "left":
                        if (dialogueLCharacterImage != null) dialogueLCharacterImage.gameObject.SetActive(true);
                        break;
                    case "right":
                        if (dialogueRCharacterImage != null) dialogueRCharacterImage.gameObject.SetActive(true);
                        break;
                    case "center":
                        if (storyRCharacterImage != null) storyRCharacterImage.gameObject.SetActive(true);
                        break;
                    default:
                        // 기본값: 오른쪽 캐릭터 표시
                        if (dialogueRCharacterImage != null) dialogueRCharacterImage.gameObject.SetActive(true);
                        break;
                }
            }
        }
        
        /// <summary>
        /// 가운데 이미지가 활성화되어 있는지 확인
        /// </summary>
        public bool IsCenterImageActive => isCenterImageActive;
        
        /// <summary>
        /// 가운데 이미지 수동 숨김 (무한 표시 모드에서 사용)
        /// </summary>
        public void ForceHideCenterImage()
        {
            if (isCenterImageActive)
            {
                isCenterImageActive = false; // 무한 대기 상태를 강제로 종료
                currentCenterImageKey = ""; // 현재 이미지 키 초기화
                HideCenterImage();
            }
        }
        
        #endregion
        
        #region 스킵 기능
        
        /// <summary>
        /// 스킵 모드 시작
        /// </summary>
        private void StartSkipMode()
        {
            if (isSkipMode) return;
            
            isSkipMode = true;
            Debug.Log($"[StoryPanel] 스킵 모드 활성화 - 속도: {GetCurrentSkipSpeed()}초");
            
            // 타이핑 효과 즉시 완료
            if (typingEffectManager != null)
            {
                // 현재 모드에 따라 적절한 텍스트 컴포넌트 선택
                TextMeshProUGUI targetText = GetCurrentDialogueText();
                if (targetText != null)
                {
                    typingEffectManager.CompleteTyping(targetText);
                }
            }
            
            
            // 자동 진행 시작
            StartAutoAdvance();
        }
        
        /// <summary>
        /// 스킵 모드 중지
        /// </summary>
        private void StopSkipMode()
        {
            if (!isSkipMode) return;
            
            isSkipMode = false;
            Debug.Log("[StoryPanel] 스킵 모드 비활성화");
            
            // 자동 진행 중지
            StopAutoAdvance();
            
        }
        
        
        /// <summary>
        /// 현재 스킵 속도 가져오기
        /// </summary>
        private float GetCurrentSkipSpeed()
        {
            if (currentSkipSpeedIndex >= 0 && currentSkipSpeedIndex < skipSpeedOptions.Length)
            {
                return skipSpeedOptions[currentSkipSpeedIndex];
            }
            return skipSpeedOptions[1]; // 기본값: 0.1초
        }
        
        /// <summary>
        /// 현재 모드에 따른 대화 텍스트 컴포넌트 가져오기
        /// </summary>
        private TextMeshProUGUI GetCurrentDialogueText()
        {
            if (IsChoiceMode())
            {
                return choiceQuestionText;
            }
            else if (IsStoryMode())
            {
                return storyChatText;
            }
            else if (IsDialogueMode())
            {
                return dialogueChatText;
            }
            
            // 기본값으로 대화 텍스트 반환
            return dialogueChatText;
        }
        
        
        /// <summary>
        /// 스킵 속도 설정 (설정에서만 사용)
        /// </summary>
        public void SetSkipSpeed(int speedIndex)
        {
            if (speedIndex >= 0 && speedIndex < skipSpeedOptions.Length)
            {
                currentSkipSpeedIndex = speedIndex;
                skipAutoAdvanceDelay = GetCurrentSkipSpeed();
                
                Debug.Log($"[StoryPanel] 스킵 속도 설정: {skipAutoAdvanceDelay}초");
                
                // 현재 스킵 모드가 활성화되어 있으면 속도 즉시 적용
                if (isSkipMode)
                {
                    StartAutoAdvance(); // 기존 코루틴 중지하고 새로운 속도로 재시작
                }
            }
        }
        
        /// <summary>
        /// 자동 진행 시작
        /// </summary>
        private void StartAutoAdvance()
        {
            if (autoAdvanceCoroutine != null)
            {
                StopCoroutine(autoAdvanceCoroutine);
            }
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine());
        }
        
        /// <summary>
        /// 자동 진행 중지
        /// </summary>
        private void StopAutoAdvance()
        {
            if (autoAdvanceCoroutine != null)
            {
                StopCoroutine(autoAdvanceCoroutine);
                autoAdvanceCoroutine = null;
            }
        }
        
        /// <summary>
        /// 자동 진행 코루틴
        /// </summary>
        private System.Collections.IEnumerator AutoAdvanceCoroutine()
        {
            while (isSkipMode)
            {
                yield return new WaitForSeconds(skipAutoAdvanceDelay);
                
                // 다음 노드로 이동
                if (Manager.dialogue != null && Manager.dialogue.IsDialogueActive)
                {
                // 타이핑 효과 즉시 완료
                if (typingEffectManager != null)
                {
                    // 현재 모드에 따라 적절한 텍스트 컴포넌트 선택
                    TextMeshProUGUI targetText = GetCurrentDialogueText();
                    if (targetText != null)
                    {
                        typingEffectManager.CompleteTyping(targetText);
                    }
                }
                    
                    // 다음 노드로 이동
                    Manager.dialogue.MoveToNextNode();
                }
                else
                {
                    // 대화가 끝났으면 스킵 모드 자동 해제 후 패널 닫기
                    StopSkipMode();
                    ClosePanel();
                    break;
                }
            }
        }
        
        /// <summary>
        /// 스킵 모드 상태 확인
        /// </summary>
        public bool IsSkipMode => isSkipMode;
        
        #endregion
        
        #endregion
    }
}