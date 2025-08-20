using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="StageSo", menuName ="Stage/Number")]
public class StageData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string StageName; //�������� 
    public int StageId; //�������� ��ȣ.
    [Header("����Ʈ�� �޼���")]
    public int QuestRate; // ����Ʈ �޼���.
    [Header("�������� Ŭ���� ����")]
    public int condition; //�������� �н� ����
    [Header("Ŭ���� ����")]
    public bool Unlock; //�������� �н� ���ɿ���
    public void init()
    {
        //QuestRate = ����Ʈ ������� �޾ƿ�.
        //condition = �ش� ���������� �н� ������ �޾ƿ�.
        if(QuestRate > 6)//����Ʈ Ŭ������ ���Ǵ޼�.
        {
           Unlock = true;
        }
    }

    
}
