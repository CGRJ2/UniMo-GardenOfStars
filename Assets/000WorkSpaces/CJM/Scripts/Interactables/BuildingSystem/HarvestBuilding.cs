using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class HarvestBuilding : BuildingInstance
{
    [SerializeField] Transform prodsParentTransform;
    [HideInInspector] public HarvestBD originData;

    public float ProdTime // 현재 업그레이드 단계에 따른 [생산 속도]
    {
        get
        {
            int level = Manager.buildings.GetUpgradeData(originData.ID).level_ProdTime;
            return originData.Stat_ProdTime.Values[level];
        }
    }

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
                prodsGenerater.Init(product, ProdTime);
            }
        };
    }
}
