using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class HarvestBuilding : BuildingInstance
{
    [SerializeField] Transform prodsParentTransform;
    //[SerializeField] float cultivateTime;
    public HarvestRuntimeData runtimeData;
    [HideInInspector] public HarvestBD originData;

    ProductGenerater[] productGeneraters;
    ObjectPool _Pool;

    private void Awake()
    {
        base.BIBaseInit();
        InitRuntimeData();
        activatePopUI.Init(this);

        productGeneraters = prodsParentTransform.GetComponentsInChildren<ProductGenerater>();
        SetIngrediantToGeneraters();
    }

    void InitRuntimeData()
    {
        if (_OriginData is HarvestBD harvestBD)
        {
            originData = harvestBD;
            runtimeData = new();
            runtimeData.SetCurLevelStatDatas(harvestBD);
        }
    }

    public void SetIngrediantToGeneraters()
    {
        string productId = originData.ProductID;
        Addressables.LoadAssetAsync<GameObject>(productId).Completed += task =>
        {
            GameObject product = task.Result;

            _Pool = Manager.pool.GetPoolBundle(product).instancePool;
            foreach (ProductGenerater prodsGenerater in productGeneraters)
            {
                prodsGenerater.Init(product, runtimeData.productionTime);
            }
        };
    }
}
[Serializable]
public class HarvestRuntimeData
{
    public int level_ProductionTime;
    public float productionTime;

    public void SetCurLevelStatDatas(HarvestBD harvestBD)
    {
        this.productionTime = harvestBD.Stat_ProductionTime.Values[level_ProductionTime];
    }
}