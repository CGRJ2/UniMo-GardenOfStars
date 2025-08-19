using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BuildingInstance_Harvest : BuildingInstance
{
    [SerializeField] IngrediantSO resultIngrediantData; // CSV or Sheet로 변경 예정
    [SerializeField] Transform prodsParentTransform;
    [SerializeField] float cultivateTime;
    ProductGenerater[] productGeneraters;
     
    private void Awake()
    {
        base.InitPopUI();

        productGeneraters = prodsParentTransform.GetComponentsInChildren<ProductGenerater>();

        if (resultIngrediantData != null) SetIngrediantToGeneraters();
    }

    public void SetIngrediantToGeneraters()
    {
        foreach(ProductGenerater prodsGenerater in productGeneraters)
        {
            prodsGenerater.Init(resultIngrediantData, cultivateTime);
        }
    }
}
