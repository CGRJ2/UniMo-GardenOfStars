using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameNpc
{ 
    public class NpcQuestUI : MonoBehaviour
    {
        [SerializeField] private int _slots;
        private List<GameObject> _displayArea = new();
        void Awake()
        {
            Init();
        }
        void Init()
        {
            // UI 카메라 방향으로 돌려놓기
            transform.forward = Camera.main.transform.forward;
        }
    }    
}

