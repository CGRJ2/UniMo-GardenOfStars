using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameNpc
{ 
    public class NpcQuestUI : MonoBehaviour
    {
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

