using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingData : ScriptableObject
{
    [field: SerializeField] public string ID { get; private set; }
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public GameObject Prefab  { get; private set; }
}

public class ProductionBD : BuildingData 
{
    [field: SerializeField] public string ProductID { get; private set; }
    [field: SerializeField] public UpgradableStat<float> Stat_ProductionTime { get; private set; }

}


[CreateAssetMenu(fileName = "Building", menuName = "SO/��Ȯ�� �ǹ� ������ Ŭ����")]
public class HarvestBD: ProductionBD
{

}

[CreateAssetMenu(fileName = "Building", menuName = "SO/�۾��� �ǹ� ������ Ŭ����")]
public class ManufactureBD: ProductionBD
{
    [field: SerializeField] public string RequireProdID { get; private set; }
    [field: SerializeField] public UpgradableStat<int> Stat_MaxStackableCount { get; private set; }
}

[Serializable]
public class UpgradableStat<T> 
{
    [field: SerializeField] public int MaxLevel { get; private set; }
    [field: SerializeField] public List<T> Values { get; private set; }
    [field: SerializeField] public List<double> cost { get; private set; }

}