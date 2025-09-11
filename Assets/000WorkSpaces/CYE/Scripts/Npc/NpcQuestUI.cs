/*using System.Collections;
using System.Collections.Generic;
using GameQuest;
using UnityEngine;

namespace GameNpc
{
    public class NpcQuestUI : MonoBehaviour
    {
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _viewContent;
        public Dictionary<string, GameObject> _itemPanel = new();
        void Start()
        {
            Init();
        }
        void Init()
        {
            InitItemPanel(Manager.quest.CurrentQuestIndex.Value);
            // 이벤트 구독
            Manager.quest.OnQuestProgressUpdate += UpdateProgressUI;
            // TO DO: 나중에 제외 필요(대화시 퀘스트가 받아지도록 변경 예정)
            Manager.quest.CurrentQuestIndex.Subscribe(InitItemPanel);
        }

        public void UpdateProgressUI()
        {
            foreach (QuestContentProgressData progressData in Manager.quest.CurrentQuest._progresses)
            {
                GameObject panel = _itemPanel[progressData.ContentTargetId];
                panel.GetComponent<QuestProgressPanel>()?.UpdateCurrentCountText((int)progressData.ProgressdProdsCount.Value);
            }
        }
        public void InitItemPanel(int currentQuestIndex)
        {
            ResetItemPanel();
            foreach (QuestContentProgressData progressData in Manager.quest.CurrentQuest._progresses)
            {
                GameObject panel = Instantiate(_cellPrefab, _viewContent.transform);
                // panel.GetComponent<NpcProgressPanel>()?.UpdateItemImage(itemSprite);
                panel.GetComponent<QuestProgressPanel>()?.UpdateItemId(progressData.ContentTargetId);
                panel.GetComponent<QuestProgressPanel>()?.UpdateCurrentCountText((int)progressData.ProgressdProdsCount.Value);
                panel.GetComponent<QuestProgressPanel>()?.UpdateTargetCountText(progressData.CurrentTargetCount);
                if (!_itemPanel.ContainsKey(progressData.ContentTargetId)) { 
                    _itemPanel.Add(progressData.ContentTargetId, panel);
                }
            }
        }
        private void ResetItemPanel()
        {
            _itemPanel = new();
            foreach (Transform child in _viewContent.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }    
}

*/