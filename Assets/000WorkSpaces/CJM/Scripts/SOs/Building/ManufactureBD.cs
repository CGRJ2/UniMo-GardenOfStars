using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "SO/작업형 건물 데이터 클래스")]
public class ManufactureBD : ProductionBD
{
    [field: SerializeField] public string RequireProdID { get; private set; }
    [field: SerializeField] public UpgradableStat<int> Stat_MaxStackableCount { get; private set; }
}
