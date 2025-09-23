using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyAdPanel : AdPanel
{
    protected override void GetReward()
    {
        Manager.player.Data.Money.Value += 200;
    }
}
