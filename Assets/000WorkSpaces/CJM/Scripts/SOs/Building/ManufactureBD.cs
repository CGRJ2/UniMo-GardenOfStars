using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "SO/�۾��� �ǹ� ������ Ŭ����")]
public class ManufactureBD : ProductionBD
{
    [field: SerializeField] public string RequireProdID { get; private set; }
    [field: SerializeField] public UpgradableStat<int> Stat_MaxStackableCount { get; private set; }
}
