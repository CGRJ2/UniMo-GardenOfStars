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

    private bool _isInPurchase;

    protected override void Awake()
    {
        base.Awake();
        _getTitleText = GetUI<TextMeshProUGUI>("GemTitleText");
        _moneyTitleText = GetUI<TextMeshProUGUI>("MoneyTitleText");
    }

    public void StartPurchase(string productId)
    {
        if (_isInPurchase) return;

        _isInPurchase = true;

        CodelessIAPStoreListener.Instance.InitiatePurchase(productId);
    }

    public void OnOrderConfirmed(ConfirmedOrder order)
    {
        _isInPurchase = false;
    }

    public void OnPurchasesFailed(FailedOrder order)
    {
        _isInPurchase = false;
    }
}
