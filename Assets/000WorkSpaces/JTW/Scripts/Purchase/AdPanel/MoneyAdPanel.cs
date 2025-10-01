using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MoneyAdPanel : AdPanel
{
    protected override void GetReward()
    {
        int count = Manager.firebase.UserData.StageList.Count;

        if(count < 2)
        {
            Manager.player.Data.Money.Value += 250;
        }
        else if(count < 3)
        {
            Manager.player.Data.Money.Value += 300;
        }
        else
        {
            Manager.player.Data.Money.Value += 360;
        }
    }
}
