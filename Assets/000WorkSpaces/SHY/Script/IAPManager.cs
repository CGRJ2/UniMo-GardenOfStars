using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IDetailedStoreListener
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
        storeController?.InitiatePurchase("noads");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
        Debug.Log("✅ IAP 초기화 완료");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"❌ IAP 초기화 실패: {error}, 메시지: {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (args.purchasedProduct.definition.id == "noads")
        {
            Debug.Log("✅ 광고 제거 상품 구매 완료");
            PlayerPrefs.SetInt("noads", 1);
            PlayerPrefs.Save();
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogError($"❌ 구매 실패: {product.definition.id}, 이유: {failureDescription.reason}, 메시지: {failureDescription.message}");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        throw new System.NotImplementedException();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        throw new System.NotImplementedException();
    }
}
