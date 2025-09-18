using KYS;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class InfoPanel_Manufacture : BaseUI
{
    ManufactureBD targetBD;

    [Header("건물 정보")]
    [SerializeField] TMP_Text tmp_Name;
    [SerializeField] TMP_Text tmp_Description;

    [Header("투입 재료 정보")]
    [SerializeField] TMP_Text tmp_RequireName;
    [SerializeField] Image image_Require;
    
    [Header("생산 재료 정보")]
    [SerializeField] TMP_Text tmp_ProdName;
    [SerializeField] Image image_Prod;

    [Header("생산 속도 스탯")]
    //[SerializeField] TMP_Text tmp_ProdTimeLevel;
    [SerializeField] TMP_Text tmp_CurProdTime;
    [SerializeField] TMP_Text tmp_AfterUpProdTime;

    [Header("생산 속도 업그레이드 비용")]
    [SerializeField] TMP_Text tmp_ProdTimeUpCost;

    [Header("최대 투입 개수 스탯")]
    //[SerializeField] TMP_Text tmp_CapacityLevel;
    [SerializeField] TMP_Text tmp_CurCapacity;
    [SerializeField] TMP_Text tmp_AfterUpCapacity;

    [Header("최대 투입 개수 업그레이드 비용")]
    [SerializeField] TMP_Text tmp_CapacityUpCost;

    [Header("업그레이드 버튼")]
    [SerializeField] Button btn_ProdTimeUpgrade;
    [SerializeField] Button btn_CapacityUpgrade;

    [Header("패널 닫기 버튼")]
    [SerializeField] Button btn_Close;

    //private void Awake() => Init();

    protected override void Awake()
    {
        base.Awake();
        // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
        if (layerType == UILayerType.Panel) // BaseUI의 기본값
        {
            layerType = UILayerType.Panel;
        }

        Init();
    }


    public void Init() // 초기화를 어디서 해줘야 할까요?
    {
        Manager.buildings.upgradeEvent += OnUpgradeEvent;

        btn_ProdTimeUpgrade.onClick.AddListener(UpgradeProdTime);
        btn_CapacityUpgrade.onClick.AddListener(UpgradeCapacity);
        btn_Close.onClick.AddListener(Close);
        
        // 언어 변경 이벤트 구독
        BuildingLocalizationHelper.SubscribeToLanguageChanged(OnLanguageChanged);
        IngrediantLocalizationHelper.SubscribeToLanguageChanged(OnLanguageChanged);
    }


    void UpgradeProdTime()
    {
        // 돈 차감
        UpgradeData upgradeData = Manager.buildings.GetUpgradeData(targetBD.ID);
        int curLevel_ProdTime = upgradeData == null ? 0 : upgradeData.level_ProdTime;

        Manager.player.Data.Money.Value -= (int)targetBD.Stat_ProdTime.cost[curLevel_ProdTime];

        // 업그레이드 스탯 적용
        Manager.buildings.UpdateUpgradedData(targetBD.ID, 1);

        // 패널 정보 업데이트
        SetUpgradeData(targetBD);

    }
    void UpgradeCapacity()
    {
        // 돈 차감
        UpgradeData upgradeData = Manager.buildings.GetUpgradeData(targetBD.ID);
        int curLevel_Capacity = upgradeData == null ? 0 : upgradeData.level_Capacity;

        Manager.player.Data.Money.Value -= (int)targetBD.Stat_Capacity.cost[curLevel_Capacity];

        // 업그레이드 스탯 적용
        Manager.buildings.UpdateUpgradedData(targetBD.ID, 0, 1);

        // 패널 정보 업데이트
        SetUpgradeData(targetBD);
    }

    public void SetUpgradeData(ManufactureBD manufacture)
    {
        ManufactureBD data = manufacture;
        targetBD = data;
        int curMoney = Manager.player.Data.Money.Value;
        UpgradeData upgradeData = Manager.buildings.GetUpgradeData(targetBD.ID);
        int curLevel_ProdTime = upgradeData == null ? 0 : upgradeData.level_ProdTime;
        int curLevel_Capacity = upgradeData == null ? 0 : upgradeData.level_Capacity;

        // 새로운 BuildingLocalizationHelper 사용
        tmp_Name.text = BuildingLocalizationHelper.GetBuildingName(data.ID);
        tmp_Description.text = BuildingLocalizationHelper.GetBuildingDescription(data.ID);

        // (0918 최재민 수정)
        tmp_RequireName.text = IngrediantLocalizationHelper.GetIngrediantText(data.RequireProdID);
        image_Require.sprite = Manager.data.Ingrediant[data.RequireProdID].Sprite;

        tmp_ProdName.text = IngrediantLocalizationHelper.GetIngrediantText(data.ProductID);
        image_Prod.sprite = Manager.data.Ingrediant[data.ProductID].Sprite;

        //Addressables.LoadAssetAsync<IngrediantData>(data.RequireProdID).Completed += requireData =>
        //{
        //    tmp_RequireName.text = IngrediantLocalizationHelper.GetIngrediantText(requireData.Result.ID);
        //    image_Require.sprite = requireData.Result.Sprite;
        //};
        //Addressables.LoadAssetAsync<IngrediantData>(data.ProductID).Completed += prodData =>
        //{
        //    tmp_ProdName.text = IngrediantLocalizationHelper.GetIngrediantText(prodData.Result.ID);
        //    image_Prod.sprite = prodData.Result.Sprite;
        //};

        // 생산 속도 업그레이드 정보
        if (curLevel_ProdTime < data.Stat_ProdTime.MaxLevel)
        {
            //tmp_ProdTimeLevel.text = $"{curLevel_ProdTime}";
            tmp_ProdTimeUpCost.text = $"{data.Stat_ProdTime.cost[curLevel_ProdTime]}";
            tmp_CurProdTime.text = $"{data.Stat_ProdTime.Values[curLevel_ProdTime]}";
            tmp_AfterUpProdTime.text = $"{data.Stat_ProdTime.Values[curLevel_ProdTime + 1]}";

            // 업그레이드 버튼 활성화/비활성화 여부
            if (curMoney > data.Stat_ProdTime.cost[curLevel_ProdTime])
            {
                btn_ProdTimeUpgrade.interactable = true;
            }
            else
            {
                btn_ProdTimeUpgrade.interactable = false;
            }
        }
        else
        {
            Debug.Log("생산 속도가 최대 단계입니다");
            tmp_CurProdTime.text = $"{data.Stat_ProdTime.Values[curLevel_ProdTime]}";
            tmp_AfterUpProdTime.text = Manager.localization.GetText("MaxLevelReached");

            tmp_ProdTimeUpCost.text = Manager.localization.GetText("MaxLevel");

            btn_ProdTimeUpgrade.interactable = false;
        }

        // 최대 투입 개수 업그레이드 정보
        if (curLevel_Capacity < data.Stat_Capacity.MaxLevel)
        {
            //tmp_CapacityLevel.text = $"{curLevel_Capacity}";
            tmp_CapacityUpCost.text = $"{data.Stat_Capacity.cost[curLevel_Capacity]}";
            tmp_CurCapacity.text = $"{data.Stat_Capacity.Values[curLevel_Capacity]}";
            tmp_AfterUpCapacity.text = $"{data.Stat_Capacity.Values[curLevel_Capacity + 1]}";

            // 업그레이드 버튼 활성화/비활성화 여부
            if (curMoney > data.Stat_Capacity.cost[curLevel_Capacity])
            {
                btn_CapacityUpgrade.interactable = true;
            }
            else
            {
                btn_CapacityUpgrade.interactable = false;
            }
        }
        else
        {
            Debug.Log("최대 투입 개수가 최대 단계입니다");
            tmp_CurCapacity.text = $"{data.Stat_Capacity.Values[curLevel_Capacity]}";
            tmp_AfterUpCapacity.text = Manager.localization.GetText("MaxLevelReached");

            tmp_CapacityUpCost.text = Manager.localization.GetText("MaxLevel");

            btn_CapacityUpgrade.interactable = false;
        }
    }

    void OnUpgradeEvent(int value)
    {
        // 패널 정보 업데이트
        SetUpgradeData(targetBD);
    }

    private void Close()
    {
        UIManager.Instance.ClosePopup();
    }

    protected override void OnDestroy()
    {
        // 언어 변경 이벤트 구독 해제
        BuildingLocalizationHelper.UnsubscribeFromLanguageChanged(OnLanguageChanged);
        IngrediantLocalizationHelper.UnsubscribeFromLanguageChanged(OnLanguageChanged);
    }

    private void OnLanguageChanged(SystemLanguage newLanguage)
    {
        // 언어가 변경되면 건물 이름과 설명 업데이트
        if (targetBD != null)
        {
            tmp_Name.text = BuildingLocalizationHelper.GetBuildingName(targetBD.ID);
            tmp_Description.text = BuildingLocalizationHelper.GetBuildingDescription(targetBD.ID);

            // 재료 이름도 다시 로드
            tmp_RequireName.text = IngrediantLocalizationHelper.GetIngrediantText(targetBD.RequireProdID);
            tmp_ProdName.text = IngrediantLocalizationHelper.GetIngrediantText(targetBD.ProductID);
        }
    }

}
