using UnityEngine;

public class Payment : MonoBehaviour
{
    public void AddGold()
    {
        Manager.player.Data.Money.Value += 11111;//골드추가량
    }
    public void AddGem()
    {
        Manager.player.Data.Gem.Value += 111;//젬추가량
    }
    public void NoAds()
    {

    }
    public void AddMaterial()
    {
        //Manager.player.Data.(재료)
    }

}
