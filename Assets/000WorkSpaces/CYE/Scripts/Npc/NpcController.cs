using System.Collections;
using System.Collections.Generic;
using GameQuest;
using UnityEngine;

namespace GameNpc
{
    public class NpcController : MonoBehaviour // InteractableBase
    {
        [SerializeField] private GameObject _interactPoint;

        void Awake()
        {
            Init();
        }
        // private void OnTriggerEnter(Collider other)
        // {
        //     // NOTICE: 현재 layer physics 설정을 통해 자기 자신에 의해 trigger되는건 막아두었으나 맵에 의해 trigger되는건 막혀있지않음.
        //     Debug.Log($"{other.CompareTag("Player")}");
        //     if (other.CompareTag("Player"))
        //     {
        //         // player로 부터 들고있는 재료 정보를 들고와서
        //         // Manager.quest.GetCurrentQuest().TryUpdateProgress("건물의id", 완성된 건물(1개), 남는재료갯수);
        //         // TryUpdateProgress 완료시
        //         // 재료를 차감하는 함수 실행
        //     }
        // }
        // void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.blue;
        //     Gizmos.DrawWireCube(_interactPoint.transform.position, new Vector3(1, 0.25f, 1));
        // }
        private void Init()
        {

        }
        public void UpdateQuestUI()
        {
            foreach (QuestProgressData item in Manager.quest.CurrentQuest.QuestProgresses)
            {
                // item에 해당하는 icon 및 갯수를 가져와서 업데이트
                Debug.Log($"{item.TargetId}");
            }
        }
        public void ReceiveProduct()
        {     
            // interact 발판에 있는 재료 정보를 들고와서
            Manager.quest.UpdateCurrentQuestProgress("재료ID", 1);
            // interact 발판에 있는 재료를 차감함
        }
    }
}
