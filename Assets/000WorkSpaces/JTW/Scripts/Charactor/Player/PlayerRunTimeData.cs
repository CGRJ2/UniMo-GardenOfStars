using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunTimeData : CharaterRuntimeData
{
    public Vector3 Direction;

    public override int GetMaxCapacity()
    {
        return Manager.player.Data.MaxCapacity;
    }

    public override float GetProductionSpeed()
    {
        return Manager.player.Data.ProductionSpeed;
    }
}
