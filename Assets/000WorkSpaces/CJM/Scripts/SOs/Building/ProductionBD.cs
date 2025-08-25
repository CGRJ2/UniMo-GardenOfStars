using UnityEngine;

public class ProductionBD : BuildingData
{
    [field: Header("생산될 재료 Id")]
    [field: SerializeField] public string ProductID { get; private set; }
    
    [field: Header("생산 시간(업그레이드 표)")]
    [field: SerializeField] public UpgradableStat<float> Stat_ProductionTime { get; private set; }

}
