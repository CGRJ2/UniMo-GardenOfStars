using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public partial class UserData : FirebaseData
{
    public FirebaseDataList<WorkerData> WorkerList;

    public PlayerData Player;

    public UserData(string id, string parentPath = null) : base(id, parentPath)
    {
        WorkerList = new FirebaseDataList<WorkerData>("WorkerList", Path, (id, parentPath) =>
        {
            return new WorkerData(id, parentPath);
        });
        InitList.Add(WorkerList);

        Player = new PlayerData("Player", Path);
        InitList.Add(Player);
    }
}
