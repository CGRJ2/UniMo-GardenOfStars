using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveChack : MonoBehaviour
{
    public GameObject[] active;

    public void chack()
    {
        List<bool> get = StageManager.instance.GetUnlockStates();
        for (int i = 0; i < active.Length; i++)
        {
            active[i].SetActive(get[i]);
        }
    }
}
