using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingData : ScriptableObject
{
    [field: SerializeField] public string ID { get; private set; }
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public GameObject Prefab  { get; private set; }
}

[Serializable]
public class UpgradableStat<T> 
{
    [field: SerializeField] public int MaxLevel { get; private set; }
    [field: SerializeField] public List<T> Values { get; private set; }
    [field: SerializeField] public List<double> cost { get; private set; }

}