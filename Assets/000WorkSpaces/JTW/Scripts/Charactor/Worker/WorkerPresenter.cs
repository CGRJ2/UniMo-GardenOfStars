using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerPresenter : MonoBehaviour
{
    void Start()
    {
        WorkerRuntimeData data = GetComponent<WorkerRuntimeData>();

        GameObject avatarPrefab = Manager.data.Character.Values[$"{data.Id}_{Manager.firebase.UserData.CurStage.Value}"].Avatar;

        Instantiate(avatarPrefab, data.transform);
    }
}
