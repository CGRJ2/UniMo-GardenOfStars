using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameQuest;

[SerializeField]
[CreateAssetMenu(fileName = "QuestContent", menuName = "SO/Quest/Content Data")]
public class CYETestQuestContentDataSO : ScriptableObject
{
    public int _questId;
    public string _targetId;    
    public int _targetCount;
}
