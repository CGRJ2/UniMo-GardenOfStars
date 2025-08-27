using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageDataCSV", menuName = "Temp_StageDataCSV")]
public class Temp_StageDataCsv : ScriptableObject
{
    public List<StageData> stageDataColumns;
}

[Serializable]
public class StageData
{
    [Header("기본 정보")]
    public string StageName; //스테이지 
    public string StageId; //스테이지 번호.
    [Header("다음 스테이지 언락을 위한 필요 퀘스트 진행도")]
    public int _NextUlockQuestIndex; //스테이지 패스 조건
    public string _NextStageId; //스테이지 패스 조건
}