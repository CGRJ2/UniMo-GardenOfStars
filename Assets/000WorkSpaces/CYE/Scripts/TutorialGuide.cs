using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGuide : MonoBehaviour
{
    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        Manager.npc.SetCurrentNpc("tutorial");
        Manager.quest.SetQuestsOnRegion("tutorial");
    }

    private void StartGuide()
    {
        // 카메라 무빙 => 플레이어 포커스        
    }

}
