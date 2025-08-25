using KYS;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class InfoPanel_Manufacture : BaseUI
{
    [Header("�ǹ� ����")]
    [SerializeField] TMP_Text tmp_Name;
    [SerializeField] TMP_Text tmp_Description;
    [SerializeField] TMP_Text tmp_RequireName;
    [SerializeField] Sprite sprite_Require;
    [SerializeField] TMP_Text tmp_ProdName;
    [SerializeField] Sprite sprite_Prod;

    [Header("���� �ӵ� ����")]
    //[SerializeField] TMP_Text tmp_ProdTimeLevel;
    [SerializeField] TMP_Text tmp_ProdTimeUpCost;
    [SerializeField] TMP_Text tmp_CurProdTime;
    [SerializeField] TMP_Text tmp_AfterUpProdTime;

    [Header("��� ���� �뷮 ����(�۾��� �ǹ� ����)")]
    //[SerializeField] TMP_Text tmp_CapacityLevel;
    [SerializeField] TMP_Text tmp_CapacityUpCost;
    [SerializeField] TMP_Text tmp_CurCapacity;
    [SerializeField] TMP_Text tmp_AfterUpCapacity;


    public void SetUpgradeData(ManufactureBuilding manufacture)
    {
        int curMoney = Manager.player.Data.Money;

        int curLevel_ProdTime = manufacture.runtimeData.level_ProductionTime;
        int curLevel_Capacity = manufacture.runtimeData.level_StackCount;
        ManufactureBD data = manufacture.originData;
        tmp_Name.text = data.Name;
        tmp_Description.text = data.Description;
        Addressables.LoadAssetAsync<IngrediantData>(data.RequireProdID).Completed += requireData =>
        {
            tmp_RequireName.text = requireData.Result.Name;
            sprite_Require = requireData.Result.Sprite;
        };
        Addressables.LoadAssetAsync<IngrediantData>(data.ProductID).Completed += prodData =>
        {
            tmp_RequireName.text = prodData.Result.Name;
            sprite_Require = prodData.Result.Sprite;
        };

        //tmp_ProdTimeLevel.text = $"{curLevel_ProdTime}";
        tmp_ProdTimeUpCost.text = $"{manufacture.originData.Stat_ProductionTime.cost[curLevel_ProdTime]}";
        tmp_CurProdTime.text = $"{manufacture.runtimeData.productionTime}";
        tmp_AfterUpProdTime.text = $"{manufacture.originData.Stat_ProductionTime.Values[curLevel_ProdTime + 1]}";

        //tmp_CapacityLevel.text = $"{curLevel_Capacity}";
        tmp_CapacityUpCost.text = $"{manufacture.originData.Stat_ProductionTime.cost[curLevel_Capacity]}";
        tmp_CurCapacity.text = $"{manufacture.runtimeData.capacity}";
        tmp_AfterUpCapacity.text = $"{manufacture.originData.Stat_ProductionTime.Values[curLevel_Capacity + 1]}";
    }
}
