using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsTest : MonoBehaviour
{
    private void Awake()
    {
        MobileAds.Initialize(OnInitalzed);
        
        
    }
    private void OnInitalzed(InitializationStatus status)
    { 
        if(status == null)
        {
            Debug.LogError("모바일 광고 초기화 실패..");
            return;
        }

        Debug.Log("모바일 광고 초기화 성공");
    }
}
