using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Purchasing;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public enum CostTypes
{
    RealMoney, Gem
}

public enum RewardTypes
{
    Gem, Money
}

public class BuyButton : KYS.BaseUI
{
    [SerializeField] private CodelessIAPButton _iapButton;

    private Button _buyButton;
    private Image _buttonImage => GetUI<Image>("ButtonImage");
    private Image _rewardImage => GetUI<Image>("BuyImage");
    private Image _costTypeImage => GetUI<Image>("CostTypeImage");

    private TextMeshProUGUI _rewardText => GetUI<TextMeshProUGUI>("RewardText");
    private TextMeshProUGUI _costText => GetUI<TextMeshProUGUI>("CostText");
    private TextMeshProUGUI _costTypeText => GetUI<TextMeshProUGUI>("CostTypeText");

    private ShopPanel _shopPanel;

    private int _cost;
    private CostTypes _costType;
    
    private int _reward;
    private RewardTypes _rewardType;
    

    protected override void Awake()
    {
        base.Awake();

        _buyButton = GetComponent<Button>();

        _buyButton.onClick.AddListener(OnClick);

    }

    public void SetInfo(string productId, ShopPanel shopPanel)
    {
        _shopPanel = shopPanel;

        CodelessIAPStoreListener.Instance.RemoveButton(_iapButton);
        _iapButton.productId = productId;
        CodelessIAPStoreListener.Instance.AddButton(_iapButton);

        BuyButtonDataCsv data = Manager.data.Buy.Values[productId];

        _costText.text = data.Cost.ToString();
        _cost = data.Cost;

        _rewardText.text = data.Reward.ToString();
        _reward = data.Reward;

        if (data.CostType == CostTypes.RealMoney)
        {
            _costTypeText.gameObject.SetActive(true);
        }
        else
        {
            if (ColorUtility.TryParseHtmlString("#3CBD5A", out Color color))
            {
                _buttonImage.color = color;
            }

            _costTypeImage.gameObject.SetActive(false);
            _costTypeImage.gameObject.SetActive(true);
        }
        _costType = data.CostType;

        _rewardType = data.RewardType;

        _rewardImage.sprite = data.RewardSprite;

        _iapButton.onOrderConfirmed.AddListener(_shopPanel.OnOrderConfirmed);
        _iapButton.onPurchaseFailed.AddListener(_shopPanel.OnPurchasesFailed);
        _iapButton.onOrderConfirmed.AddListener(OnOrderConfirmed);
    }

    private void OnClick()
    {
        if (_shopPanel.IsInPurchase) return;

        if(_costType == CostTypes.RealMoney)
        {
            _shopPanel.StartPurchase(_iapButton.productId);
        }
        else
        {
            if (Manager.player.Data.Gem.Value < _cost || Manager.player.Data.Gem.IsInUpdate) return;

            Manager.player.Data.Gem.Subscribe(OnBought);
            Manager.player.Data.Gem.Value -= _cost;
        }
    }

    private void OnOrderConfirmed(ConfirmedOrder order)
    {
        if(_rewardType == RewardTypes.Gem)
        {
            Manager.player.Data.Gem.Value += _reward;
        }
        else
        {
            Manager.player.Data.Money.Value += _reward;
        }
    }

    private void OnBought(int value = 0)
    {
        if (_rewardType == RewardTypes.Gem)
        {
            Manager.player.Data.Gem.Value += _reward;
        }
        else
        {
            Manager.player.Data.Money.Value += _reward;
        }
        Manager.player.Data.Gem.Unsubscribe(OnBought);
    }
}

public class BuyButtonDataCsv : IUsableId
{
    public string Id;

    public int Cost;
    public CostTypes CostType;

    public int Reward;
    public RewardTypes RewardType;
    public Sprite RewardSprite;

    public string GetId()
    {
        return Id;
    }
}

public partial class DataManager
{
    [SerializeField] private bool _isBuyAdressable = true;

    // 구글 스프레드 시트 다운로드 주소
    private const string _buyDataTableURL = "https://docs.google.com/spreadsheets/d/1CwrcyyODjYAwjCgYkofKQl815o-vOWkUH7yy6mdUtY4/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _buyAdress = "BuyCsv";

    public DataTableParser<BuyButtonDataCsv> Buy;
    private async void BuyRoutine()
    {
        string dataCsv;

        if (_isBuyAdressable)
        {
            dataCsv = await GetDataString(_isBuyAdressable, _buyAdress);
        }
        else
        {
            dataCsv = await GetDataString(_isBuyAdressable, _buyDataTableURL);
        }

        Buy = new DataTableParser<BuyButtonDataCsv>((words, dict) =>
        {
            BuyButtonDataCsv buy = new BuyButtonDataCsv();

            buy.Id = words[dict["Id"]];

            int.TryParse(words[dict["Cost"]], out buy.Cost);
            Enum.TryParse<CostTypes>(words[dict["CostType"]], true, out buy.CostType);

            int.TryParse(words[dict["Reward"]], out buy.Reward);
            Enum.TryParse<RewardTypes>(words[dict["RewardType"]], true, out buy.RewardType);

            if (Addressables.ResourceLocators.Any(locator => locator.Locate($"Image/{words[dict["RewardType"]]}.png", typeof(Sprite), out var locations)))
            {
                Addressables.LoadAssetAsync<Sprite>($"Image/{words[dict["RewardType"]]}.png").Completed += task =>
                {
                    if (task.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogWarning("RewardType 로드 실패");
                        return;
                    }

                    buy.RewardSprite = task.Result;
                };
            }

            return buy;
        });

        Buy.Load(dataCsv);
    }
}
