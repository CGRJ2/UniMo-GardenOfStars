using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
            
            // 게스트 계정 확인 및 UI 업데이트
            CheckGuestAccountAndUpdateUI();
        }

        public override void Cleanup()
        {
            OnLogoutConfirmed = null;
            OnLogoutCancelled = null;
            base.Cleanup();
        }

        private void SetupButtons()
        {
            Debug.Log($"[LogoutPopup] SetupButtons() 시작 - Time: {Time.time}, isButtonsSetup: {isButtonsSetup}");

            // 이미 설정되었으면 중복 호출 방지
            if (isButtonsSetup)
            {
                Debug.Log($"[LogoutPopup] SetupButtons 이미 완료됨 - 중복 호출 방지");
                return;
            }

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

            isButtonsSetup = true; // 설정 완료 플래그
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
            Debug.Log("[LogoutPopup] 로그아웃 확인");

            // 게스트 계정인지 확인
            if (!IsGuestAccount())
            {
                Debug.LogWarning("[LogoutPopup] 게스트 계정이 아닙니다. 로그아웃을 취소합니다.");
                ShowMessagePopUpWithKey("logout_guest_only");
                return;
            }

            // 로그아웃 처리
            ExecuteLogout();
        }

        /// <summary>
        /// 게스트 계정인지 확인
        /// </summary>
        private bool IsGuestAccount()
        {
            try
            {
                if (Manager.firebase?.Auth?.CurrentUser == null)
                {
                    Debug.Log("[LogoutPopup] 현재 로그인된 사용자가 없습니다.");
                    return false;
                }

                // Firebase Auth에서 익명 사용자인지 확인
                bool isAnonymous = Manager.firebase.Auth.CurrentUser.IsAnonymous;
                Debug.Log($"[LogoutPopup] 현재 사용자: {Manager.firebase.Auth.CurrentUser.UserId}, 익명: {isAnonymous}");
                
                return isAnonymous;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LogoutPopup] 게스트 계정 확인 중 오류: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 로그아웃 실행
        /// </summary>
        private void ExecuteLogout()
        {
            try
            {
                Debug.Log("[LogoutPopup] 게스트 계정 로그아웃을 시작합니다.");

                // Firebase 로그아웃
                Manager.firebase.Auth.SignOut();

                // 이벤트 호출
                OnLogoutConfirmed?.Invoke();

                // UI 정리
                Manager.ui.HideAllHUDElements();
                
                // 로그인 씬으로 이동
                SceneManager.LoadScene("JTW_LoginScene");
                
                Debug.Log("[LogoutPopup] 게스트 계정 로그아웃이 완료되었습니다.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LogoutPopup] 로그아웃 실행 중 오류: {e.Message}");
                ShowMessagePopUpWithKey("logout_error");
            }
        }

        /// <summary>
        /// 메시지 팝업 표시
        /// </summary>
        private void ShowMessagePopUp(string message)
        {
            Manager.ui.ShowMessagePopUpAsync(message);
        }

        /// <summary>
        /// 번역 키를 사용한 메시지 팝업 표시
        /// </summary>
        private void ShowMessagePopUpWithKey(string localizationKey)
        {
            Manager.ui.ShowMessagePopUpWithKeyAsync(localizationKey);
        }

        /// <summary>
        /// 게스트 계정 확인 및 UI 업데이트
        /// </summary>
        private void CheckGuestAccountAndUpdateUI()
        {
            bool isGuest = IsGuestAccount();
            
            if (!isGuest)
            {
                // 게스트 계정이 아닌 경우 확인 버튼 비활성화
                if (confirmButton != null)
                {
                    confirmButton.interactable = false;
                    var confirmTextComponent = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (confirmTextComponent != null)
                    {
                        confirmTextComponent.text = GetLocalizedText("logout_guest_only", "게스트 계정만 로그아웃할 수 있습니다.");
                    }
                }
                
                // 메시지 업데이트
                if (logoutText1 != null)
                {
                    logoutText1.text = GetLocalizedText("logout_guest_only", "게스트 계정만 로그아웃할 수 있습니다.");
                }
                
                Debug.Log("[LogoutPopup] 게스트 계정이 아니므로 로그아웃 버튼을 비활성화합니다.");
            }
            else
            {
                // 게스트 계정인 경우 정상적으로 활성화
                if (confirmButton != null)
                {
                    confirmButton.interactable = true;
                }
                
                Debug.Log("[LogoutPopup] 게스트 계정이므로 로그아웃이 가능합니다.");
            }
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