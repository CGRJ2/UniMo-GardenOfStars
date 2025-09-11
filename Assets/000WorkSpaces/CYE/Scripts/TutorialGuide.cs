using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGuide : MonoBehaviour
{
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
        // 카메라 무빙 => 플레이어 포커스        
    }

}
