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
        
        btn_ProdTimeUpgrade.onClick.AddListener(UpgradeProdTime);
        btn_Close.onClick.AddListener(Close);
    }

    void UpgradeProdTime()
    {
        // 돈 차감
        int curLevel_ProdTime = Manager.buildings.GetUpgradeData(targetBD.ID).level_ProdTime;
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
        int curLevel_ProdTime = Manager.buildings.GetUpgradeData(data.ID).level_ProdTime;

        tmp_Name.text = data.Name;
        tmp_Description.text = data.Description;

        Addressables.LoadAssetAsync<IngrediantData>(data.ProductID).Completed += prodData =>
        {
            tmp_ProdName.text = prodData.Result.Name;
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
            tmp_AfterUpProdTime.text = $"이미 최대 단계입니다.";

            tmp_ProdTimeUpCost.text = $"최대 단계";

            btn_ProdTimeUpgrade.interactable = false;
        }
    }

    private void Close()
    {
           UIManager.Instance.ClosePanel();
    }
}
