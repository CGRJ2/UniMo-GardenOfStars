using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameQuest;
using System;


[CreateAssetMenu(fileName = "QuestData", menuName = "SO/Quest/Base Data")]
public class CYETestQuestDataSO : ScriptableObject, IComparable
{
    public int _id;
    public string _name;
    public QuestType _questType;
    // public List<CYETestQuestProgressDataSO> _questContents;
    public string _npcId;
    public int _questOrder;
    public string _description;
    int IComparable.CompareTo(object data)
    {
        if (data == null) return 1;
        CYETestQuestDataSO convertData = data as CYETestQuestDataSO;
        if (this._npcId != convertData._npcId)
        {
            Debug.LogError("[QuestData.cs] 잘못된 비교입니다.");
            return 0;
        }
        if (this._questOrder.CompareTo(convertData._questOrder) != 0)
        {
            return this._questOrder.CompareTo(convertData._questOrder);
        }
        return 0;
    }
}
