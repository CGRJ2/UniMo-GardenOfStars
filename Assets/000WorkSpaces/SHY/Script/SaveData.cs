using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<bool> unlockStates = new List<bool>(); // ������ �ʿ�� ��������..?
    public List<int> questRates = new List<int>();
    public List<bool> stageClears = new List<bool>();

    //�ٸ� ���尪�� �߰� .
}
