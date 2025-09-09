using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameNpc
{
    public class NpcData : FirebaseData
    {
        private NpcDataCsv _npcCsv => Manager.data.Npc.Values[Id];
        public string NpcName => _npcCsv.NpcName;
        public string StageId => _npcCsv.StageId;
        public string NpcImageLocation => _npcCsv.NpcImageLocation;
        public string[] FocusText => _npcCsv.FocusText;
        public string Description => _npcCsv.Description;
        public FirebaseProperty<bool> IsTalked;
        public NpcData(string id, string parentPath = null) : base(id, parentPath)
        {
            IsTalked = new FirebaseProperty<bool>("IsTalked", Path);
            InitList.Add(IsTalked);
        }
    }
}