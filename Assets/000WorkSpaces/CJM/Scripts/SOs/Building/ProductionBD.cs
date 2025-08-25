using UnityEngine;

public class ProductionBD : BuildingData
{
    [field: Header("����� ��� Id")]
    [field: SerializeField] public string ProductID { get; private set; }
    
    [field: Header("���� �ð�(���׷��̵� ǥ)")]
    [field: SerializeField] public UpgradableStat<float> Stat_ProductionTime { get; private set; }

}
