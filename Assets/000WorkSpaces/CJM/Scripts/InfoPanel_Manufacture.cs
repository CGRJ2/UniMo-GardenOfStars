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
        btn_ProdTimeUpgrade.onClick.AddListener(UpgradeProdTime);
        btn_CapacityUpgrade.onClick.AddListener(UpgradeCapacity);
        btn_Close.onClick.AddListener(Close);
    }


    void UpgradeProdTime()
    {
        // 업그레이드 스탯 적용
        Manager.buildings.UpdateUpgradedData(targetBD.ID, 1);

        // 돈 차감
        int curLevel_ProdTime = Manager.buildings.GetUpgradeData(targetBD.ID).level_ProdTime;
        Manager.player.Data.Money -= (int)targetBD.Stat_ProdTime.cost[curLevel_ProdTime];

        // 패널 정보 업데이트
        SetUpgradeData(targetBD);

    }
    void UpgradeCapacity()
    {
        // 업그레이드 스탯 적용
        Manager.buildings.UpdateUpgradedData(targetBD.ID, 0, 1);

        // 돈 차감
        int curLevel_Capacity = Manager.buildings.GetUpgradeData(targetBD.ID).level_Capacity;
        Manager.player.Data.Money -= (int)targetBD.Stat_Capacity.cost[curLevel_Capacity];

        // 패널 정보 업데이트
        SetUpgradeData(targetBD);
    }

    public void SetUpgradeData(ManufactureBD manufacture)
    {
        ManufactureBD data = manufacture;
        targetBD = data;
        int curMoney = Manager.player.Data.Money;
        int curLevel_ProdTime = Manager.buildings.GetUpgradeData(data.ID).level_ProdTime;
        int curLevel_Capacity = Manager.buildings.GetUpgradeData(data.ID).level_Capacity;

        tmp_Name.text = data.Name;
        tmp_Description.text = data.Description;
        Addressables.LoadAssetAsync<IngrediantData>(data.RequireProdID).Completed += requireData =>
        {
            tmp_RequireName.text = requireData.Result.Name;
            image_Require.sprite = requireData.Result.Sprite;
        };
        Addressables.LoadAssetAsync<IngrediantData>(data.ProductID).Completed += prodData =>
        {
            tmp_ProdName.text = prodData.Result.Name;
            image_Prod.sprite = prodData.Result.Sprite;
        };

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
            tmp_AfterUpProdTime.text = $"이미 최대 단계입니다.";

            tmp_ProdTimeUpCost.text = $"최대 단계";

            btn_ProdTimeUpgrade.interactable = false;
        }

        // 최대 투입 개수 업그레이드 정보
        if (curLevel_Capacity < data.Stat_Capacity.MaxLevel)
        {
            //tmp_CapacityLevel.text = $"{curLevel_Capacity}";
            tmp_CapacityUpCost.text = $"{data.Stat_ProdTime.cost[curLevel_Capacity]}";
            tmp_CurCapacity.text = $"{data.Stat_Capacity.Values[curLevel_Capacity]}";
            tmp_AfterUpCapacity.text = $"{data.Stat_ProdTime.Values[curLevel_Capacity + 1]}";

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
            tmp_AfterUpCapacity.text = $"이미 최대 단계입니다.";

            tmp_CapacityUpCost.text = $"최대 단계";

            btn_CapacityUpgrade.interactable = false;
        }
    }


    private void Close()
    {
        UIManager.Instance.ClosePanel();
    }



}
