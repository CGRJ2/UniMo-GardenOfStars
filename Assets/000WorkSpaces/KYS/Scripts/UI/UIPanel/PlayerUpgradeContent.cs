using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class PlayerUpgradeContent : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string titleTextName = "TitleText";
        [SerializeField] private string descriptionTextName = "DescriptionText";
        [SerializeField] private string upgradeButtonName = "UpgradeButton";
        [SerializeField] private string costTextName = "CostText";
        [SerializeField] private string levelTextName = "LevelText";
        [SerializeField] private string effectTextName = "EffectText";
        [SerializeField] private string iconImageName = "IconImage";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI titleText => GetUI<TextMeshProUGUI>(titleTextName);
        private TextMeshProUGUI descriptionText => GetUI<TextMeshProUGUI>(descriptionTextName);
        private Button upgradeButton => GetUI<Button>(upgradeButtonName);
        private TextMeshProUGUI costText => GetUI<TextMeshProUGUI>(costTextName);
        private TextMeshProUGUI levelText => GetUI<TextMeshProUGUI>(levelTextName);
        private TextMeshProUGUI effectText => GetUI<TextMeshProUGUI>(effectTextName);
        private Image iconImage => GetUI<Image>(iconImageName);

        [Header("Upgrade Settings")]
        [SerializeField] private string upgradeName = "";
        [SerializeField] private int currentLevel = 0;
        [SerializeField] private int upgradeCost = 100;
        [SerializeField] private float effectValue = 1f;
        [SerializeField] private Sprite upgradeIcon;

        protected override void Awake()
        {
            base.Awake();
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_upgrade_title",
                "ui_upgrade_description",
                "ui_upgrade_button",
                "ui_cost_label",
                "ui_level_label",
                "ui_effect_label"
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
            var upgradeEventHandler = GetEventWithSFX(upgradeButtonName, "SFX_ButtonClick");
            if (upgradeEventHandler != null)
            {
                upgradeEventHandler.Click += (data) => OnUpgradeButtonClicked();
            }
        }

        private void UpdateUI()
        {
            if (titleText != null)
                titleText.text = $"{GetLocalizedText("ui_upgrade_title")} {upgradeName}";

            if (descriptionText != null)
                descriptionText.text = $"{GetLocalizedText("ui_upgrade_description")} {upgradeName}";

            if (costText != null)
                costText.text = $"{GetLocalizedText("ui_cost_label")}: {upgradeCost}";

            if (levelText != null)
                levelText.text = $"{GetLocalizedText("ui_level_label")}: {currentLevel}";

            if (effectText != null)
                effectText.text = $"{GetLocalizedText("ui_effect_label")}: {effectValue:F1}";

            if (iconImage != null && upgradeIcon != null)
                iconImage.sprite = upgradeIcon;
        }

        public void SetUpgradeData(string name, int level, int cost, float effect, Sprite icon = null)
        {
            upgradeName = name;
            currentLevel = level;
            upgradeCost = cost;
            effectValue = effect;
            upgradeIcon = icon;
            UpdateUI();
        }

        private void OnUpgradeButtonClicked()
        {
            // 업그레이드 로직
            currentLevel++;
            upgradeCost = Mathf.RoundToInt(upgradeCost * 1.5f);
            effectValue += 0.5f;
            UpdateUI();
            Debug.Log($"[PlayerUpgradeContent] 업그레이드 완료: {upgradeName} (레벨 {currentLevel})");
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[PlayerUpgradeContent] TitleText: {titleText != null}");
            Debug.Log($"[PlayerUpgradeContent] DescriptionText: {descriptionText != null}");
            Debug.Log($"[PlayerUpgradeContent] UpgradeButton: {upgradeButton != null}");
            Debug.Log($"[PlayerUpgradeContent] CostText: {costText != null}");
            Debug.Log($"[PlayerUpgradeContent] LevelText: {levelText != null}");
            Debug.Log($"[PlayerUpgradeContent] EffectText: {effectText != null}");
            Debug.Log($"[PlayerUpgradeContent] IconImage: {iconImage != null}");
        }
    }
}
