using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    // TODO : MoveSpeed 같은 값은 DataManager에서 가져오는 것으로 변경?

    public float MoveSpeed;
    public int MoveSpeedLv;

    public int MaxCapacity;
    public int MaxCapacityLv;

    public float ProductionSpeed;
    public int ProductionSpeedLv;

    public float Nego;
    public int NegoLv;

    public int Money;

    public PlayerData()
    {
        MoveSpeed = 5;
        MoveSpeedLv = 1;

        MaxCapacity = 5;
        MaxCapacityLv = 1;

        ProductionSpeed = 2;
        ProductionSpeedLv = 1;

        Nego = 20;
        NegoLv = 1;

        Money = 0;
    }
}
