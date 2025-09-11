using GameQuest;
using System;
using System.Linq;

namespace GameNpc
{
    [Serializable]
    public class NpcData : FirebaseData
    {
        private NpcDataCsv _npcCsv => Manager.data.Npc.Values[Id];
        public string NpcName => _npcCsv.NpcName;
        public string StageId => _npcCsv.StageId;
        public string NpcImageLocation => _npcCsv.NpcImageLocation;
        public string[] FocusText => _npcCsv.FocusText;
        public string Description => _npcCsv.Description;

        // 대화 관련
        public FirebaseProperty<string> NpcID;
        public FirebaseProperty<bool> IsTalked;

        // 퀘스트 관련
        public FirebaseProperty<string> CurrentQuestID;
        public FirebaseDataList<QuestBaseData> QuestList;
        public QuestBaseData CurQuestData => QuestList.Get(CurrentQuestID.Value);


        public NpcData(string id, string parentPath = null) : base(id, parentPath)
        {
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