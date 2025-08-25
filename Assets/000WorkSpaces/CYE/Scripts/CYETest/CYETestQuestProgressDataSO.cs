using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameQuest;

[SerializeField]
[CreateAssetMenu(fileName = "QuestProgressData", menuName = "SO/Quest/Progress Data")]
public class CYETestQuestProgressDataSO : ScriptableObject
{
    public int _questId;
    public string _targetId;
    public int _currentCount;
    public QuestProgressState _currentState;
}
