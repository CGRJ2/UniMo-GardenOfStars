using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BuildingManager : Singleton<BuildingManager>
{
    public List<BuildingInstance> ActivatedBIList = new();
    public WorkStatoinLists workStatinLists = new ();

    public Action workStationActiveEvent;



    private void Awake() => Init();
    void Init()
    {
        base.SingletonInit();
    }



}

[Serializable]
public struct WorkStatoinLists
{
    public List<InsertArea> insertAreas;
    public List<WorkArea> workAreas;
    public List<WorkArea_SwitchType> workAreas_SwitchType;
    public List<ProdsArea> prodsAreas;
    public List<ProductGenerater> productGeneraters;

    public WorkStatoinLists(bool init = true)
    {
        insertAreas = new();
        workAreas = new();
        workAreas_SwitchType = new();
        prodsAreas = new();
        productGeneraters = new();
    }
}
