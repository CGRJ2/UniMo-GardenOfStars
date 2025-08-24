using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameQuest
{
    public class QuestBaseData
    {
        #region 
        private int _id;
        public int Id { get { return _id; } }
        private QuestType _questType;
        public QuestType QuestType { get { return _questType; } }
        private Dictionary<string, int> _questContents;
        public Dictionary<string, int> QuestContents { get { return _questContents; } }
        private string _description;
        public string Description { get { return _description; } }
        #endregion

        #region 
        public void Init(string rawData)
        {
            
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