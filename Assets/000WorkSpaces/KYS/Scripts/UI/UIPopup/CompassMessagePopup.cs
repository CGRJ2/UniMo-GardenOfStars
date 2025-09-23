using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KYS
{
    /// <summary>
    /// TutorialPopup - DialogueManager와 연동하여 튜토리얼 텍스트를 표시하는 팝업
    /// </summary>
    public class CompassMessagePopup : BaseUI
    {
        [Header("UI Element Names")]
        [SerializeField] private string compassTextName = "MessageText";


        #region UI Element References
        private TextMeshProUGUI compassText => GetUI<TextMeshProUGUI>(compassTextName);

        #endregion

        [Header("Compass Settings")]
        [SerializeField] private string compassNpcId = "tutorial";
        [SerializeField] private string compassStageId = "tutorial_stage";
        [SerializeField] private string compassNodeId = ""; // 직접 노드 ID 설정

        [Header("Position Settings")]
        [SerializeField] private bool useCustomPosition = false;
        [SerializeField] private Vector2 customPosition = Vector2.zero;
        [SerializeField] private CompassPositionType positionType = CompassPositionType.Center;

        [Header("Size Settings")]
        [SerializeField] private bool useCustomSize = false;
        [SerializeField] private Vector2 customSize = new Vector2(400, 300);
        [SerializeField] private CompassSizeType sizeType = CompassSizeType.Medium;

        [Header("Close Settings")]
        [SerializeField] private bool canCloseWithPanelClick = true; // 패널 클릭으로 닫기 가능 여부

        private DialogueData currentDialogueData;

        public enum CompassPositionType
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

        public enum CompassSizeType
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
            //StartTutorialDialogue(); 는 SetTutorialNode()에서 호출하도록 변경
            SetupPanelClick();
        }

        private void SetupButtons()
        {

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
            Debug.Log("[CompassMessagePopup] 패널 클릭으로 컴퍼스 종료");
            ForceEndCompass();
        }

        private void StartCompassDialogue()
        {
            // 컴퍼스는 독립적인 대화 시스템을 사용 (DialogueManager와 분리)
            // 직접 노드 데이터를 가져와서 표시
            if (!string.IsNullOrEmpty(compassNodeId))
            {
                LoadCompassNodeData(compassNodeId);
            }
            else
            {
                Debug.LogError("[CompassMessagePopup] 컴퍼스 노드 ID가 설정되지 않았습니다.");
            }
        }

        /// <summary>
        /// 컴퍼스 노드 데이터를 직접 로드하여 표시
        /// </summary>
        private void LoadCompassNodeData(string nodeId)
        {
            try
            {
                // DataManager에서 직접 노드 데이터 가져오기
                if (Manager.data?.Dialogue?.Values == null)
                {
                    Debug.LogError("[CompassMessagePopup] Dialogue 데이터가 로드되지 않았습니다.");
                    return;
                }

                if (!Manager.data.Dialogue.Values.TryGetValue(nodeId, out DialogueDataCsv csvData))
                {
                    Debug.LogError($"[CompassMessagePopup] 노드 '{nodeId}'를 찾을 수 없습니다.");
                    return;
                }

                // DialogueData 생성
                currentDialogueData = GetOrCreateDialogueData(nodeId);
                if (currentDialogueData == null)
                {
                    Debug.LogError($"[CompassMessagePopup] 노드 {nodeId}의 데이터를 생성할 수 없습니다.");
                    return;
                }

                Debug.Log($"[CompassMessagePopup] 컴퍼스 노드 로드 완료: {nodeId}");
                
                // 텍스트 업데이트
                UpdateCompassText();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CompassMessagePopup] 컴퍼스 노드 로드 실패: {e.Message}");
            }
        }

        /// <summary>
        /// DialogueData 생성 또는 가져오기
        /// </summary>
        private DialogueData GetOrCreateDialogueData(string nodeId)
        {
            // DialogueData는 FirebaseData를 상속하므로 올바른 생성자 사용
            // parentPath는 Firebase 경로 (일반적으로 "Dialogue" 또는 null)
            DialogueData dialogueData = new DialogueData(nodeId, "Dialogue");
            
            return dialogueData;
        }

        // DialogueManager 이벤트는 더 이상 사용하지 않음 (독립적인 시스템)

        private void UpdateCompassText()
        {
            if (compassText != null && currentDialogueData != null)
            {
                // 다국어 지원된 대화 텍스트 표시
                compassText.text = currentDialogueData.GetLocalizedDialogueText();
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
            // 패널 클릭 이벤트 해제
            Button panelButton = GetComponent<Button>();
            if (panelButton != null)
            {
                panelButton.onClick.RemoveAllListeners();
            }

            base.Cleanup();
        }

        /// <summary>
        /// 외부에서 컴퍼스 NPC ID와 Stage ID 설정
        /// </summary>
        public void SetCompassDialogue(string npcId, string stageId)
        {
            compassNpcId = npcId;
            compassStageId = stageId;
            compassNodeId = ""; // 노드 ID 초기화
        }

        /// <summary>
        /// 외부에서 직접 노드 ID 설정
        /// </summary>
        public void SetCompassNode(string nodeId)
        {
            compassNodeId = nodeId;
            compassNpcId = ""; // NPC ID 초기화
            compassStageId = ""; // Stage ID 초기화
            
            // 노드 ID 설정 후 대화 시작
            StartCompassDialogue();
        }

        /// <summary>
        /// 외부에서 컴퍼스 설정 (NPC/Stage 또는 직접 노드)
        /// </summary>
        public void SetCompass(string npcId = "", string stageId = "", string nodeId = "")
        {
            if (!string.IsNullOrEmpty(nodeId))
            {
                SetCompassNode(nodeId);
            }
            else if (!string.IsNullOrEmpty(npcId) && !string.IsNullOrEmpty(stageId))
            {
                SetCompassDialogue(npcId, stageId);
            }
        }

        /// <summary>
        /// 플레이어가 컴퍼스 행동을 완료했을 때 호출
        /// </summary>
        public void CompleteCompassAction()
        {
            // 다음 노드로 이동 (자동으로 다음 대사 표시)
            if (currentDialogueData != null && !string.IsNullOrEmpty(currentDialogueData.NextNodeId))
            {
                Manager.dialogue.MoveToNextNode();
            }
            else
            {
                // 다음 노드가 없으면 컴퍼스 완료
                CompleteCompass();
            }
        }

        /// <summary>
        /// 컴퍼스 완료 처리
        /// </summary>
        public void CompleteCompass()
        {
            // 컴퍼스는 독립적인 시스템이므로 DialogueManager와 무관하게 처리
            Debug.Log("[CompassMessagePopup] 컴퍼스 완료");
            
            // 팝업 닫기
            Manager.ui.ClosePopup();
        }

        /// <summary>
        /// 컴퍼스를 강제로 종료 (플레이어가 중단하고 싶을 때)
        /// </summary>
        public void ForceEndCompass()
        {
            Debug.Log("[CompassMessagePopup] 컴퍼스 강제 종료");
            
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
                case CompassPositionType.Center:
                    return Vector2.zero;
                case CompassPositionType.Top:
                    return new Vector2(0, canvasSize.y * 0.25f);
                case CompassPositionType.Bottom:
                    return new Vector2(0, -canvasSize.y * 0.25f);
                case CompassPositionType.Left:
                    return new Vector2(-canvasSize.x * 0.25f, 0);
                case CompassPositionType.Right:
                    return new Vector2(canvasSize.x * 0.25f, 0);
                case CompassPositionType.TopLeft:
                    return new Vector2(-canvasSize.x * 0.25f, canvasSize.y * 0.25f);
                case CompassPositionType.TopRight:
                    return new Vector2(canvasSize.x * 0.25f, canvasSize.y * 0.25f);
                case CompassPositionType.BottomLeft:
                    return new Vector2(-canvasSize.x * 0.25f, -canvasSize.y * 0.25f);
                case CompassPositionType.BottomRight:
                    return new Vector2(canvasSize.x * 0.25f, -canvasSize.y * 0.25f);
                case CompassPositionType.Custom:
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
                case CompassSizeType.Small:
                    return new Vector2(canvasSize.x * 0.3f, canvasSize.y * 0.3f);
                case CompassSizeType.Medium:
                    return new Vector2(canvasSize.x * 0.5f, canvasSize.y * 0.5f);
                case CompassSizeType.Large:
                    return new Vector2(canvasSize.x * 0.7f, canvasSize.y * 0.7f);
                case CompassSizeType.FullScreen:
                    return new Vector2(canvasSize.x * 0.9f, canvasSize.y * 0.9f);
                case CompassSizeType.Custom:
                    return customSize;
                default:
                    return new Vector2(400, 300);
            }
        }

        /// <summary>
        /// 컴퍼스 위치 설정
        /// </summary>
        public void SetCompassPosition(CompassPositionType position)
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
            positionType = CompassPositionType.Custom;
            useCustomPosition = true;
            ApplyPosition();
        }

        /// <summary>
        /// 컴퍼스 크기 설정
        /// </summary>
        public void SetCompassSize(CompassSizeType size)
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
            sizeType = CompassSizeType.Custom;
            useCustomSize = true;
            ApplySize();
        }

        /// <summary>
        /// 위치 설정 비활성화 (기본 중앙 위치)
        /// </summary>
        public void ResetPosition()
        {
            useCustomPosition = false;
            positionType = CompassPositionType.Center;
        }

        /// <summary>
        /// 크기 설정 비활성화 (기본 중간 크기)
        /// </summary>
        public void ResetSize()
        {
            useCustomSize = false;
            sizeType = CompassSizeType.Medium;
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