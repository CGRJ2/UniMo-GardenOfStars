using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameQuest;


[CreateAssetMenu(fileName = "QuestData", menuName = "SO/Quest/Base Data")]
public class CYETestQuestDataSO : ScriptableObject
{
    public int _id;
    public string _name;
    public QuestType _questType;
    public List<CYETestQuestProgressDataSO> _questContents;
    public string _description;
}
