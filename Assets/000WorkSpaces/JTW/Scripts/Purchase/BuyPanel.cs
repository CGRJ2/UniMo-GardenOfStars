using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyPanel : MonoBehaviour
{
    [SerializeField] private GameObject _buyButtonPrefab;
    [SerializeField] private RewardTypes _rewardTypes;

    [SerializeField] private GameObject _itemPanel;
    [SerializeField] private GameObject _contentPanel;
    [SerializeField] private ShopPanel _shopPanel;

    private void Start()
    {
        foreach (var value in Manager.data.Buy.Values)
        {
            if (value.Value.RewardType != _rewardTypes) continue;

            BuyButton buy = Instantiate(_buyButtonPrefab, transform).GetComponent<BuyButton>();

            buy.SetInfo(value.Key, _shopPanel);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(_itemPanel.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_contentPanel.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_shopPanel.GetComponent<RectTransform>());
    }
}
