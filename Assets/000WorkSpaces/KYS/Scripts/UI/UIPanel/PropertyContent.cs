using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class PropertyContent : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string titleTextName = "TitleText";
        [SerializeField] private string descriptionTextName = "DescriptionText";
        [SerializeField] private string buildButtonName = "BuildButton";
        [SerializeField] private string cancelButtonName = "CancelButton";
        [SerializeField] private string costTextName = "CostText";
        [SerializeField] private string levelTextName = "LevelText";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI titleText => GetUI<TextMeshProUGUI>(titleTextName);
        private TextMeshProUGUI descriptionText => GetUI<TextMeshProUGUI>(descriptionTextName);
        private Button buildButton => GetUI<Button>(buildButtonName);
        private Button cancelButton => GetUI<Button>(cancelButtonName);
        private TextMeshProUGUI costText => GetUI<TextMeshProUGUI>(costTextName);
        private TextMeshProUGUI levelText => GetUI<TextMeshProUGUI>(levelTextName);

        [Header("Build Settings")]
        [SerializeField] private string buildingName = "";
        [SerializeField] private int buildingCost = 100;
        [SerializeField] private int buildingLevel = 1;

        protected override void Awake()
        {
            base.Awake();
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_build_title",
                "ui_build_description",
                "ui_build_button",
                "ui_cancel_button",
                "ui_cost_label",
                "ui_level_label"
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
            var buildEventHandler = GetEventWithSFX(buildButtonName, "SFX_ButtonClick");
            if (buildEventHandler != null)
            {
                buildEventHandler.Click += (data) => OnBuildButtonClicked();
            }

            var cancelEventHandler = GetEventWithSFX(cancelButtonName, "SFX_ButtonClick");
            if (cancelEventHandler != null)
            {
                cancelEventHandler.Click += (data) => OnCancelButtonClicked();
            }
        }

        private void UpdateUI()
        {
            if (titleText != null)
                titleText.text = $"{GetLocalizedText("ui_build_title")} {buildingName}";

            if (descriptionText != null)
                descriptionText.text = $"{GetLocalizedText("ui_build_description")} {buildingName}";

            if (costText != null)
                costText.text = $"{GetLocalizedText("ui_cost_label")}: {buildingCost}";

            if (levelText != null)
                levelText.text = $"{GetLocalizedText("ui_level_label")}: {buildingLevel}";
        }

        public void SetBuildingData(BuildingData buildingData)
        {
            buildingName = buildingData.Name;
            buildingCost = buildingData.Cost;
            // buildingLevel = level; //업그레이드 데이터를 받아올 때 적용
            UpdateUI();
        }


        private void OnBuildButtonClicked()
        {
            // 건물 건설 로직
            Debug.Log($"[BuildContent] 건물 건설 시도: {buildingName}");
            Hide();
        }

        private void OnCancelButtonClicked()
        {
            Debug.Log("[BuildContent] 건설 취소");
            Hide();
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[BuildContent] TitleText: {titleText != null}");
            Debug.Log($"[BuildContent] DescriptionText: {descriptionText != null}");
            Debug.Log($"[BuildContent] BuildButton: {buildButton != null}");
            Debug.Log($"[BuildContent] CancelButton: {cancelButton != null}");
            Debug.Log($"[BuildContent] CostText: {costText != null}");
            Debug.Log($"[BuildContent] LevelText: {levelText != null}");
        }
    }
}
