using System.Collections;
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
            foreach (QuestProgressData progressData in Manager.quest.CurrentQuest._questProgresses)
            {
                GameObject panel = _itemPanel[progressData._targetId];
                panel.GetComponent<QuestProgressPanel>()?.UpdateCurrentCountText(progressData._currentCount);
            }
        }
        public void InitItemPanel(int currentQuestIndex)
        {
            ResetItemPanel();
            foreach (QuestProgressData progressData in Manager.quest.CurrentQuest._questProgresses)
            {
                GameObject panel = Instantiate(_cellPrefab, _viewContent.transform);
                // panel.GetComponent<NpcProgressPanel>()?.UpdateItemImage(itemSprite);
                panel.GetComponent<QuestProgressPanel>()?.UpdateItemId(progressData._targetId);
                panel.GetComponent<QuestProgressPanel>()?.UpdateCurrentCountText(progressData._currentCount);
                panel.GetComponent<QuestProgressPanel>()?.UpdateTargetCountText(progressData._targetCount);
                if (!_itemPanel.ContainsKey(progressData._targetId)) { 
                    _itemPanel.Add(progressData._targetId, panel);
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

