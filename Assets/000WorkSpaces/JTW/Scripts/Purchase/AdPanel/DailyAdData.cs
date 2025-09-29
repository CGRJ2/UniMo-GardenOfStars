using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyAdData : FirebaseData
{
    public FirebaseProperty<long> LastTime;

    public FirebaseProperty<int> Count;

    public DailyAdData(string id, string parentPath) : base(id, parentPath)
    {
        LastTime = new FirebaseProperty<long>("LastTime", Path);

        Count = new FirebaseProperty<int>("Count", Path);

        InitList.Add(Count);
        InitList.Add(LastTime);
    }
}
