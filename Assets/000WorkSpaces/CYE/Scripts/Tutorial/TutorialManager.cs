using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using GameNpc;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private static TutorialManager _instance;
    public static TutorialManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TutorialManager>();
            }
            return _instance;
        }
    }

    public int Sequence = 0;
    [SerializeField] private CinemachineBrain _cinemachineBrain;

    public NpcController tutorialNPC;

    private void Start()
    {
        StartCoroutine(Init());
        
        //Manager.ui.SwitchToTutorialProgressHUD(); // 譬配府倔侩 HUD 老何 剁快扁
    }
    private IEnumerator Init()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("[TutorialGuide] Call");
    }

    private void StartGuide()
    {
             
    }
}
