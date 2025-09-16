using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class HRRoomPanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string hrRoomTextName = "HRRoomText";
        [SerializeField] private string runGemButtonTextName = "RunGemButtonText";
        [SerializeField] private string runMoneyButtonTextName = "RunMoneyButtonText";
        [SerializeField] private string closeButtonName = "CloseButton";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI hrRoomText => GetUI<TextMeshProUGUI>(hrRoomTextName);
        private TextMeshProUGUI runGemButtonText => GetUI<TextMeshProUGUI>(runGemButtonTextName);
        private TextMeshProUGUI runMoneyButtonText => GetUI<TextMeshProUGUI>(runMoneyButtonTextName);
        private Button closeButton => GetUI<Button>(closeButtonName);

        // 이벤트 핸들러 저장용
        private System.Action<UnityEngine.EventSystems.PointerEventData> closeButtonHandler;

        protected override void Awake()
        {
            base.Awake();
            Manager.player.Data.Money.Subscribe(OnMoneyChanged);
            Initialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Manager.player?.Data?.Money.Unsubscribe(OnMoneyChanged);
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_hr_room_title",
                "ui_hire_button",
                "ui_fire_button",
                "ui_close_button",
                "ui_worker_count_label",
                "ui_salary_label"
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
            
            // UI 업데이트
            UpdateUI();

            // 초기 값 설정
            UpdateMoney(Manager.player.Data.Money.Value);
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        private void SetupButtons()
        {
            // CloseButton 이벤트 설정 (중복 등록 방지)
            var closeEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClickBack");
            if (closeEventHandler != null)
            {
                // 기존 이벤트 해제
                if (closeButtonHandler != null)
                {
                    closeEventHandler.Click -= closeButtonHandler;
                }
                
                // 새 이벤트 핸들러 생성 및 등록
                closeButtonHandler = (data) => OnCloseButtonClicked();
                closeEventHandler.Click += closeButtonHandler;
            }
        }

        private void UpdateUI()
        {
            // HRRoomText 업데이트
            if (hrRoomText != null)
                hrRoomText.text = GetLocalizedText("ui_hr_room_title");
        }

        public void UpdateMoney(int amount)
        {
            //currentMoney = amount;
            if (runMoneyButtonText != null)
            {
                runMoneyButtonText.text = $"{amount:N0}";
            }
        }

        /// <summary>
        /// ObservableProperty Money 값 변경 시 호출되는 콜백
        /// </summary>
        private void OnMoneyChanged(int newMoneyValue)
        {
            UpdateMoney(newMoneyValue);
        }

        private void OnCloseButtonClicked()
        {
            Debug.LogWarning("[HRRoomPanel] 패널 닫기");
            Manager.ui.ClosePanel();
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[HRRoomPanel] HRRoomText: {hrRoomText != null}");
            Debug.Log($"[HRRoomPanel] RunGemButtonText: {runGemButtonText != null}");
            Debug.Log($"[HRRoomPanel] RunMoneyButtonText: {runMoneyButtonText != null}");
            Debug.Log($"[HRRoomPanel] CloseButton: {closeButton != null}");
        }
    }
}
