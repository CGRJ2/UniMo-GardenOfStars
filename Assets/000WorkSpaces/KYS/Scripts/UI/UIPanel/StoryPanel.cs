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
        private TextMeshProUGUI dialogueLCharacterNameText => GetUI<TextMeshProUGUI>("RunLDialogueCaracterNameText");
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
            // 타이핑 효과 정리
            if (typingEffectManager != null)
            {
                typingEffectManager.StopTyping();
            }
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
            
            // 타이핑이 방금 완료된 상태라면 다음 클릭을 위해 대기
            if (typingEffectManager.IsTypingCompleted)
            {
                Debug.Log("[StoryPanel] 타이핑 완료됨 - 다음 클릭 대기");
                typingEffectManager.ResetTypingCompleted(); // 다음 클릭을 위해 초기화
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
        
        #region 노드 기반 대화 시스템
        
        /// <summary>
        /// 노드 기반 대화 시작
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