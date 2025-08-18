using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KYS.UI;
using KYS.UI.MVP;

namespace KYS.UI.Examples
{
    /// <summary>
    /// 이전 UI를 숨기지 않는 팝업 예시
    /// </summary>
    public class OverlayPopup : BaseUI
    {
        [Header("Overlay Elements")]
        [SerializeField] private TextMeshProUGUI messageText;
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
        
        public void SetMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
        }
        
        private void OnCloseClicked()
        {
            Debug.Log("[OverlayPopup] 오버레이 팝업 닫기");
            Hide();
        }
    }
    
    /// <summary>
    /// 이전 UI를 비활성화하는 팝업 예시
    /// </summary>
    public class ModalPopup : BaseUI
    {
        [Header("Modal Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Popup;
            canCloseWithESC = true;
            canCloseWithBackdrop = true;
            hidePreviousUI = false; // 이전 UI 숨기지 않음
            disablePreviousUI = true; // 이전 UI 비활성화
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            if (confirmButton != null)
            {
                GetEventWithSFX("ConfirmButton").Click += (data) => OnConfirmClicked();
            }
            
            if (cancelButton != null)
            {
                GetBackEvent("CancelButton").Click += (data) => OnCancelClicked();
            }
            
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }
        
        public void SetModalData(string title, string content)
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
        
        private void OnConfirmClicked()
        {
            Debug.Log("[ModalPopup] 확인 버튼 클릭");
            Hide();
        }
        
        private void OnCancelClicked()
        {
            Debug.Log("[ModalPopup] 취소 버튼 클릭");
            Hide();
        }
    }
    
    /// <summary>
    /// ESC로 닫을 수 없는 중요 패널 예시
    /// </summary>
    public class ImportantPanel : BaseUI
    {
        [Header("Important Elements")]
        [SerializeField] private TextMeshProUGUI warningText;
        [SerializeField] private Button acknowledgeButton;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Panel;
            panelGroup = UIPanelGroup.GameExit;
            canCloseWithESC = false; // ESC로 닫을 수 없음
            canCloseWithBackdrop = false; // 배경 클릭으로 닫을 수 없음
            hidePreviousUI = true;
            disablePreviousUI = false;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            if (acknowledgeButton != null)
            {
                GetEventWithSFX("AcknowledgeButton").Click += (data) => OnAcknowledgeClicked();
            }
            
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }
        
        public void SetWarningMessage(string message)
        {
            if (warningText != null)
            {
                warningText.text = message;
            }
        }
        
        private void OnAcknowledgeClicked()
        {
            Debug.Log("[ImportantPanel] 확인 버튼 클릭");
            Hide();
        }
    }
    
    /// <summary>
    /// 완전히 독립적인 팝업 (이전 UI에 영향 없음)
    /// </summary>
    public class IndependentPopup : BaseUI
    {
        [Header("Independent Elements")]
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private Button okButton;
        
        protected override void Awake()
        {
            base.Awake();
            layerType = UILayerType.Popup;
            canCloseWithESC = true;
            canCloseWithBackdrop = true;
            hidePreviousUI = false; // 이전 UI 숨기지 않음
            disablePreviousUI = false; // 이전 UI 비활성화하지 않음
            destroyOnClose = true; // 닫을 때 파괴
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            if (okButton != null)
            {
                GetEventWithSFX("OkButton").Click += (data) => OnOkClicked();
            }
            
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            UIManager.Instance.UnregisterUI(this);
        }
        
        public void SetInfo(string info)
        {
            if (infoText != null)
            {
                infoText.text = info;
            }
        }
        
        private void OnOkClicked()
        {
            Debug.Log("[IndependentPopup] 확인 버튼 클릭");
            Hide();
        }
    }
}
