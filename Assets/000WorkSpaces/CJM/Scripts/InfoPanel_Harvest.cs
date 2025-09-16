using KYS;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class InfoPanel_Harvest : BaseUI
{
    HarvestBD targetBD;

    [Header("건물 정보")]
    [SerializeField] TMP_Text tmp_Name;
    [SerializeField] TMP_Text tmp_Description;

    [Header("생산 재료 정보")]
    [SerializeField] TMP_Text tmp_ProdName;
    [SerializeField] Image image_Prod;

    [Header("생산 속도 스탯")]
    //[SerializeField] TMP_Text tmp_ProdTimeLevel;
    [SerializeField] TMP_Text tmp_CurProdTime;
    [SerializeField] TMP_Text tmp_AfterUpProdTime;

    [Header("생산 속도 업그레이드 비용")]
    [SerializeField] TMP_Text tmp_ProdTimeUpCost;

    [Header("업그레이드 버튼")]
    [SerializeField] Button btn_ProdTimeUpgrade;

    [Header("패널 닫기 버튼")]
    [SerializeField] Button btn_Close;



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

    public void Init()  // 초기화를 어디서 해줘야 할까요?
    {
        Manager.buildings.upgradeEvent += OnUpgradeEvent;

        btn_ProdTimeUpgrade.onClick.AddListener(UpgradeProdTime);
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

    public void SetUpgradeData(HarvestBD harvest)
    {
        HarvestBD data = harvest;
        targetBD = data;

        int curMoney = Manager.player.Data.Money.Value;

        UpgradeData upgradeData = Manager.buildings.GetUpgradeData(targetBD.ID);
        int curLevel_ProdTime = upgradeData == null ? 0 : upgradeData.level_ProdTime;

        // 기존 LocalizationManager와 DataManager를 활용한 번역 시스템 사용
        tmp_Name.text = BuildingLocalizationHelper.GetBuildingName(data.ID);
        tmp_Description.text = BuildingLocalizationHelper.GetBuildingDescription(data.ID);

        Addressables.LoadAssetAsync<IngrediantData>(data.ProductID).Completed += prodData =>
        {
            tmp_ProdName.text = IngrediantLocalizationHelper.GetIngrediantText(prodData.Result.ID);
            image_Prod.sprite = prodData.Result.Sprite;
        };

        if (curLevel_ProdTime < data.Stat_ProdTime.MaxLevel)
        {
            tmp_ProdTimeUpCost.text = $"{data.Stat_ProdTime.cost[curLevel_ProdTime]}";
            tmp_CurProdTime.text = $"{data.Stat_ProdTime.Values[curLevel_ProdTime]}";
            tmp_AfterUpProdTime.text = $"{data.Stat_ProdTime.Values[curLevel_ProdTime + 1]}";
            
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
    
    /// <summary>
    /// 언어가 변경될 때 호출되는 메서드
    /// </summary>
    private void OnLanguageChanged(SystemLanguage newLanguage)
    {
        if (targetBD != null)
        {
            // 건물 정보 다시 로드
            tmp_Name.text = BuildingLocalizationHelper.GetBuildingName(targetBD.ID);
            tmp_Description.text = BuildingLocalizationHelper.GetBuildingDescription(targetBD.ID);
            
            // 재료 이름도 다시 로드
            Addressables.LoadAssetAsync<IngrediantData>(targetBD.ProductID).Completed += prodData =>
            {
                tmp_ProdName.text = IngrediantLocalizationHelper.GetIngrediantText(prodData.Result.ID);
            };
        }
    }
}
