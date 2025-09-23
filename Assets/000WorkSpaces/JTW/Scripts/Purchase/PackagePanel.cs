using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class PackagePanel : KYS.BaseUI
{
    [SerializeField] private CodelessIAPButton _iapButton;
    [SerializeField] private ShopPanel _shopPanel;

    // 번역이 필요할 Text
    private TextMeshProUGUI _nameText => GetUI<TextMeshProUGUI>("PackageName");
    private TextMeshProUGUI _descriptionText => GetUI<TextMeshProUGUI>("PackageDesc");
    private TextMeshProUGUI _buttonText => GetUI<TextMeshProUGUI>("PackageButtonText");

    private Button _buyButton => GetUI<Button>("PackageButton");

    private Image _completeImage => GetUI<Image>("CompleteBG");
    private Image _soldImage => GetUI<Image>("SoldImage");

    protected override void Awake()
    {
        base.Awake();

        _iapButton = GetComponent<CodelessIAPButton>();

        _buyButton.onClick.AddListener(OnClick);

        if (Manager.firebase.UserData.AdRemoved.Value)
        {
            _buttonText.text = "구매 완료";
            _completeImage.gameObject.SetActive(true);
            _soldImage.gameObject.SetActive(true);
            _buyButton.interactable = false;
        }
        else
        {
            _buttonText.text = "KRW 8900";
        }

        _iapButton.onOrderConfirmed.AddListener(_shopPanel.OnOrderConfirmed);
        _iapButton.onPurchaseFailed.AddListener(_shopPanel.OnPurchasesFailed);
        _iapButton.onOrderConfirmed.AddListener(OnOrderConfirmed);
    }

    public void SetInfo(string productId, ShopPanel shopPanel)
    {
        _shopPanel = shopPanel;

        CodelessIAPStoreListener.Instance.RemoveButton(_iapButton);

        _iapButton.productId = productId;

        CodelessIAPStoreListener.Instance.AddButton(_iapButton);
    }

    private void OnClick()
    {
        if (Manager.firebase.UserData.AdRemoved.Value) return;

        _shopPanel.StartPurchase(_iapButton.productId);
    }

    private void OnOrderConfirmed(ConfirmedOrder order)
    {
        Manager.firebase.UserData.AdRemoved.Value = true;
        _buttonText.text = "구매 완료";
        _buyButton.interactable = false;
    }
}
