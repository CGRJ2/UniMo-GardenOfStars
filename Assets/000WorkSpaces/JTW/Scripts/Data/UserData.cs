using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public partial class UserData : FirebaseData
{
    public FirebaseDataList<WorkerData> WorkerList;

    public PlayerData Player;

    public FirebaseDataList<UpgradeData> BuildingUpgradeList;

    public UserData(string id, string parentPath = null) : base(id, parentPath)
    {
        WorkerList = new FirebaseDataList<WorkerData>("WorkerList", Path, (id, parentPath) =>
        {
            return new WorkerData(id, parentPath);
        });
        InitList.Add(WorkerList);

        Player = new PlayerData("Player", Path);
        InitList.Add(Player);

        BuildingUpgradeList = new FirebaseDataList<UpgradeData>("BuildingUpgradeList", Path, (id, parentPath) =>
        {
            return new UpgradeData(id, parentPath);
        });
        InitList.Add(BuildingUpgradeList);
    }
}
