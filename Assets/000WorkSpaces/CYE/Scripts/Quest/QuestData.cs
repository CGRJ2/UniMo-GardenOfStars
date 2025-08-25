using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameQuest
{
    public class QuestBaseData : IComparer<QuestBaseData>
    {
        #region 
        public int _id;
        // public int Id { get { return _id; } }
        public int _npcId;
        public int _questOrder;
        public string _name;
        // public string Name { get { return _name; } }
        public QuestType _questType;
        // public QuestType QuestType { get { return _questType; } }
        public string _description;
        // public string Description { get { return _description; } }
        #endregion

        #region 
        public void Init(string rawData)
        {

        }
        // public void InitContents(CYETestQuestContentDataSO[] contentList)
        // {
        //     foreach (CYETestQuestContentDataSO item in contentList)
        //     {
        //         _questContents.Add(item._targetId, item._targetCount);
        //     }
        // }
        int IComparer<QuestBaseData>.Compare(QuestBaseData dataA, QuestBaseData dataB)
        {
            if (dataA._npcId != dataB._npcId)
            {
                Debug.LogError("[QuestData.cs] 잘못된 비교입니다.");
                return 0;
            }
            if (dataA._questOrder.CompareTo(dataB._questOrder) != 0)
            {
                return dataA._questOrder.CompareTo(dataB._questOrder);
            }
            return 0;
        }
        #endregion
    }
    // public class QuestDataSO : ScriptableObject
    // {
    //     #region 
    //     private int _id;
    //     public int Id { get { return _id; } }
    //     private QuestType _questType;
    //     public QuestType QuestType { get { return _questType; } }
    //     private Dictionary<string, int> _questContents;
    //     public Dictionary<string, int> QuestContents { get { return _questContents; } }
    //     private string _description;
    //     public string Description { get { return _description; } }
    //     #endregion

    //     #region 
    //     public virtual void Init(string rawData)
    //     {
    //         // rawdata를 SO의 데이터 구조 형식으로 변환하여 저장
    //     }
    //     public virtual void Synchronize()
    //     { 
            
    //     }
    //     #endregion
    // }
}