using GameQuest;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public partial class UserData : FirebaseData
{
    public PlayerData Player;

    public FirebaseDataList<UpgradeData> BuildingUpgradeList;

    public FirebaseDataList<StageData> StageList;

    public FirebaseProperty<string> CurStage;

    public FirebaseProperty<int> TutorialSequence;

    public FirebaseProperty<bool> AdRemoved; // 광고제거 상품 구매 여부

    public FirebaseDataList<DailyAdData> DailyAdList;

    public StageData CurStageData => StageList.Get(CurStage.Value);



    public UserData(string id, string parentPath = null) : base(id, parentPath)
    {
        StageList = new FirebaseDataList<StageData>("StageList", Path, (id, parentPath) =>
        {
            return new StageData(id, parentPath);
        });
        InitList.Add(StageList);

        Player = new PlayerData("Player", Path);
        InitList.Add(Player);

        BuildingUpgradeList = new FirebaseDataList<UpgradeData>("BuildingUpgradeList", Path, (id, parentPath) =>
        {
            return new UpgradeData(id, parentPath);
        });
        InitList.Add(BuildingUpgradeList);

        CurStage = new FirebaseProperty<string>("CurStage", Path, "Tutorial");
        InitList.Add(CurStage);

        AdRemoved = new FirebaseProperty<bool>("AdRemoved", Path);
        InitList.Add(AdRemoved);

        DailyAdList = new FirebaseDataList<DailyAdData>("DailyAdList", Path, (id, parentPath) =>
        {
            return new DailyAdData(id, parentPath);
        });
        InitList.Add(DailyAdList);

        TutorialSequence = new FirebaseProperty<int>("TutorialSequence", Path);
        InitList.Add(TutorialSequence);
    }
}
