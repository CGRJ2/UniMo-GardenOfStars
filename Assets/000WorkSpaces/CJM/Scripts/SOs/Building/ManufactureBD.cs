using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "SO/작업형 건물 데이터 클래스")]
public class ManufactureBD : ProductionBD
{
    [field: Header("투입 재료(소모) Id와 수량")]
    [field: SerializeField] public string RequireProdID { get; private set; }
    [field: SerializeField] public int RequireProdCound { get; private set; }
    [field: Header("재료 스택 가능 개수(업그레이드 표)")]
    [field: SerializeField] public UpgradableStat<int> Stat_Capacity { get; private set; }
}
