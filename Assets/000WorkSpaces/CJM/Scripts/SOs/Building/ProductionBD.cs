using UnityEngine;

public class ProductionBD : BuildingData
{
    [field: SerializeField] public string ProductID { get; private set; }
    [field: SerializeField] public UpgradableStat<float> Stat_ProductionTime { get; private set; }

}
