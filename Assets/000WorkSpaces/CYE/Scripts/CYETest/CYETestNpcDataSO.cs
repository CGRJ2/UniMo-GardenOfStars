using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameQuest;
using System;

[System.Serializable]
public struct NpcDialogue
{
    public int targetCount;
    public string keyword;
    public List<string> dialogue;
}

[CreateAssetMenu(fileName = "QuestData", menuName = "SO/Npc/Base Data")]
public class CYETestNpcDataSO : ScriptableObject
{
    public int _id;
    public string _name;
    public string _stageId;
    public List<NpcDialogue> _dialogue = new();
    public string _description;
}
