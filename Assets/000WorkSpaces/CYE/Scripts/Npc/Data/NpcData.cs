using GameQuest;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace GameNpc
{
    [Serializable]
    public class NpcData : FirebaseData
    {
        //private NpcDataCsv _npcCsv => Manager.data.Npc.Values[Id];
        //public string NpcName => _npcCsv.Name_KR;
        //public string Description => _npcCsv.Description;
        //public List<string> TextLines_KR = new();
        //public List<string> TextLines_EN = new();

        // 대화 관련
        public FirebaseProperty<string> NpcID;
        public FirebaseProperty<bool> IsTalked;

        // 퀘스트 관련
        public FirebaseProperty<string> CurrentQuestID;
        public FirebaseDataList<QuestBaseData> QuestList;
        public QuestBaseData CurQuestData => QuestList.Get(CurrentQuestID.Value);


        public NpcData(string id, string parentPath = null) : base(id, parentPath)
        {
            /*var textlines = Manager.data.NpcTextLines.Values.Where((kvp) => kvp.Value.NpcId == Id);
            foreach (KeyValuePair<string, NpcTextLineDataCsv> kvp in textlines)
            {
                TextLines_KR.Add(kvp.Value.TextLine_KR);
                TextLines_EN.Add(kvp.Value.TextLine_EN);
            }*/

            NpcID = new FirebaseProperty<string>("ID", Path);
            InitList.Add(NpcID);

            IsTalked = new FirebaseProperty<bool>("IsTalked", Path);
            InitList.Add(IsTalked);

            CurrentQuestID = new FirebaseProperty<string>("CurrentQuestID", Path);
            InitList.Add(CurrentQuestID);

            QuestList = new FirebaseDataList<QuestBaseData>("QuestDataList", Path, (id, parentPath) =>
                {
                    return new QuestBaseData(id, parentPath);
                });
            InitList.Add(QuestList);
        }
    }
}