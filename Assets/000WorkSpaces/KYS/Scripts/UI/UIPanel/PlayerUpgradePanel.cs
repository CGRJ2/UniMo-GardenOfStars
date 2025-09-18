using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class PlayerUpgradePanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string closeButtonName = "CloseButton";
        [SerializeField] private string moneyTextName = "RunMoneyBottonText";
        [SerializeField] private string titleTextName = "PlayerUpgradeTitleText";

        private Button closeButton => GetUI<Button>(closeButtonName);
        private TextMeshProUGUI moneyText => GetUI<TextMeshProUGUI>(moneyTextName);
        private TextMeshProUGUI titleText => GetUI<TextMeshProUGUI>(titleTextName);

        protected override void Awake()
        {
            base.Awake();

            SetupButtons();
            UpdateUI();

            // 초기 값 설정
            UpdateMoney(Manager.player.Data.Money.Value);

            // ObservableProperty 구독 - 실시간 돈 업데이트
            Manager.player.Data.Money.Subscribe(OnMoneyChanged);
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_player_upgrade_title",
                "ui_close_button",
                "ui_player_level_label",
                "ui_player_exp_label",
                "ui_player_money_label"
            };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Manager.player?.Data?.Money.Unsubscribe(OnMoneyChanged);
        }

        public override void Initialize()
        {
            base.Initialize();
            
            // 언어 변경 이벤트 구독
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }

        public override void Cleanup()
        {
            // 언어 변경 이벤트 구독 해제
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
            base.Cleanup();
        }

        private void SetupButtons()
        {
            Debug.Log($"[PlayerUpgradePanel] SetupButtons() 시작 - Time: {Time.time}, isButtonsSetup: {isButtonsSetup}");

            // 이미 설정되었으면 중복 호출 방지
            if (isButtonsSetup)
            {
                Debug.Log($"[PlayerUpgradePanel] SetupButtons 이미 완료됨 - 중복 호출 방지");
                return;
            }

            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var closeEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClickBack");
            if (closeEventHandler != null)
            {
                closeEventHandler.Click += (data) => OnCloseButtonClicked();
            }

            isButtonsSetup = true; // 설정 완료 플래그
        }

        private void UpdateUI()
        {
            // 제목 텍스트 업데이트
            if (titleText != null)
            {
                titleText.text = Manager.localization.GetText(titleTextName);
            }
        }

        public void UpdateMoney(int amount)
        {
            if (moneyText != null)
            {
                moneyText.text = $"{amount:N0}";
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
            Debug.Log("[PlayerUpgradePanel] 패널 닫기");
            Hide();
        }

        /// <summary>
        /// 언어 변경 이벤트 핸들러
        /// </summary>
        private void OnLanguageChanged(SystemLanguage newLanguage)
        {
            UpdateUI();
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[PlayerUpgradePanel] CloseButton: {closeButton != null}");
        }
    }
}
