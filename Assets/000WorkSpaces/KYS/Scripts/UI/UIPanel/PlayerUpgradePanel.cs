using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class PlayerUpgradePanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string titleTextName = "TitleText";
        [SerializeField] private string closeButtonName = "CloseButton";
        [SerializeField] private string upgradeListName = "UpgradeList";
        [SerializeField] private string playerLevelTextName = "PlayerLevelText";
        [SerializeField] private string playerExpTextName = "PlayerExpText";
        [SerializeField] private string playerMoneyTextName = "PlayerMoneyText";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI titleText => GetUI<TextMeshProUGUI>(titleTextName);
        private Button closeButton => GetUI<Button>(closeButtonName);
        private Transform upgradeList => GetUI<Transform>(upgradeListName);
        private TextMeshProUGUI playerLevelText => GetUI<TextMeshProUGUI>(playerLevelTextName);
        private TextMeshProUGUI playerExpText => GetUI<TextMeshProUGUI>(playerExpTextName);
        private TextMeshProUGUI playerMoneyText => GetUI<TextMeshProUGUI>(playerMoneyTextName);

        [Header("Player Info")]
        [SerializeField] private int playerLevel = 1;
        [SerializeField] private int playerExp = 0;
        [SerializeField] private int playerMoney = 1000;

        protected override void Awake()
        {
            base.Awake();
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

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
            UpdateUI();
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        private void SetupButtons()
        {
            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var closeEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClick");
            if (closeEventHandler != null)
            {
                closeEventHandler.Click += (data) => OnCloseButtonClicked();
            }
        }

        private void UpdateUI()
        {
            if (titleText != null)
                titleText.text = GetLocalizedText("ui_player_upgrade_title");

            if (playerLevelText != null)
                playerLevelText.text = $"{GetLocalizedText("ui_player_level_label")}: {playerLevel}";

            if (playerExpText != null)
                playerExpText.text = $"{GetLocalizedText("ui_player_exp_label")}: {playerExp}";

            if (playerMoneyText != null)
                playerMoneyText.text = $"{GetLocalizedText("ui_player_money_label")}: {playerMoney}";
        }

        public void SetPlayerData(int level, int exp, int money)
        {
            playerLevel = level;
            playerExp = exp;
            playerMoney = money;
            UpdateUI();
        }

        public void UpdatePlayerMoney(int newMoney)
        {
            playerMoney = newMoney;
            if (playerMoneyText != null)
                playerMoneyText.text = $"{GetLocalizedText("ui_player_money_label")}: {playerMoney}";
        }

        private void OnCloseButtonClicked()
        {
            Debug.Log("[PlayerUpgradePanel] 패널 닫기");
            Hide();
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[PlayerUpgradePanel] TitleText: {titleText != null}");
            Debug.Log($"[PlayerUpgradePanel] CloseButton: {closeButton != null}");
            Debug.Log($"[PlayerUpgradePanel] UpgradeList: {upgradeList != null}");
            Debug.Log($"[PlayerUpgradePanel] PlayerLevelText: {playerLevelText != null}");
            Debug.Log($"[PlayerUpgradePanel] PlayerExpText: {playerExpText != null}");
            Debug.Log($"[PlayerUpgradePanel] PlayerMoneyText: {playerMoneyText != null}");
        }
    }
}
