using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemAdPanel : AdPanel
{
    protected override void GetReward()
    {
        Manager.player.Data.Gem.Value += 10;
    }
}
