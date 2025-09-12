using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class TutorialManager : Singleton<TutorialManager>
{
    public int Sequence = 0;
    [SerializeField] private CinemachineBrain _cinemachineBrain;

    private void Start()
    {
        StartCoroutine(Init());
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
