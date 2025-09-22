using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Purchasing;

public class ShopPanel : KYS.BaseUI
{
    // 번역이 필요한 Text
    private TextMeshProUGUI _getTitleText;
    private TextMeshProUGUI _moneyTitleText;

    public bool IsInPurchase;

    protected override void Awake()
    {
        base.Awake();
        _getTitleText = GetUI<TextMeshProUGUI>("GemTitleText");
        _moneyTitleText = GetUI<TextMeshProUGUI>("MoneyTitleText");
    }

    public void StartPurchase(string productId)
    {
        if (IsInPurchase) return;

        IsInPurchase = true;

        CodelessIAPStoreListener.Instance.InitiatePurchase(productId);
    }

    public void OnOrderConfirmed(ConfirmedOrder order)
    {
        IsInPurchase = false;
    }

    public void OnPurchasesFailed(FailedOrder order)
    {
        IsInPurchase = false;
    }
}
