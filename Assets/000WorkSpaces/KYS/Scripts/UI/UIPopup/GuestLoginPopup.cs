using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class GuestLoginPopup : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string guestLoginText1Name = "GuestLoginText1";
        [SerializeField] private string guestLoginText2Name = "GuestLoginText2";
        [SerializeField] private string confirmButtonName = "ConfirmButton";
        [SerializeField] private string cancelButtonName = "CancelButton";
        [SerializeField] private string guestLoginTextAreaName = "GuestLoginTextArea";
        [SerializeField] private string guestLoginTextName = "GuestLoginText";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI guestLoginText1 => GetUI<TextMeshProUGUI>(guestLoginText1Name);
        private TextMeshProUGUI guestLoginText2 => GetUI<TextMeshProUGUI>(guestLoginText2Name);
        private Button confirmButton => GetUI<Button>(confirmButtonName);
        private Button cancelButton => GetUI<Button>(cancelButtonName);
        private GameObject guestLoginTextArea => GetUI(guestLoginTextAreaName);
        private TextMeshProUGUI guestLoginText => GetUI<TextMeshProUGUI>(guestLoginTextName);

        [Header("Guest Login Settings")]
        [SerializeField] private string confirmText = "게스트로 시작";
        [SerializeField] private string cancelText = "취소";
        [SerializeField] private string guestMessage1 = "게스트로 로그인하시겠습니까?";
        [SerializeField] private string guestMessage2 = "게스트 계정은 임시 계정으로, 데이터가 손실될 수 있습니다.";
       

        // 이벤트
        public System.Action OnGuestLoginConfirmed;
        public System.Action OnGuestLoginCancelled;
        public System.Action OnCreateAccountRequested; // 계정 생성 요청 이벤트

        protected override void Awake()
        {
            base.Awake();
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_guest_login_title",
                "ui_guest_login_message1",
                "ui_guest_login_message2",
                "ui_guest_login_confirm",
                "ui_guest_login_cancel",
                "ui_guest_login_description"
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
            OnGuestLoginConfirmed = null;
            OnGuestLoginCancelled = null;
            OnCreateAccountRequested = null;
            base.Cleanup();
        }

        private void SetupButtons()
        {
            // 확인 버튼 (게스트로 시작)
            if (confirmButton != null)
            {
                var confirmHandler = GetEventWithSFX("ConfirmButton", "SFX_ButtonClick");
                if (confirmHandler != null)
                {
                    confirmHandler.Click += (data) => OnConfirmClicked();
                }
            }

            // 취소 버튼
            if (cancelButton != null)
            {
                var cancelHandler = GetEventWithSFX("CancelButton", "SFX_ButtonClickBack");
                if (cancelHandler != null)
                {
                    cancelHandler.Click += (data) => OnCancelClicked();
                }
            }
        }

        private void UpdateUI()
        {
            // 게스트 로그인 메시지 설정
            if (guestLoginText1 != null)
            {
                guestLoginText1.text = GetLocalizedText("ui_guest_login_message1", guestMessage1);
            }

            if (guestLoginText2 != null)
            {
                guestLoginText2.text = GetLocalizedText("ui_guest_login_message2", guestMessage2);
            }

            // 버튼 텍스트 설정
            UpdateButtonTexts();

            // 게스트 로그인 설명 텍스트 설정
            if (guestLoginText != null)
            {
                guestLoginText.text = GetLocalizedText("ui_guest_login_title");
            }
        }

        private void UpdateButtonTexts()
        {
            // 확인 버튼 텍스트 (게스트로 시작)
            var confirmTextComponent = confirmButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (confirmTextComponent != null)
            {
                confirmTextComponent.text = GetLocalizedText("ui_guest_login_confirm", confirmText);
            }

            // 취소 버튼 텍스트
            var cancelTextComponent = cancelButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (cancelTextComponent != null)
            {
                cancelTextComponent.text = GetLocalizedText("ui_guest_login_cancel", cancelText);
            }
        }

        private void OnConfirmClicked()
        {
            Debug.Log("[GuestLoginPopup] 게스트 로그인 확인");
            
            //// 게스트 로그인 처리
            //ProcessGuestLogin();
            
            //// 이벤트 호출
            OnGuestLoginConfirmed?.Invoke();
            
            //// 팝업 닫기
            Manager.ui.ClosePopup();
        }

        private void OnCancelClicked()
        {
            Debug.Log("[GuestLoginPopup] 게스트 로그인 취소");
            
            // 이벤트 호출
            OnGuestLoginCancelled?.Invoke();
            
            // 팝업 닫기
            Manager.ui.ClosePopup();
        }

        private void ProcessGuestLogin()
        {
            // 실제 게스트 로그인 로직 구현
            Debug.Log("[GuestLoginPopup] 게스트 로그인 처리 중...");
            
            // TODO: 실제 게스트 로그인 로직 추가
            // Manager.auth.GuestLogin();
            // Manager.data.InitializeGuestData();
        }

        /// <summary>
        /// 게스트 로그인 팝업 표시
        /// </summary>
        public static void ShowGuestLoginPopup(System.Action onConfirmed = null, System.Action onCancelled = null)
        {
            if (Manager.ui != null)
            {
                Manager.ui.ShowPopUpAsync<GuestLoginPopup>((popup) =>
                {
                    if (popup != null)
                    {
                        popup.OnGuestLoginConfirmed = onConfirmed;
                        popup.OnGuestLoginCancelled = onCancelled;
                    }
                });
            }
        }

        /// <summary>
        /// 커스텀 메시지로 게스트 로그인 팝업 표시
        /// </summary>
        public void SetCustomMessages(string message1, string message2, string description = null)
        {
            if (guestLoginText1 != null)
            {
                guestLoginText1.text = message1;
            }

            if (guestLoginText2 != null)
            {
                guestLoginText2.text = message2;
            }

            if (guestLoginText != null && !string.IsNullOrEmpty(description))
            {
                guestLoginText.text = description;
            }
        }

        /// <summary>
        /// 게스트 로그인 텍스트 영역 표시/숨김
        /// </summary>
        public void SetGuestLoginTextAreaVisible(bool visible)
        {
            if (guestLoginTextArea != null)
            {
                guestLoginTextArea.SetActive(visible);
            }
        }

        /// <summary>
        /// 버튼 텍스트 커스터마이징
        /// </summary>
        public void SetButtonTexts(string confirmText, string cancelText)
        {
            this.confirmText = confirmText;
            this.cancelText = cancelText;
            UpdateButtonTexts();
        }

        /// <summary>
        /// 계정 생성 요청 처리 (텍스트 클릭 시 호출 가능)
        /// </summary>
        public void RequestCreateAccount()
        {
            Debug.Log("[GuestLoginPopup] 계정 생성 요청");
            OnCreateAccountRequested?.Invoke();
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[GuestLoginPopup] UI 요소 정보:");
            Debug.Log($"- GuestLoginText1: {(guestLoginText1 != null ? "찾음" : "없음")}");
            Debug.Log($"- GuestLoginText2: {(guestLoginText2 != null ? "찾음" : "없음")}");
            Debug.Log($"- ConfirmButton: {(confirmButton != null ? "찾음" : "없음")}");
            Debug.Log($"- CancelButton: {(cancelButton != null ? "찾음" : "없음")}");
            Debug.Log($"- GuestLoginTextArea: {(guestLoginTextArea != null ? "찾음" : "없음")}");
            Debug.Log($"- GuestLoginText: {(guestLoginText != null ? "찾음" : "없음")}");
        }
    }
}