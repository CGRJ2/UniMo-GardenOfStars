using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    /// <summary>
    /// 터치/클릭으로 생성되는 정보 HUD UI
    /// </summary>
    public class TouchInfoHUD : BaseUI
    {
        #region UI Element Names
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string titleTextName = "titleText";
        [SerializeField] private string descriptionTextName = "descriptionText";
        [SerializeField] private string iconImageName = "iconImage";
        [SerializeField] private string closeButtonName = "closeButton";
        [SerializeField] private string actionButtonName = "actionButton";
        [SerializeField] private string closeButtonTextName = "closeButtonText";
        [SerializeField] private string actionButtonTextName = "actionButtonText";
        
        #endregion
        
        #region UI Element References (동적 참조)
        // UI 요소 참조 (GetUI<T>() 메서드로 동적 참조)
        private TextMeshProUGUI titleText => GetUI<TextMeshProUGUI>(titleTextName);
        private TextMeshProUGUI descriptionText => GetUI<TextMeshProUGUI>(descriptionTextName);
        private Image iconImage => GetUI<Image>(iconImageName);
        private Button closeButton => GetUI<Button>(closeButtonName);
        private Button actionButton => GetUI<Button>(actionButtonName);
        private TextMeshProUGUI closeButtonText => GetUI<TextMeshProUGUI>(closeButtonTextName);
        private TextMeshProUGUI actionButtonText => GetUI<TextMeshProUGUI>(actionButtonTextName);
        #endregion
        
        [Header("HUD Settings")]
        //[SerializeField] private float hudOffset = 50f; // 터치 위치로부터의 오프셋
        [SerializeField] private float maxDistanceFromEdge = 100f; // 화면 가장자리로부터 최대 거리
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        
        private Vector2 touchPosition;
        private RectTransform hudRectTransform;
        private Canvas parentCanvas;
        
        // HUD Backdrop 관리
        private HUDBackdropUI hudBackdrop;

        protected override void Awake()
        {
            base.Awake();

            // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
            if (layerType == UILayerType.Panel) // BaseUI의 기본값
            {
                layerType = UILayerType.HUD;
            }


        }
        
        protected override string[] GetAutoLocalizeKeys()
        {
            return new string[] {
                titleTextName,      // titleText -> info_hud_title
                descriptionTextName, // descriptionText -> info_hud_description  
                closeButtonTextName, // closeButtonText -> info_hud_close
                actionButtonTextName // actionButtonText -> info_hud_action
            };
        }
        
        public override void Show()
        {
            base.Show();
            
            if (enableDebugLogs)
                Debug.Log("[TouchInfoHUD] HUD 표시 시작");
            
            // HUD Backdrop 생성
            CreateHUDBackdrop();
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            if (enableDebugLogs)
                Debug.Log("[TouchInfoHUD] 정보 HUD 초기화");
            
            SetupHUD();
            SetupButtons();
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            
            if (enableDebugLogs)
                Debug.Log("[TouchInfoHUD] 정보 HUD 정리");
        }
        
        /// <summary>
        /// InfoHUD 완전 파괴 (메모리 누수 방지)
        /// </summary>
        public override void Hide()
        {
            if (!IsActive) 
            {
                Debug.Log("[TouchInfoHUD] HUD가 이미 비활성화되어 있음");
                return;
            }
            
            Debug.Log("[TouchInfoHUD] HUD 완전 파괴 시작");
            
            // HUD Backdrop 제거
            RemoveHUDBackdrop();
            
            // UI 닫기 사운드 재생
            PlayCloseSound();
            
            if (useAnimation)
            {
                Debug.Log("[TouchInfoHUD] 애니메이션으로 HUD 파괴");
                PlayHideAnimation(() => {
                    // 애니메이션 완료 후 완전히 파괴
                    Debug.Log("[TouchInfoHUD] 애니메이션 완료 후 HUD 파괴");
                    OnHide();
                    
                    // UIManager에서 등록 해제
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.UnregisterUI(this);
                    }
                    
                    // GameObject 완전 파괴
                    Destroy(gameObject);
                });
            }
            else
            {
                Debug.Log("[TouchInfoHUD] 즉시 HUD 파괴");
                OnHide();
                
                // UIManager에서 등록 해제
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UnregisterUI(this);
                }
                
                // GameObject 완전 파괴
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// HUD 설정
        /// </summary>
        private void SetupHUD()
        {
            hudRectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
            
            if (hudRectTransform == null)
            {
                Debug.LogError("[TouchInfoHUD] RectTransform을 찾을 수 없습니다.");
                return;
            }
            
            if (parentCanvas == null)
            {
                Debug.LogError("[TouchInfoHUD] 부모 Canvas를 찾을 수 없습니다.");
                return;
            }
        }
        
        /// <summary>
        /// 버튼 설정
        /// </summary>
        private void SetupButtons()
        {
            // 닫기 버튼
            if (closeButton != null)
            {
                GetEventWithSFX(closeButtonName, "SFX_ButtonClickBack").Click += (data) => OnCloseClicked();
            }
            
            // 액션 버튼
            if (actionButton != null)
            {
                GetEventWithSFX(actionButtonName, "SFX_ButtonClick").Click += (data) => OnActionClicked();
            }
        }
        
        /// <summary>
        /// HUD 위치 설정
        /// </summary>
        public void SetHUDPosition(Vector2 screenPosition)
        {
            touchPosition = screenPosition;
            
            if (hudRectTransform == null || parentCanvas == null)
                return;
            
            // 화면 좌표를 캔버스 좌표로 변환
            Vector2 canvasPosition = ScreenToCanvasPosition(screenPosition);
            
            // HUD가 화면 밖으로 나가지 않도록 조정
            Vector2 adjustedPosition = ClampPositionToScreen(canvasPosition);
            
            // HUD 위치 설정
            hudRectTransform.anchoredPosition = adjustedPosition;
            
            if (enableDebugLogs)
                Debug.Log($"[TouchInfoHUD] HUD 위치 설정: {adjustedPosition}");
        }
        
        /// <summary>
        /// 화면 좌표를 캔버스 좌표로 변환
        /// </summary>
        private Vector2 ScreenToCanvasPosition(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPosition,
                parentCanvas.worldCamera,
                out Vector2 localPoint);
            
            return localPoint;
        }
        
        /// <summary>
        /// HUD가 화면 밖으로 나가지 않도록 위치 조정
        /// </summary>
        private Vector2 ClampPositionToScreen(Vector2 position)
        {
            Vector2 hudSize = hudRectTransform.sizeDelta;
            Vector2 canvasSize = (parentCanvas.transform as RectTransform).sizeDelta;
            
            // HUD 크기의 절반
            Vector2 halfHUDSize = hudSize * 0.5f;
            
            // 화면 경계 계산
            float minX = -canvasSize.x * 0.5f + halfHUDSize.x + maxDistanceFromEdge;
            float maxX = canvasSize.x * 0.5f - halfHUDSize.x - maxDistanceFromEdge;
            float minY = -canvasSize.y * 0.5f + halfHUDSize.y + maxDistanceFromEdge;
            float maxY = canvasSize.y * 0.5f - halfHUDSize.y - maxDistanceFromEdge;
            
            // 위치 클램핑
            float clampedX = Mathf.Clamp(position.x, minX, maxX);
            float clampedY = Mathf.Clamp(position.y, minY, maxY);
            
            return new Vector2(clampedX, clampedY);
        }
        
        /// <summary>
        /// HUD 정보 설정
        /// </summary>
        public void SetInfo(string title, string description, Sprite icon = null)
        {
            if (titleText != null)
                titleText.text = title;
            
            if (descriptionText != null)
                descriptionText.text = description;
            
            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }
            else if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }
            
            if (enableDebugLogs)
                Debug.Log($"[TouchInfoHUD] 정보 설정: {title} - {description}");
        }
        
        /// <summary>
        /// 액션 버튼 텍스트 설정
        /// </summary>
        public void SetActionButtonText(string text)
        {
            if (actionButton != null)
            {
                if (actionButtonText != null)
                    actionButtonText.text = text;
                else
                {
                    TextMeshProUGUI buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                        buttonText.text = text;
                }
            }
        }
        
        /// <summary>
        /// 액션 버튼 활성화/비활성화
        /// </summary>
        public void SetActionButtonActive(bool active)
        {
            if (actionButton != null)
                actionButton.gameObject.SetActive(active);
        }
        
        /// <summary>
        /// 닫기 버튼 클릭
        /// </summary>
        private void OnCloseClicked()
        {
            if (enableDebugLogs)
                Debug.Log("[TouchInfoHUD] 닫기 버튼 클릭");
            
            Hide();
        }
        
        /// <summary>
        /// 액션 버튼 클릭
        /// </summary>
        private void OnActionClicked()
        {
            if (enableDebugLogs)
                Debug.Log("[TouchInfoHUD] 액션 버튼 클릭");
            
            // 여기에 액션 로직 추가
            // 예: 건물 업그레이드, 생산 시작 등
        }
        
        /// <summary>
        /// 정적 메서드로 HUD 생성 (UIManager를 통해)
        /// </summary>
        public static void ShowInfoHUD(Vector2 screenPosition, string title, string description, Sprite icon = null)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSingleInfoHUDAsync<TouchInfoHUD>(screenPosition, title, description, icon);
            }
        }
        
        /// <summary>
        /// HUD용 Backdrop 생성
        /// </summary>
        private void CreateHUDBackdrop()
        {
            if (hudBackdrop != null)
            {
                if (enableDebugLogs)
                    Debug.Log("[TouchInfoHUD] 이미 HUD Backdrop가 존재합니다.");
                return;
            }

            // HUDCanvas 가져오기
            Canvas hudCanvas = UIManager.Instance?.GetCanvasByLayer(UILayerType.HUD);
            if (hudCanvas == null)
            {
                Debug.LogError("[TouchInfoHUD] HUDCanvas를 찾을 수 없습니다.");
                return;
            }

            // HUD Backdrop GameObject 생성 (HUDCanvas의 자식으로)
            GameObject backdropGO = new GameObject("HUDBackdrop");
            backdropGO.transform.SetParent(hudCanvas.transform);
            
            if (enableDebugLogs)
                Debug.Log("[TouchInfoHUD] HUD Backdrop GameObject 생성 완료");

            // RectTransform 설정 (전체 화면)
            RectTransform backdropRect = backdropGO.AddComponent<RectTransform>();
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;
            backdropRect.localScale = Vector3.one;
            
            // HUDBackdropUI 컴포넌트 추가 및 저장
            hudBackdrop = backdropGO.AddComponent<HUDBackdropUI>();
            
            // HUD를 Backdrop의 자식으로 이동
            transform.SetParent(backdropGO.transform);
            
            // Backdrop를 HUDCanvas의 최하단으로 이동 (하이어라키 순서 조정)
            backdropGO.transform.SetAsFirstSibling();
            
            // Backdrop 클릭 이벤트 설정
            SetupHUDBackdropClickEvent();
            
            if (enableDebugLogs)
                Debug.Log("[TouchInfoHUD] HUD Backdrop 설정 완료");
        }

        /// <summary>
        /// HUD Backdrop 클릭 이벤트 설정
        /// </summary>
        private void SetupHUDBackdropClickEvent()
        {
            if (hudBackdrop == null) return;
            
            hudBackdrop.OnBackdropClicked += () =>
            {
                if (enableDebugLogs)
                    Debug.Log("[TouchInfoHUD] HUD Backdrop 클릭으로 HUD 닫기");
                Hide();
            };
            
            if (enableDebugLogs)
                Debug.Log("[TouchInfoHUD] HUD Backdrop 클릭 이벤트 설정 완료");
        }

        /// <summary>
        /// HUD Backdrop 제거
        /// </summary>
        private void RemoveHUDBackdrop()
        {
            if (hudBackdrop != null)
            {
                if (enableDebugLogs)
                    Debug.Log("[TouchInfoHUD] HUD Backdrop 제거");
                
                // HUD를 원래 부모로 이동
                transform.SetParent(hudBackdrop.transform.parent);
                
                // Backdrop 파괴
                Destroy(hudBackdrop.gameObject);
                hudBackdrop = null;
            }
        }
        
        /// <summary>
        /// UI 요소 정보 디버그 출력
        /// </summary>
        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[TouchInfoHUD] UI 요소 정보:");
            Debug.Log($"  - titleText: {titleTextName} -> {(titleText != null ? "찾음" : "없음")}");
            Debug.Log($"  - descriptionText: {descriptionTextName} -> {(descriptionText != null ? "찾음" : "없음")}");
            Debug.Log($"  - iconImage: {iconImageName} -> {(iconImage != null ? "찾음" : "없음")}");
            Debug.Log($"  - closeButton: {closeButtonName} -> {(closeButton != null ? "찾음" : "없음")}");
            Debug.Log($"  - actionButton: {actionButtonName} -> {(actionButton != null ? "찾음" : "없음")}");
            Debug.Log($"  - closeButtonText: {closeButtonTextName} -> {(closeButtonText != null ? "찾음" : "없음")}");
            Debug.Log($"  - actionButtonText: {actionButtonTextName} -> {(actionButtonText != null ? "찾음" : "없음")}");
        }
    }
}

