using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using UnityEngine.TextCore.Text;

namespace KYS
{
    public class PropertyContent : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string runBuildingNameText = "RunBuildingNameText";
        [SerializeField] private string buyButtonName = "BuyButton";
        [SerializeField] private string costTextName = "MoenyButtonText";

        [SerializeField] Button buyButton;

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI titleText => GetUI<TextMeshProUGUI>(runBuildingNameText);
        //private Button buyButton => GetUI<Button>(buyButtonName);
        private TextMeshProUGUI costText => GetUI<TextMeshProUGUI>(costTextName);

        private string buildingID;

        [Header("Build Settings")]
        [SerializeField] private string buildingName = "";
        [SerializeField] private int buildingCost = 100;
        [SerializeField] private int buildingLevel = 1;

        protected override void Awake()
        {
            base.Awake();
            Debug.LogWarning("초기화 실행");

            buyButton.onClick.AddListener(() =>
            {
                Debug.LogWarning("구매 버튼 클릭");
                Manager.buildings.buildingSeller.SpawnBuildingItem(buildingID);
                Manager.ui.ClosePanel();
            });
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
            var buildEventHandler = GetEventWithSFX(buyButtonName, "SFX_ButtonClick");
            if (buildEventHandler != null)
            {
                buildEventHandler.Click += (data) => OnBuildButtonClicked();
            }

        }

        private void UpdateUI()
        {
            if (titleText != null)
                titleText.text = $"{GetLocalizedText("ui_build_title")} {buildingName}";

            if (costText != null)
                costText.text = $"{GetLocalizedText("ui_cost_label")}: {buildingCost}";

        }

        public void SetBuildingData(BuildingData buildingData, UpgradeData upgradeData = null)
        {
            buildingID = buildingData.ID;
            buildingName = buildingData.Name;
            buildingCost = buildingData.Cost;

            if (buildingData is HarvestBD harvestBD)
            {
                // 재료(생산품) 이름, 스프라이트
                /*Addressables.LoadAssetAsync<IngrediantData>(harvestBD.ProductID).Completed += prodData =>
                {
                    string prodNameKey = $"RunIngrediantName{prodData.Result.Name}";

                    tmp_ProdName.text = Manager.localization.GetText(prodNameKey);
                    image_Prod.sprite = prodData.Result.Sprite;
                };*/


                //업그레이드 데이터를 받아올 때 적용
                if (upgradeData != null)
                {
                    //buildingLevel = level; 
                }
            }
            else if (buildingData is ManufactureBD)
            {
                // 재료(생산품) 이름, 스프라이트

                // 재료(투입용) 이름, 스프라이트


                //업그레이드 데이터를 받아올 때 적용
                if (upgradeData != null)
                {
                    //buildingLevel = level; 
                }
            }
            
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
            Debug.Log($"[BuildContent] BuildButton: {buyButton != null}");
            Debug.Log($"[BuildContent] CostText: {costText != null}");
        }
    }
}
