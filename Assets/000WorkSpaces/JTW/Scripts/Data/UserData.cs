using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class UserData : FirebaseData
{
    public FirebaseDataList<WorkerData> WorkerList;

    public UserData(string id, string parentPath = null) : base(id, parentPath)
    {

        WorkerList = new FirebaseDataList<WorkerData>("WorkerList", Path, (id, parentPath) =>
        {
            return new WorkerData(id, parentPath);
        });
    }
}
