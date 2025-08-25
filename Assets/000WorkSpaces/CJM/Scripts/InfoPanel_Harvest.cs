using KYS;
using TMPro;
using UnityEngine;

public class InfoPanel_Harvest : BaseUI
{
    [Header("생산 속도 스탯")]
    [SerializeField] TMP_Text tmp_ProdTimeLevel;
    [SerializeField] TMP_Text tmp_ProdTimeUpCost;
    [SerializeField] TMP_Text tmp_CurProdTime;
    [SerializeField] TMP_Text tmp_AfterUpProdTime;


    public void SetUpgradeData(HarvestBuilding harvest)
    {
        int curMoney = Manager.player.Data.Money;

        int curLevel_ProdTime = harvest.runtimeData.level_ProductionTime;

        tmp_ProdTimeLevel.text = $"{curLevel_ProdTime}";
        tmp_ProdTimeUpCost.text = $"{harvest.originData.Stat_ProductionTime.cost[curLevel_ProdTime]}";
        tmp_CurProdTime.text = $"{harvest.runtimeData.productionTime}";
        tmp_ProdTimeLevel.text = $"{harvest.originData.Stat_ProductionTime.Values[curLevel_ProdTime + 1]}";
    }
}
