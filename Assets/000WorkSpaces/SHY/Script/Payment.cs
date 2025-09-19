using UnityEngine;

public class Payment : MonoBehaviour
{
    public void AddGold(int value)
    {//골드 추가량
        Manager.firebase.UserData.Player.Money.Value += value;
        Debug.Log($"골드 구매됨 추가량 : {value}");
    }
    public void AddGem(int value)
    {//임시 주석 중/  젬 추가
        //Manager.firebase.UserData.Player.Gem.Value += value;
        Debug.Log($"젬 구매됨 추가량 : {value}");
    }
    public void NoAds()
    {
        Debug.Log("광고제거 구매");
        Manager.firebase.UserData.AdRemoved.Value = true;
    }
    public void AddMaterial()
    {
        
    }
    public void AddSkin()
    {
       //스킨구매 
       Debug.Log("스킨 구매 감사합니다.");
    }

    public void AddWorker()
    {
        //워커구매
        Debug.Log("워커 구매 감사합니다.");
    }
   
    public void PassDailyReward()
    {
        //데일리패스 구매
        Debug.Log("데일리패스 구매 감사합니다.");
    }

    //결제 구글콘솔에 만들기.
    //
}
