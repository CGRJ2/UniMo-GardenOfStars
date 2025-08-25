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
        // ������ �ٲ� �� ���� �ش� ������ ����Ʈ�� ��� �ʱ�ȭ => ���� ����Ʈ ���¿� ���� requirements ����
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
