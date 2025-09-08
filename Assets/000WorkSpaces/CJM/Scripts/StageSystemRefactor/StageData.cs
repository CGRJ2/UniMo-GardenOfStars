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

    public StageData(string id, string parentPath) : base(id, parentPath)
    {
        WorkerList = new FirebaseDataList<WorkerData>("WorkerList", Path, (id, parentPath) =>
        {
            return new WorkerData(id, parentPath);
        });
    }
}
