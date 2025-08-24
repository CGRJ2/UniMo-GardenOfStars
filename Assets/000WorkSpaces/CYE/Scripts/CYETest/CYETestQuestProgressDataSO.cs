using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameQuest;

[CreateAssetMenu(fileName = "QuestProgressData", menuName = "SO/Quest/Progress Data")]
public class CYETestQuestProgressDataSO : ScriptableObject
{
    public string _targetId;
    public int _targetCount;
    public int _currentCount;
    public QuestProgressState _currentState;
}
