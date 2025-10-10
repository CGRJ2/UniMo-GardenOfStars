using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class HarvestBuilding : BuildingInstance
{
    [SerializeField] Transform prodsParentTransform;
    [HideInInspector] public HarvestBD originData;

    ProductGenerater[] productGeneraters;
    ObjectPool _Pool;

    private void Awake() => Init();
    
    public override void Init()
    {
        base.Init();

        if (Manager.data.Building.Values[ID] is HarvestBD harvestBD) originData = harvestBD;

        activatePopUI.Init(this);

        productGeneraters = prodsParentTransform.GetComponentsInChildren<ProductGenerater>();
        SetIngrediantToGeneraters();
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
                prodsGenerater.Init(product, originData);
            }
        };
    }
}
