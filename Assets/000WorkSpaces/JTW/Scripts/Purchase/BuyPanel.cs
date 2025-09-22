using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyPanel : MonoBehaviour
{
    [SerializeField] private GameObject _buyButtonPrefab;
    [SerializeField] private RewardTypes _rewardTypes;

    [SerializeField] private ShopPanel _shopPanel;

    private void Awake()
    {
        foreach (var value in Manager.data.Buy.Values)
        {
            if (value.Value.RewardType != _rewardTypes) continue;

            BuyButton buy = Instantiate(_buyButtonPrefab, transform).GetComponent<BuyButton>();

            buy.SetInfo(value.Key, _shopPanel);
        }

    }
}
