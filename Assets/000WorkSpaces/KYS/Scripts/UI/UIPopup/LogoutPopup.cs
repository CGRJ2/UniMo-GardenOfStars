using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
namespace KYS
{
    public class LogoutPopup : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string logoutText1Name = "LogoutText1";
        [SerializeField] private string logoutText2Name = "LogoutText2";
        [SerializeField] private string confirmButtonName = "ConfirmButton";
        [SerializeField] private string cancelButtonName = "CancelButton";
        [SerializeField] private string logoutTextAreaName = "LogoutTextArea";
        [SerializeField] private string logoutTextName = "LogoutText";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI logoutText1 => GetUI<TextMeshProUGUI>(logoutText1Name);
        private TextMeshProUGUI logoutText2 => GetUI<TextMeshProUGUI>(logoutText2Name);
        private Button confirmButton => GetUI<Button>(confirmButtonName);
        private Button cancelButton => GetUI<Button>(cancelButtonName);
        private GameObject logoutTextArea => GetUI(logoutTextAreaName);
        private TextMeshProUGUI logoutText => GetUI<TextMeshProUGUI>(logoutTextName);

        [Header("Logout Settings")]
        [SerializeField] private string confirmText = "확인";
        [SerializeField] private string cancelText = "취소";
        [SerializeField] private string logoutMessage1 = "정말로 로그아웃 하시겠습니까?";
        [SerializeField] private string logoutMessage2 = "진행 중인 작업이 저장되지 않을 수 있습니다.";

        // 이벤트
        public System.Action OnLogoutConfirmed;
        public System.Action OnLogoutCancelled;

        protected override void Awake()
        {
            base.Awake();
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_logout_title",
                "ui_logout_message1",
                "ui_logout_message2",
                "ui_logout_confirm",
                "ui_logout_cancel"
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
            OnLogoutConfirmed = null;
            OnLogoutCancelled = null;
            base.Cleanup();
        }

        private void SetupButtons()
        {
            // 확인 버튼
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
            // 로그아웃 메시지 설정
            if (logoutText1 != null)
            {
                logoutText1.text = GetLocalizedText("ui_logout_message1", logoutMessage1);
            }

            if (logoutText2 != null)
            {
                logoutText2.text = GetLocalizedText("ui_logout_message2", logoutMessage2);
            }

            // 버튼 텍스트 설정
            UpdateButtonTexts();

            // 로그아웃 텍스트 영역 설정 (선택사항)
            if (logoutText != null)
            {
                logoutText.text = GetLocalizedText("ui_logout_title", "로그아웃");
            }
        }

        private void UpdateButtonTexts()
        {
            // 확인 버튼 텍스트
            var confirmTextComponent = confirmButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (confirmTextComponent != null)
            {
                confirmTextComponent.text = GetLocalizedText("ui_logout_confirm", confirmText);
            }

            // 취소 버튼 텍스트
            var cancelTextComponent = cancelButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (cancelTextComponent != null)
            {
                cancelTextComponent.text = GetLocalizedText("ui_logout_cancel", cancelText);
            }
        }

        private void OnConfirmClicked()
        {
            //TODO: 로그아웃 기능 구현 필요
            //Debug.Log("[LogoutPopup] 로그아웃 확인");

            //// 로그아웃 처리
            //ProcessLogout();
            //Manager.firebase.Auth.SignOut();
            //StartCoroutine(WaitLogin());



            //// 이벤트 호출
            //OnLogoutConfirmed?.Invoke();

            //// 팝업 닫기
            //Manager.ui.ClosePopup();
        }


        private IEnumerator WaitLogin()
        {
            Manager.firebase.InitUserData();   
            yield return new WaitForSeconds(1f);

        }

        private void OnCancelClicked()
        {
            Debug.Log("[LogoutPopup] 로그아웃 취소");
            
            // 이벤트 호출
            OnLogoutCancelled?.Invoke();

            // 팝업 닫기
            Manager.ui.ClosePopup();
        }

        private void ProcessLogout()
        {
            // 실제 로그아웃 로직 구현
            // 예: Firebase 로그아웃, 세션 정리 등
            Debug.Log("[LogoutPopup] 로그아웃 처리 중...");
            
            // TODO: 실제 로그아웃 로직 추가
            // Manager.auth.Logout();
        }

        /// <summary>
        /// 로그아웃 팝업 표시
        /// </summary>
        public static void ShowLogoutPopup(System.Action onConfirmed = null, System.Action onCancelled = null)
        {
            if (Manager.ui != null)
            {
                Manager.ui.ShowPopUpAsync<LogoutPopup>((popup) =>
                {
                    if (popup != null)
                    {
                        popup.OnLogoutConfirmed = onConfirmed;
                        popup.OnLogoutCancelled = onCancelled;
                    }
                });
            }
        }

        /// <summary>
        /// 커스텀 메시지로 로그아웃 팝업 표시
        /// </summary>
        public void SetCustomMessages(string message1, string message2)
        {
            if (logoutText1 != null)
            {
                logoutText1.text = message1;
            }

            if (logoutText2 != null)
            {
                logoutText2.text = message2;
            }
        }

        /// <summary>
        /// 로그아웃 텍스트 영역 표시/숨김
        /// </summary>
        public void SetLogoutTextAreaVisible(bool visible)
        {
            if (logoutTextArea != null)
            {
                logoutTextArea.SetActive(visible);
            }
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[LogoutPopup] UI 요소 정보:");
            Debug.Log($"- LogoutText1: {(logoutText1 != null ? "찾음" : "없음")}");
            Debug.Log($"- LogoutText2: {(logoutText2 != null ? "찾음" : "없음")}");
            Debug.Log($"- ConfirmButton: {(confirmButton != null ? "찾음" : "없음")}");
            Debug.Log($"- CancelButton: {(cancelButton != null ? "찾음" : "없음")}");
            Debug.Log($"- LogoutTextArea: {(logoutTextArea != null ? "찾음" : "없음")}");
            Debug.Log($"- LogoutText: {(logoutText != null ? "찾음" : "없음")}");
        }
    }
}