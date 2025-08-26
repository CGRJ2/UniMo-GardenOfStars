using System.Collections;
using System.Collections.Generic;
using GameQuest;
using UnityEngine;

namespace GameNpc
{
    public class NpcQuestUI : MonoBehaviour
    {
        public int _slots;
        public List<GameObject> _displayArea = new();
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
            // UI 카메라 방향으로 돌려놓기
            transform.forward = Camera.main.transform.forward;
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
                panel.GetComponent<NpcProgressPanel>()?.UpdateCurrentCountText(progressData._currentCount);
            }
        }
        public void InitItemPanel(int currentQuestIndex)
        {
            ResetItemPanel();
            foreach (QuestProgressData progressData in Manager.quest.CurrentQuest._questProgresses)
            {
                GameObject panel = Instantiate(_cellPrefab, _viewContent.transform);
                // panel.GetComponent<NpcProgressPanel>()?.UpdateItemImage(itemSprite);
                panel.GetComponent<NpcProgressPanel>()?.UpdateCurrentCountText(progressData._currentCount);
                panel.GetComponent<NpcProgressPanel>()?.UpdateTargetCountText(progressData._targetCount);
                _itemPanel.Add(progressData._targetId, panel);
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

