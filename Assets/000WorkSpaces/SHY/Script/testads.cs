using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;



public class testads : MonoBehaviour
{
    [Header("광고 제거 상태")]
    public bool adRemoved = false;
    




    IEnumerator Start()
    {
        // 저장된 광고 제거 상태 불러오기
        yield return new WaitForSeconds(2f); // UI 조정이 끝난 후 실행되도록 대기
        ApplyBannerState();
       

    }
    
    public void ApplyBannerState()
    {
        if (adRemoved ==true)
        {
            Manager.ad.HideBannerAd();
            Debug.Log("광고 제거 상태 적용됨");
        }
        
        else if(GameObject.Find($"{Manager.ad.bannerSize}(Clone)") ==null)
        {
            Manager.ad.LoadBannerAd();
            Debug.Log("광고 표시 상태 적용됨");
        }
        else
        {
            Debug.Log("광고가 이미 제거되어있거나 표시중입니다.");
        }

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
            //Manager.player.Data.Money.Value += 333;
            FindAnyObjectByType<TestUi>()?.Reward();
        });
        //FindAnyObjectByType<AdManager>()?.ShowRewardedAd(() =>
        //{
        //    FindAnyObjectByType<TestUi>()?.Reward();
        //});
    }
}
