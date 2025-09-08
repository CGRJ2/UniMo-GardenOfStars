using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class testads : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Mainclick()
    {
        Manager.ad.ShowInterstitialAd();
        //FindAnyObjectByType<AdManager>()?.ShowInterstitialAd();
    }

    public void RewardClick()
    {
        Manager.ad.ShowRewardedAd(() =>
        {
            FindAnyObjectByType<TestUi>()?.Reward();
        });
        //FindAnyObjectByType<AdManager>()?.ShowRewardedAd(() =>
        //{
        //    FindAnyObjectByType<TestUi>()?.Reward();
        //});
    }
}
