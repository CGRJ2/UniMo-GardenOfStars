using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="StageSo", menuName ="Stage/Number")]
public class StageData : ScriptableObject
{
    //public enum StageState
    //{
    //    locked, //아직 클리어 안함.
    //    Unlocked, //클리어함.
    //    Nextopen //다음스테이지 해금.
    //}
    [Header("기본 정보")]
    public string StageName; //스테이지 
    public int StageId; //스테이지 번호.
    [Header("퀘스트의 달성율")]
    public int QuestRate; // 퀘스트 달성율.
    [Header("스테이지 클리어 조건(값을 넣어 수정)")]
    public int condition; //스테이지 패스 조건
    //[Header("스테이지 상태")]
    //public StageState state; //스테이지 상태.
    [Header("스테이지 언락 여부")]
    public bool Unlock; //스테이지 패스 가능여부
    [Header("클리어 여부 넥스트스테이지 오픈관련")]
    public bool StageClear;
    public void init()
    {
        //QuestRate = 퀘스트 진행률을 받아옴.
        //condition = 해당 스테이지의 패스 조건을 받아옴.
        if(QuestRate >= condition)//퀘스트 클리어율 조건달성.
        {
           StageClear = true;
        }
    }
    public void init(int newProgress) // 진행도 갱신용
    {
        QuestRate = newProgress;
        StageClear = QuestRate >= condition;
    }



}
