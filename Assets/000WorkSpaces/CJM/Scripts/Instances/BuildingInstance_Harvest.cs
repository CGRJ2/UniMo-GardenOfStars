using UnityEngine;

public class BuildingInstance_Harvest : BuildingInstance
{
    [SerializeField] IngrediantSO resultIngrediantSO;
    [SerializeField] Transform prodsParentTransform;
    ProductGenerater[] productGeneraters;

    private void Awake()
    {
        productGeneraters = prodsParentTransform.GetComponentsInChildren<ProductGenerater>();

        if (resultIngrediantSO != null) SetIngrediantToGeneraters();
    }

    public void SetIngrediantToGeneraters()
    {
        foreach(ProductGenerater prodsGenerater in productGeneraters)
        {
            prodsGenerater.SetProduct(resultIngrediantSO);
        }
    }
}
