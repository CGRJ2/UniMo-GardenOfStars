using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "SO/�۾��� �ǹ� ������ Ŭ����")]
public class ManufactureBD : ProductionBD
{
    [field: Header("���� ������ ���(�Ҹ�) Id")]
    [field: SerializeField] public string RequireProdID { get; private set; }
    [field: Header("��� ���� ���� ����(���׷��̵� ǥ)")]
    [field: SerializeField] public UpgradableStat<int> Stat_MaxStackableCount { get; private set; }
}
