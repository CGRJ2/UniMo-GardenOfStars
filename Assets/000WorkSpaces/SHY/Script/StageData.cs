using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="StageSo", menuName ="Stage/Number")]
public class StageData : ScriptableObject
{
    //public enum StageState
    //{
    //    locked, //���� Ŭ���� ����.
    //    Unlocked, //Ŭ������.
    //    Nextopen //������������ �ر�.
    //}
    [Header("�⺻ ����")]
    public string StageName; //�������� 
    public int StageId; //�������� ��ȣ.
    [Header("����Ʈ�� �޼���")]
    public int QuestRate; // ����Ʈ �޼���.
    [Header("�������� Ŭ���� ����(���� �־� ����)")]
    public int condition; //�������� �н� ����
    //[Header("�������� ����")]
    //public StageState state; //�������� ����.
    [Header("�������� ��� ����")]
    public bool Unlock; //�������� �н� ���ɿ���
    [Header("Ŭ���� ���� �ؽ�Ʈ�������� ���°���")]
    public bool StageClear;
    public void init()
    {
        //QuestRate = ����Ʈ ������� �޾ƿ�.
        //condition = �ش� ���������� �н� ������ �޾ƿ�.
        if(QuestRate >= condition)//����Ʈ Ŭ������ ���Ǵ޼�.
        {
           StageClear = true;
        }
    }
    public void init(int newProgress) // ���൵ ���ſ�
    {
        QuestRate = newProgress;
        StageClear = QuestRate >= condition;
    }



}
