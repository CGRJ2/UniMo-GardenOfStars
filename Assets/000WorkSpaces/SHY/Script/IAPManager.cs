using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener
{
    private IStoreController storeController;
    private IExtensionProvider extensionProvider;

    void Start()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct("noads", ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public void BuyNoAds()
    {
        storeController.InitiatePurchase("noads");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
        Debug.Log("✅ IAP 초기화 완료");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"❌ IAP 초기화 실패: {error}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (args.purchasedProduct.definition.id == "noads")
        {
            Debug.Log("✅ 광고 제거 상품 구매 완료");
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"❌ 구매 실패: {product.definition.id}, 이유: {failureReason}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message = null)
    {
        throw new System.NotImplementedException();
    }
}
