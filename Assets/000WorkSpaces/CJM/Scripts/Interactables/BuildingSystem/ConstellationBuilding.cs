using GameQuest;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstellationBuilding : BuildingInstance
{
    public Quest curQuest;
    
    public List<Temp_Requirement> requirements;
    public QuestProdsInsertArea prodsInsertArea;

    private void Awake()
    {
        base.BIBaseInit();
        prodsInsertArea.Init(this);
        // 지역이 바뀔 때 마다 해당 지역의 퀘스트를 들고 초기화 => 현재 퀘스트 상태에 따라 requirements 갱신
        //curQuest = Manager.quest.GetCurrentQuest(); 
    }
}

[Serializable]
public class Temp_Requirement
{
    public bool isClear;
    public string prodId;
    public int curCount;
    public int needCount;
}
