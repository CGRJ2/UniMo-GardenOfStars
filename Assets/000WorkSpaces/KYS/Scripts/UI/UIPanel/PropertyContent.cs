using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace KYS
{
    public class PropertyContent : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string buildingTextName = "RunBuildingNameText";
        [SerializeField] private string buildBuyButtonName = "BuyButton";
        [SerializeField] private string firstBuyButtonName = "FirstBuyButton";
        [SerializeField] private string costTextName = "RunBuildingCostText";
        //[SerializeField] private string levelTextName = "LevelText";
        [SerializeField] private string ItemContent1Name = "ItemContent1";
        [SerializeField] private string ItemContent2Name = "ItemContent2";
        [SerializeField] private string RunMaterials = "RunMaterialsText";
        [SerializeField] private string image_MaterialName = "RunMaterialImage";
        [SerializeField] private string RunProdName = "RunProductText";
        [SerializeField] private string image_ProdName = "RunProductImage";
        [SerializeField] private string BeforeBuyScreenName = "BeforeBuyScreen";
        [SerializeField] private string LockScreenName = "LockScreen";

        [SerializeField] private string UpgradeButtonName = "UpgradeButton";
        [SerializeField] private string UpgradeButtonTextName = "UpgradeButtonText";


        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI buildingText => GetUI<TextMeshProUGUI>(buildingTextName);
        private Button buyButton => GetUI<Button>(buildBuyButtonName);
        private Button firstbuyButton => GetUI<Button>(firstBuyButtonName);
        private TextMeshProUGUI costText => GetUI<TextMeshProUGUI>(costTextName);
        private TextMeshProUGUI MaterialsNameText => GetUI<TextMeshProUGUI>(RunMaterials);
        private TextMeshProUGUI ProdNameText => GetUI<TextMeshProUGUI>(RunProdName);
        private Image image_Material => GetUI<Image>(image_MaterialName);
        private Image image_Prod => GetUI<Image>(image_ProdName);
        //private TextMeshProUGUI levelText => GetUI<TextMeshProUGUI>(levelTextName);
        private GameObject BeforeBuyScreen => GetUI(BeforeBuyScreenName);
        private GameObject LockScreen => GetUI(LockScreenName);
        private GameObject ItemContent1 => GetUI(ItemContent1Name);
        private GameObject ItemContent2 => GetUI(ItemContent2Name);
        private Button UpgradeButton => GetUI<Button>(UpgradeButtonName);


        [Header("Build Settings")]
        [SerializeField] private string buildingName = "";
        private string buildingID;
        private int buildingCost = 0;
        private BuildingData currentBuildingData; // 현재 건물 데이터 저장

        protected override void Awake()
        {
            base.Awake();
            //Debug.LogWarning($"[PropertyContent] Awake 실행 - {gameObject.name}");

            // buyButton이 null인지 확인
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(() =>
                {
                    Debug.LogWarning($"구매 버튼 실행 - buildingID: {buildingID}");
                    BuyBuilding();
                });
                //Debug.LogWarning($"[PropertyContent] buyButton 이벤트 등록 완료: {buildBuyButtonName}");
            }
            else
            {
                Debug.LogError($"[PropertyContent] buyButton을 찾을 수 없습니다: {buildBuyButtonName}");
            }
        }

        // 그냥 구매버튼 & First구매버튼 둘 다 이 함수를 사용하도록 바꿨습니다.
        void BuyBuilding()
        {
            string purchasedBuildingID = Manager.firebase.UserData.CurStageData.PurchasedBuildingID.Value;
            BuildingSeller estate = Manager.buildings.buildingSeller;

            // 구매 불가능한 상황들
            if (!string.IsNullOrEmpty(purchasedBuildingID))
            {

                Manager.ui.ShowMessagePopUpWithKeyAsync("msg_already_have_building", () =>
                {
                    Debug.LogWarning("회수 영역에 이미 구매해둔 건물이 있을 때는 건물을 구매할 수 없습니다.");
                });

                return;
            }
            if (estate.IsOnHand())
            {


                Manager.ui.ShowMessagePopUpWithKeyAsync("msg_cannot_purchase_building_holding", () =>
                {
                    Debug.LogWarning("손에 뭔가 쥐고 있을 때는 건물을 구매할 수 없습니다");
                });

                return;
            }

            // 돈 체크 후 구매 진행
            int curMoney = Manager.player.Data.Money.Value;
            int cost = Manager.data.Building[buildingID].Cost;
            Debug.LogWarning($"CurMoney:{curMoney}, cost:{cost}");
            if (cost <= curMoney)
            {
                Debug.Log($"[PropertyContent] 구매 성공 - ClosePanel 호출 - Time: {Time.time}");

                Manager.player.Data.Money.Value -= cost;
                estate.SpawnBuildingItem(buildingID);
                Manager.ui.ClosePanel();

                Manager.Audio.SfxPlay("Money");
            }
            else
            {

                Manager.ui.ShowMessagePopUpWithKeyAsync("msg_not_enough_money", () =>
                {
                    Debug.LogWarning("돈이 모자랍니다");
                });
            }
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
            //Debug.LogWarning($"[PropertyContent] Initialize 실행 - {gameObject.name}");
            SetupButtons();
            UpdateUI();

            // 언어 변경 이벤트 구독
            BuildingLocalizationHelper.SubscribeToLanguageChanged(OnLanguageChanged);
            IngrediantLocalizationHelper.SubscribeToLanguageChanged(OnLanguageChanged);
        }

        public override void Cleanup()
        {
            // 언어 변경 이벤트 구독 해제
            BuildingLocalizationHelper.UnsubscribeFromLanguageChanged(OnLanguageChanged);
            IngrediantLocalizationHelper.UnsubscribeFromLanguageChanged(OnLanguageChanged);
            base.Cleanup();
        }

        private void SetupButtons()
        {
            //Debug.LogWarning($"[PropertyContent] SetupButtons 실행 - {gameObject.name}");
            //Debug.LogWarning($"[PropertyContent] firstBuyButtonName: {firstBuyButtonName}");
            //Debug.LogWarning($"[PropertyContent] UpgradeButtonName: {UpgradeButtonName}");

            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var buildEventHandler = GetEventWithSFX(firstBuyButtonName, "SFX_ButtonClick");
            if (buildEventHandler != null)
            {
                buildEventHandler.Click += (data) => OnFirstBuyClicked();
                //Debug.LogWarning($"[PropertyContent] 첫 구매 버튼 이벤트 설정 완료: {firstBuyButtonName}");
            }
            else
            {
                Debug.LogError($"[PropertyContent] 첫 구매 버튼을 찾을 수 없습니다: {firstBuyButtonName}");
            }

            var UpgradeEventHandler = GetEventWithSFX(UpgradeButtonName, "SFX_ButtonClick");
            if (UpgradeEventHandler != null)
            {
                UpgradeEventHandler.Click += (data) => OnUpgradeClicked();
                //Debug.LogWarning($"[PropertyContent] 업그레이드 버튼 이벤트 설정 완료: {UpgradeButtonName}");
            }
            else
            {
                Debug.LogError($"[PropertyContent] 업그레이드 버튼을 찾을 수 없습니다: {UpgradeButtonName}");
            }

        }

        private void UpdateUI()
        {

            if (costText != null)
                costText.text = $"{buildingCost}";
            else
                Debug.LogWarning($"비용 텍스트를 찾을 수 없습니다: {costTextName}");

        }

        public void SetBuildingData(BuildingData buildingData, UpgradeData upgradeData = null)
        {
            buildingID = buildingData.ID;
            buildingName = buildingData.Name;
            buildingCost = buildingData.Cost;
            currentBuildingData = buildingData; // 건물 데이터 저장

            if (buildingData is HarvestBD harvestBD)
            {
                // BuildingLocalizationHelper를 사용하여 건물 이름 번역
                buildingText.text = BuildingLocalizationHelper.GetBuildingName(buildingData.ID);
                // 재료(생산품) 이름, 스프라이트
                Addressables.LoadAssetAsync<IngrediantData>(harvestBD.ProductID).Completed += prodData =>
                {
                    SwitchAfrterBuyModeHarvestMode();
                    ProdNameText.text = IngrediantLocalizationHelper.GetIngrediantText(prodData.Result.ID);
                    image_Prod.sprite = prodData.Result.Sprite;
                };


                //업그레이드 데이터를 받아올 때 적용
                if (upgradeData != null)
                {
                    //buildingLevel = level; 
                }
            }
            else if (buildingData is ManufactureBD manufactureBD)
            {
                // BuildingLocalizationHelper를 사용하여 건물 이름 번역
                buildingText.text = BuildingLocalizationHelper.GetBuildingName(buildingData.ID);

                Addressables.LoadAssetAsync<IngrediantData>(manufactureBD.RequireProdID).Completed += requireData =>
                {
                    SwitchAfrterBuyModeManufactureMode();
                    MaterialsNameText.text = IngrediantLocalizationHelper.GetIngrediantText(requireData.Result.ID);
                    image_Material.sprite = requireData.Result.Sprite;
                };
                Addressables.LoadAssetAsync<IngrediantData>(manufactureBD.ProductID).Completed += prodData =>
                {
                    ProdNameText.text = IngrediantLocalizationHelper.GetIngrediantText(prodData.Result.ID);
                    image_Prod.sprite = prodData.Result.Sprite;
                };

                //업그레이드 데이터를 받아올 때 적용
                if (upgradeData != null)
                {
                    //buildingLevel = level; 
                }
            }

            UpdateUI();
        }


        private void OnFirstBuyClicked()
        {
            Debug.LogWarning($"[PropertyContent] OnFirstBuyClicked 실행 - buildingID: {buildingID}");
            BuyBuilding();
        }

        private void OnCancelButtonClicked()
        {
            Debug.Log("[BuildContent] 건설 취소");
            Hide();
        }

        private void OnUpgradeClicked()
        {
            Debug.LogWarning($"[PropertyContent] OnUpgradeClicked 실행 - currentBuildingData: {currentBuildingData?.Name}");

            // 저장된 BuildingData 타입에 따라 적절한 패널 열기
            if (currentBuildingData != null)
            {
                if (currentBuildingData is HarvestBD harvestBD)
                {
                    // 수확형 건물 업그레이드 패널 열기
                    Manager.ui.ShowPopUpAsync<InfoPanel_Harvest>((popup) =>
                    {
                        if (popup != null)
                        {
                            popup.SetUpgradeData(harvestBD);
                        }
                    });
                }
                else if (currentBuildingData is ManufactureBD manufactureBD)
                {
                    // 제조형 건물 업그레이드 패널 열기
                    Manager.ui.ShowPopUpAsync<InfoPanel_Manufacture>((popup) =>
                    {
                        if (popup != null)
                        {
                            popup.SetUpgradeData(manufactureBD);
                        }
                    });
                }
            }
        }



        [ContextMenu("구매 후 (모든 버튼 표시) 수확 모드")]
        public void SwitchAfrterBuyModeHarvestMode()
        {

            if (BeforeBuyScreen != null)
            {
                BeforeBuyScreen.SetActive(false);
            }
            if (LockScreen != null)
            {
                LockScreen.SetActive(false);
            }

            if (ItemContent1 != null)
            {
                ItemContent1.SetActive(true);
            }
            if (ItemContent2 != null)
            {
                ItemContent2.SetActive(false);
            }

        }

        [ContextMenu("구매 후 (모든 버튼 표시) 제조 모드")]
        public void SwitchAfrterBuyModeManufactureMode()
        {
            if (BeforeBuyScreen != null)
            {
                BeforeBuyScreen.SetActive(false);
            }
            if (LockScreen != null)
            {
                LockScreen.SetActive(false);
            }

            if (ItemContent1 != null)
            {
                ItemContent1.SetActive(true);
            }
            if (ItemContent2 != null)
            {
                ItemContent2.SetActive(true);
            }

        }

        [ContextMenu("구매 전 (Block 활성화)")]
        public void SwitchBeforeBuyMode()
        {
            if (BeforeBuyScreen != null)
            {
                BeforeBuyScreen.SetActive(true);
            }
            if (LockScreen != null)
            {
                LockScreen.SetActive(false);
            }

        }

        [ContextMenu("구매 Lock (Block 활성화 및 Lock 이미지)")]
        public void SwitchLockMode()
        {
            if (BeforeBuyScreen != null)
            {
                BeforeBuyScreen.SetActive(false);
            }
            if (LockScreen != null)
            {
                LockScreen.SetActive(true);
            }

        }

        /// <summary>
        /// 언어가 변경될 때 호출되는 메서드
        /// </summary>
        private void OnLanguageChanged(SystemLanguage newLanguage)
        {
            if (currentBuildingData != null)
            {
                // 건물 이름 다시 로드
                buildingText.text = BuildingLocalizationHelper.GetBuildingName(currentBuildingData.ID);

                // 재료 이름도 다시 로드
                if (currentBuildingData is HarvestBD harvestBD)
                {
                    Addressables.LoadAssetAsync<IngrediantData>(harvestBD.ProductID).Completed += prodData =>
                    {
                        ProdNameText.text = IngrediantLocalizationHelper.GetIngrediantText(prodData.Result.ID);
                    };
                }
                else if (currentBuildingData is ManufactureBD manufactureBD)
                {
                    Addressables.LoadAssetAsync<IngrediantData>(manufactureBD.RequireProdID).Completed += requireData =>
                    {
                        MaterialsNameText.text = IngrediantLocalizationHelper.GetIngrediantText(requireData.Result.ID);
                    };
                    Addressables.LoadAssetAsync<IngrediantData>(manufactureBD.ProductID).Completed += prodData =>
                    {
                        ProdNameText.text = IngrediantLocalizationHelper.GetIngrediantText(prodData.Result.ID);
                    };
                }
            }
        }
    }
}
