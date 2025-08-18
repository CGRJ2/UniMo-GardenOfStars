using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KYS.UI;
using KYS.UI.MVP;

namespace KYS.UI.Examples
{
    /// <summary>
    /// 상호작용 팝업 예시 (팝업 UI 그룹)
    /// </summary>
    public class InteractionPopup : BaseUI
    {
        [Header("Interaction Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button interactButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Image itemIcon;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Popup;
            canCloseWithESC = true;
            canCloseWithBackdrop = true;
            hidePreviousUI = false; // 이전 UI 숨기지 않음
            disablePreviousUI = false; // 이전 UI 비활성화하지 않음
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            if (interactButton != null)
            {
                GetEventWithSFX("InteractButton").Click += (data) => OnInteractClicked();
            }
            
            if (closeButton != null)
            {
                GetBackEvent("CloseButton").Click += (data) => OnCloseClicked();
            }
            
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }
        
        public void SetInteractionData(string title, string description, Sprite icon = null)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = description;
            }
            
            if (itemIcon != null && icon != null)
            {
                itemIcon.sprite = icon;
                itemIcon.gameObject.SetActive(true);
            }
            else if (itemIcon != null)
            {
                itemIcon.gameObject.SetActive(false);
            }
        }
        
        #region Event Handlers
        
        private void OnInteractClicked()
        {
            Debug.Log("[InteractionPopup] 상호작용 실행");
            // 실제 상호작용 로직 실행
            Hide();
        }
        
        private void OnCloseClicked()
        {
            Debug.Log("[InteractionPopup] 팝업 닫기");
            Hide();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 정보 표시 팝업 예시 (팝업 UI 그룹)
    /// </summary>
    public class InfoPopup : BaseUI
    {
        [Header("Info Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button okButton;
        [SerializeField] private Button closeButton;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Popup;
            canCloseWithESC = true;
            canCloseWithBackdrop = true;
            hidePreviousUI = false; // 이전 UI 숨기지 않음
            disablePreviousUI = false; // 이전 UI 비활성화하지 않음
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            if (okButton != null)
            {
                GetEventWithSFX("OkButton").Click += (data) => OnOkClicked();
            }
            
            if (closeButton != null)
            {
                GetBackEvent("CloseButton").Click += (data) => OnCloseClicked();
            }
            
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }
        
        public void SetInfo(string title, string content)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }
            
            if (contentText != null)
            {
                contentText.text = content;
            }
        }
        
        #region Event Handlers
        
        private void OnOkClicked()
        {
            Debug.Log("[InfoPopup] 확인 버튼 클릭");
            Hide();
        }
        
        private void OnCloseClicked()
        {
            Debug.Log("[InfoPopup] 팝업 닫기");
            Hide();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 로딩 팝업 예시 (로딩 UI 그룹)
    /// </summary>
    public class LoadingPopup : BaseUI
    {
        [Header("Loading Elements")]
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image loadingIcon;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Loading;
            canCloseWithESC = false;
            canCloseWithBackdrop = false;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            // 로딩 아이콘 회전 애니메이션 (DOTween이 없는 경우 코루틴 사용)
            if (loadingIcon != null)
            {
                StartCoroutine(RotateLoadingIcon());
            }
            
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            
            // 애니메이션 정리
            if (loadingIcon != null)
            {
                StopAllCoroutines();
            }
            
            UIManager.Instance.UnregisterUI(this);
        }
        
        public void SetLoadingText(string text)
        {
            if (loadingText != null)
            {
                loadingText.text = text;
            }
        }
        
        public void SetProgress(float progress)
        {
            if (progressSlider != null)
            {
                progressSlider.value = Mathf.Clamp01(progress);
            }
        }
        
        public void SetProgressText(string text)
        {
            if (loadingText != null)
            {
                loadingText.text = text;
            }
        }
        
        private System.Collections.IEnumerator RotateLoadingIcon()
        {
            while (true)
            {
                if (loadingIcon != null)
                {
                    loadingIcon.transform.Rotate(0, 0, -360f * Time.deltaTime);
                }
                yield return null;
            }
        }
    }
    
    /// <summary>
    /// 툴팁 팝업 예시 (팝업 UI 그룹)
    /// </summary>
    public class TooltipPopup : BaseUI
    {
        [Header("Tooltip Elements")]
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private RectTransform tooltipRect;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Popup;
            canCloseWithESC = false;
            canCloseWithBackdrop = false;
            useAnimation = false; // 툴팁은 애니메이션 없이 즉시 표시
            hidePreviousUI = false; // 이전 UI 숨기지 않음
            disablePreviousUI = false; // 이전 UI 비활성화하지 않음
        }
        
        public override void Initialize()
        {
            base.Initialize();
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }
        
        public void SetTooltip(string text, Vector3 worldPosition)
        {
            if (tooltipText != null)
            {
                tooltipText.text = text;
            }
            
            // 월드 좌표를 스크린 좌표로 변환
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            
            // UI 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipRect.parent as RectTransform,
                screenPosition,
                null,
                out Vector2 localPoint
            );
            
            tooltipRect.anchoredPosition = localPoint;
        }
        
        public void SetTooltip(string text, Vector2 screenPosition)
        {
            if (tooltipText != null)
            {
                tooltipText.text = text;
            }
            
            // 스크린 좌표를 UI 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipRect.parent as RectTransform,
                screenPosition,
                null,
                out Vector2 localPoint
            );
            
            tooltipRect.anchoredPosition = localPoint;
        }
    }
}
