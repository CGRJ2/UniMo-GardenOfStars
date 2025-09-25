using GameNpc;
using GameQuest;
using GooglePlayGames.BasicApi;
using System;
using UnityEngine;

[Serializable]
public partial class StageData : FirebaseData
{
    public string StageId;
    public string stageName;
    public int requiredQuestIndex;
    public string nextStageId;

    public FirebaseDataList<WorkerData> WorkerList;
    
    public FirebaseDataList<PlaceTileData> PlaceTileList;
    public FirebaseProperty<string> PurchasedBuildingID;

    public FirebaseProperty<long> StageLastExitTime;



    public NpcData Npc;
    


    public StageData(string id, string parentPath) : base(id, parentPath)
    {
        WorkerList = new FirebaseDataList<WorkerData>("WorkerList", Path, (id, parentPath) =>
        {
            return new WorkerData(id, parentPath);
        });

        PlaceTileList = new FirebaseDataList<PlaceTileData>("PlaceTileList", Path, (id, parentPath) =>
        {
            return new PlaceTileData(id, parentPath);
        });

        PurchasedBuildingID = new FirebaseProperty<string>("PurchasedBuildingID", Path);
        StageLastExitTime = new FirebaseProperty<long>("StageLastExitTime", Path);
        // 해당 스테이지의 id에 존재하는 NPCId
        Npc = new NpcData("NpcData", Path);

        InitList.Add(WorkerList);
        InitList.Add(PlaceTileList);
        InitList.Add(PurchasedBuildingID);
        InitList.Add(Npc);
        InitList.Add(StageLastExitTime);
    }
}
