using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameNpc
{
    public class NpcController : MonoBehaviour
    {
        [SerializeField] Transform requireTilesParent;
        [SerializeField] Transform view;
        public Transform view_Dissolve;
        QuestRequireTile[] requireTiles;

        [SerializeField] private Transform _focusPopUpCanvas;
        void Awake()
        {
            // 타이틀씬에서 시작 시
            //Init();


            // 스테이지씬에서 시작 시
            StartCoroutine(WaitAndInit());
        }

        IEnumerator WaitAndInit()
        {
            yield return new WaitUntil(() => Manager.firebase.IsFirebaseInit);
            yield return new WaitUntil(() => Manager.firebase.UserData != null);
            yield return new WaitUntil(() => Manager.firebase.UserData.IsInit);
            //Debug.LogWarning("UserData Inited");

            yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData != null);
            yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.IsInit);
            //Debug.LogWarning("CurStageData Inited");

            yield return new WaitUntil(() => Manager.npc.CurStageNpc != null);
            //Debug.LogWarning("Npc Inited");

            yield return new WaitUntil(() => !string.IsNullOrEmpty(Manager.npc.CurStageNpc.CurrentQuestID.Value));
            //Debug.LogWarning("CurQuestID Inited");

            yield return new WaitUntil(() => Manager.npc.CurStageNpc.QuestList.IsInit); // <<<<<<=== Error
            //Debug.LogWarning("QuestList Inited");

            yield return new WaitUntil(() => Manager.npc.CurStageNpc.CurQuestData != null);
            //Debug.LogWarning("CurQuestData Inited");

            yield return new WaitUntil(() => Manager.npc.CurStageNpc.CurQuestData.QuestContentList.IsInit);
            //Debug.LogWarning("QuestContentList Inited");


            Init();
        }

        private void Init()
        {
            requireTiles = requireTilesParent.GetComponentsInChildren<QuestRequireTile>(true);

            if (Manager.firebase.UserData.CurStage.Value != "Tutorial")
            {
                UpdateQuestData();

                // 스테이지ID에 맞는 NPC ID의 메쉬와 재질로 설정해주기
                //view.GetComponent<MeshFilter>

            }
            else
            {
                // 튜토리얼 퀘스트가 진행중인 시퀀스 01, 05, 08에는 퀘스트 발판 바로 띄우기
                if (Manager.firebase.UserData.TutorialSequence.Value == 1 ||
                    Manager.firebase.UserData.TutorialSequence.Value == 5 ||
                    Manager.firebase.UserData.TutorialSequence.Value == 8) UpdateQuestData();

                // 석상 깨어난 상태 => 우주 재질
                if (Manager.firebase.UserData.TutorialSequence.Value >= 9)
                {
                    view_Dissolve.gameObject.SetActive(false);
                }
                // 깨어나지 않은 상태 => 돌 재질
                else
                {
                    view_Dissolve.gameObject.SetActive(true);
                }

                TutorialManager.Instance.tutorialNPC = this;
            }

            Manager.npc.CurStageNpc.CurrentQuestID.Subscribe(UpdateQuestData);

            Manager.camera.cam_NpcFocus.Follow = transform;
        }

        public void UpdateQuestData(string questID = null)
        {
            Debug.LogWarning($"퀘스트 발판 업데이트(현재 퀘스트ID : {Manager.npc.CurStageNpc.CurrentQuestID.Value})");


            // QC데이터가 있는 만큼만 발판 활성화
            var QCDataList = Manager.npc.CurStageNpc.CurQuestData.QuestContentList.List;
            for (int i = 0; i < QCDataList.Count; i++)
            {
                requireTiles[i].gameObject.SetActive(true);

                requireTiles[i].QC_Data = QCDataList[i];
                requireTiles[i].SetUp();
            }
            if (QCDataList.Count < requireTiles.Length)
            {
                for (int i = QCDataList.Count; i < requireTiles.Length; i++)
                {
                    requireTiles[i].gameObject.SetActive(false);
                }
            }
        }

        public void HideQuestTiles()
        {
            requireTilesParent.gameObject.SetActive(false);
        }

        public void ShowQuestTiles()
        {
            requireTilesParent.gameObject.SetActive(true);
        }
    }
}
