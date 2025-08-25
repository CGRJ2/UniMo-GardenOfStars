using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<bool> unlockStates = new List<bool>(); // 저장할 필요는 없을지도..?
    public List<int> questRates = new List<int>();
    public List<bool> stageClears = new List<bool>();

    //다른 저장값들 추가 .
}
