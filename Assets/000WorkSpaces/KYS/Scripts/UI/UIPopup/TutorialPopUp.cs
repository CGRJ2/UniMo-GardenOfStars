using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KYS
{
    /// <summary>
    /// TutorialPopup - DialogueManager와 연동하여 튜토리얼 텍스트를 표시하는 팝업
    /// </summary>
    public class TutorialPopUp : BaseUI
    {
        [Header("UI Element Names")]
        [SerializeField] private string tutorialTextName = "TutorialText";
        [SerializeField] private string closeButtonName = "CloseButton";
        [SerializeField] private string nextButtonName = "NextButton";

        #region UI Element References
        private TextMeshProUGUI tutorialText => GetUI<TextMeshProUGUI>(tutorialTextName);
        private Button closeButton => GetUI<Button>(closeButtonName);
        private Button nextButton => GetUI<Button>(nextButtonName);
        #endregion

        [Header("Tutorial Settings")]
        [SerializeField] private string tutorialNpcId = "tutorial";
        [SerializeField] private string tutorialStageId = "tutorial_stage";
        [SerializeField] private string tutorialNodeId = ""; // 직접 노드 ID 설정

        [Header("Position Settings")]
        [SerializeField] private bool useCustomPosition = false;
        [SerializeField] private Vector2 customPosition = Vector2.zero;
        [SerializeField] private TutorialPositionType positionType = TutorialPositionType.Center;

        [Header("Size Settings")]
        [SerializeField] private bool useCustomSize = false;
        [SerializeField] private Vector2 customSize = new Vector2(400, 300);
        [SerializeField] private TutorialSizeType sizeType = TutorialSizeType.Medium;

        [Header("Close Settings")]
        [SerializeField] private bool canCloseWithPanelClick = true; // 패널 클릭으로 닫기 가능 여부

        private DialogueData currentDialogueData;

        public enum TutorialPositionType
        {
            Center,      // 화면 중앙
            Top,         // 화면 상단
            Bottom,      // 화면 하단
            Left,        // 화면 왼쪽
            Right,       // 화면 오른쪽
            TopLeft,     // 화면 좌상단
            TopRight,    // 화면 우상단
            BottomLeft,  // 화면 좌하단
            BottomRight, // 화면 우하단
            Custom       // 사용자 지정 위치
        }

        public enum TutorialSizeType
        {
            Small,       // 작은 크기
            Medium,      // 중간 크기
            Large,       // 큰 크기
            FullScreen,  // 전체 화면
            Custom       // 사용자 지정 크기
        }

        protected override void Awake()
        {
            base.Awake();
            
            // Popup으로 설정
            layerType = UILayerType.Popup;
            
            // Backdrop 클릭으로 닫기 비활성화
            canCloseWithBackdrop = false;
            canCloseWithESC = true;
            
            // Backdrop 생성 활성화
            createBackdropForPopup = true;
        }

        protected override void OnShow()
        {
            base.OnShow();
            
            // 위치 및 크기 설정 적용
            ApplyPosition();
            ApplySize();
        }

        /// <summary>
        /// Backdrop 생성 후 설정을 적용하는 메서드
        /// </summary>
        protected override void SetupBackdropClickEvent()
        {
            base.SetupBackdropClickEvent();
            
            // Backdrop 투명하게 설정
            if (ownBackdrop != null)
            {
                ownBackdrop.SetBackdropColor(new Color(0, 0, 0, 0f)); // 완전 투명
                ownBackdrop.SetBackdropClickable(false); // 클릭 비활성화
                ownBackdrop.SetRaycastTarget(false); // RaycastTarget 비활성화 (터치 이벤트 차단하지 않음)
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
            SetupPanelClick();
            StartTutorialDialogue();
        }

        private void SetupButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextButtonClicked);
            }
        }

        private void SetupPanelClick()
        {
            if (!canCloseWithPanelClick) return;

            // 패널에 Button 컴포넌트가 없으면 추가
            Button panelButton = GetComponent<Button>();
            if (panelButton == null)
            {
                panelButton = gameObject.AddComponent<Button>();
            }

            // 패널 클릭 이벤트 설정
            panelButton.onClick.AddListener(OnPanelClicked);
        }

        /// <summary>
        /// 패널 클릭 시 호출되는 메서드
        /// </summary>
        private void OnPanelClicked()
        {
            Debug.Log("[TutorialPopUp] 패널 클릭으로 튜토리얼 종료");
            ForceEndTutorial();
        }

        private void StartTutorialDialogue()
        {
            // DialogueManager 이벤트 구독
            Manager.dialogue.OnDialogueStarted += OnDialogueStarted;
            Manager.dialogue.OnDialogueCompleted += OnDialogueCompleted;
            Manager.dialogue.OnDialogueNodeChanged += OnDialogueNodeChanged;

            // 튜토리얼 대화 시작
            if (!string.IsNullOrEmpty(tutorialNodeId))
            {
                // 직접 노드 ID가 설정된 경우 해당 노드로 이동
                Manager.dialogue.MoveToNode(tutorialNodeId);
            }
            else
            {
                // NPC와 Stage로 대화 시작
                Manager.dialogue.StartDialogue(tutorialNpcId, tutorialStageId);
            }
        }

        private void OnDialogueStarted(DialogueData dialogueData)
        {
            currentDialogueData = dialogueData;
            UpdateTutorialText();
        }

        private void OnDialogueCompleted(DialogueData dialogueData)
        {
            // 튜토리얼 완료 시 팝업 닫기
            Manager.ui.ClosePopup();
        }

        private void OnDialogueNodeChanged(string nodeId)
        {
            UpdateTutorialText();
        }

        private void UpdateTutorialText()
        {
            if (tutorialText != null && currentDialogueData != null)
            {
                // 다국어 지원된 대화 텍스트 표시
                tutorialText.text = currentDialogueData.GetLocalizedDialogueText();
            }

            // Next 버튼 표시/숨김 처리 (다음 노드가 있으면 표시)
            if (nextButton != null)
            {
                bool hasNextNode = !string.IsNullOrEmpty(currentDialogueData?.NextNodeId);
                nextButton.gameObject.SetActive(hasNextNode);
            }
        }

        private void OnNextButtonClicked()
        {
            // 다음 노드로 이동
            Manager.dialogue.MoveToNextNode();
        }

        private void OnCloseButtonClicked()
        {
            // 대화 종료
            Manager.dialogue.EndDialogue();
            Manager.ui.ClosePopup();
        }

        public override void Cleanup()
        {
            // DialogueManager 이벤트 구독 해제
            if (Manager.dialogue != null)
            {
                Manager.dialogue.OnDialogueStarted -= OnDialogueStarted;
                Manager.dialogue.OnDialogueCompleted -= OnDialogueCompleted;
                Manager.dialogue.OnDialogueNodeChanged -= OnDialogueNodeChanged;
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();
            }

            // 패널 클릭 이벤트 해제
            Button panelButton = GetComponent<Button>();
            if (panelButton != null)
            {
                panelButton.onClick.RemoveAllListeners();
            }

            base.Cleanup();
        }

        /// <summary>
        /// 외부에서 튜토리얼 NPC ID와 Stage ID 설정
        /// </summary>
        public void SetTutorialDialogue(string npcId, string stageId)
        {
            tutorialNpcId = npcId;
            tutorialStageId = stageId;
            tutorialNodeId = ""; // 노드 ID 초기화
        }

        /// <summary>
        /// 외부에서 직접 노드 ID 설정
        /// </summary>
        public void SetTutorialNode(string nodeId)
        {
            tutorialNodeId = nodeId;
            tutorialNpcId = ""; // NPC ID 초기화
            tutorialStageId = ""; // Stage ID 초기화
        }

        /// <summary>
        /// 외부에서 튜토리얼 설정 (NPC/Stage 또는 직접 노드)
        /// </summary>
        public void SetTutorial(string npcId = "", string stageId = "", string nodeId = "")
        {
            if (!string.IsNullOrEmpty(nodeId))
            {
                SetTutorialNode(nodeId);
            }
            else if (!string.IsNullOrEmpty(npcId) && !string.IsNullOrEmpty(stageId))
            {
                SetTutorialDialogue(npcId, stageId);
            }
        }

        /// <summary>
        /// 플레이어가 튜토리얼 행동을 완료했을 때 호출
        /// </summary>
        public void CompleteTutorialAction()
        {
            // 다음 노드로 이동 (자동으로 다음 대사 표시)
            if (currentDialogueData != null && !string.IsNullOrEmpty(currentDialogueData.NextNodeId))
            {
                Manager.dialogue.MoveToNextNode();
            }
            else
            {
                // 다음 노드가 없으면 튜토리얼 완료
                CompleteTutorial();
            }
        }

        /// <summary>
        /// 튜토리얼 완료 처리
        /// </summary>
        public void CompleteTutorial()
        {
            // 대화 종료
            Manager.dialogue.EndDialogue();
            
            // 팝업 닫기
            Manager.ui.ClosePopup();
        }

        /// <summary>
        /// 튜토리얼을 강제로 종료 (플레이어가 중단하고 싶을 때)
        /// </summary>
        public void ForceEndTutorial()
        {
            // 대화 강제 종료
            Manager.dialogue.EndDialogue();
            
            // 팝업 닫기
            Manager.ui.ClosePopup();
        }

        #region Position and Size Methods

        /// <summary>
        /// 위치 설정 적용
        /// </summary>
        private void ApplyPosition()
        {
            if (!useCustomPosition) return;

            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;

            Vector2 targetPosition = GetPositionByType();
            rectTransform.anchoredPosition = targetPosition;
        }

        /// <summary>
        /// 크기 설정 적용
        /// </summary>
        private void ApplySize()
        {
            if (!useCustomSize) return;

            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;

            Vector2 targetSize = GetSizeByType();
            rectTransform.sizeDelta = targetSize;
        }

        /// <summary>
        /// 위치 타입에 따른 위치 계산
        /// </summary>
        private Vector2 GetPositionByType()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            Canvas canvas = GetComponentInParent<Canvas>();
            
            if (canvas == null) return Vector2.zero;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.sizeDelta;

            switch (positionType)
            {
                case TutorialPositionType.Center:
                    return Vector2.zero;
                case TutorialPositionType.Top:
                    return new Vector2(0, canvasSize.y * 0.25f);
                case TutorialPositionType.Bottom:
                    return new Vector2(0, -canvasSize.y * 0.25f);
                case TutorialPositionType.Left:
                    return new Vector2(-canvasSize.x * 0.25f, 0);
                case TutorialPositionType.Right:
                    return new Vector2(canvasSize.x * 0.25f, 0);
                case TutorialPositionType.TopLeft:
                    return new Vector2(-canvasSize.x * 0.25f, canvasSize.y * 0.25f);
                case TutorialPositionType.TopRight:
                    return new Vector2(canvasSize.x * 0.25f, canvasSize.y * 0.25f);
                case TutorialPositionType.BottomLeft:
                    return new Vector2(-canvasSize.x * 0.25f, -canvasSize.y * 0.25f);
                case TutorialPositionType.BottomRight:
                    return new Vector2(canvasSize.x * 0.25f, -canvasSize.y * 0.25f);
                case TutorialPositionType.Custom:
                    return customPosition;
                default:
                    return Vector2.zero;
            }
        }

        /// <summary>
        /// 크기 타입에 따른 크기 계산
        /// </summary>
        private Vector2 GetSizeByType()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return new Vector2(400, 300);

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.sizeDelta;

            switch (sizeType)
            {
                case TutorialSizeType.Small:
                    return new Vector2(canvasSize.x * 0.3f, canvasSize.y * 0.3f);
                case TutorialSizeType.Medium:
                    return new Vector2(canvasSize.x * 0.5f, canvasSize.y * 0.5f);
                case TutorialSizeType.Large:
                    return new Vector2(canvasSize.x * 0.7f, canvasSize.y * 0.7f);
                case TutorialSizeType.FullScreen:
                    return new Vector2(canvasSize.x * 0.9f, canvasSize.y * 0.9f);
                case TutorialSizeType.Custom:
                    return customSize;
                default:
                    return new Vector2(400, 300);
            }
        }

        /// <summary>
        /// 튜토리얼 위치 설정
        /// </summary>
        public void SetTutorialPosition(TutorialPositionType position)
        {
            positionType = position;
            useCustomPosition = true;
            ApplyPosition();
        }

        /// <summary>
        /// 커스텀 위치 설정
        /// </summary>
        public void SetCustomPosition(Vector2 position)
        {
            customPosition = position;
            positionType = TutorialPositionType.Custom;
            useCustomPosition = true;
            ApplyPosition();
        }

        /// <summary>
        /// 튜토리얼 크기 설정
        /// </summary>
        public void SetTutorialSize(TutorialSizeType size)
        {
            sizeType = size;
            useCustomSize = true;
            ApplySize();
        }

        /// <summary>
        /// 커스텀 크기 설정
        /// </summary>
        public void SetCustomSize(Vector2 size)
        {
            customSize = size;
            sizeType = TutorialSizeType.Custom;
            useCustomSize = true;
            ApplySize();
        }

        /// <summary>
        /// 위치 설정 비활성화 (기본 중앙 위치)
        /// </summary>
        public void ResetPosition()
        {
            useCustomPosition = false;
            positionType = TutorialPositionType.Center;
        }

        /// <summary>
        /// 크기 설정 비활성화 (기본 중간 크기)
        /// </summary>
        public void ResetSize()
        {
            useCustomSize = false;
            sizeType = TutorialSizeType.Medium;
        }

        /// <summary>
        /// 패널 클릭으로 닫기 기능 설정
        /// </summary>
        public void SetPanelClickable(bool clickable)
        {
            canCloseWithPanelClick = clickable;
            
            Button panelButton = GetComponent<Button>();
            if (panelButton != null)
            {
                panelButton.onClick.RemoveAllListeners();
                
                if (clickable)
                {
                    panelButton.onClick.AddListener(OnPanelClicked);
                }
            }
        }

        #endregion
    }
}