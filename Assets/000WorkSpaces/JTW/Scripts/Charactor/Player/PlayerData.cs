using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    // TODO : MoveSpeed 같은 값은 DataManager에서 가져오는 것으로 변경?

    public float MoveSpeed;
    public ObservableProperty<int> MoveSpeedLv;

    public int MaxCapacity;
    public ObservableProperty<int> MaxCapacityLv;

    public float ProductionSpeed;
    public ObservableProperty<int> ProductionSpeedLv;

    public float Nego;
    public ObservableProperty<int> NegoLv;

    public ObservableProperty<int> Money;

    public PlayerData()
    {
        MoveSpeed = 5;
        MoveSpeedLv.Value = 1;

        MaxCapacity = 5;
        MaxCapacityLv.Value = 1;

        ProductionSpeed = 2;
        ProductionSpeedLv.Value = 1;

        Nego = 20;
        NegoLv.Value = 1;

        Money.Value = 0;
    }
}
