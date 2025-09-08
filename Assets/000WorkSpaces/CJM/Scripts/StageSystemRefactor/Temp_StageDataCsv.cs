using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageDataCSV", menuName = "Temp_StageDataCSV")]
public class Temp_StageDataCsv : ScriptableObject
{
    public List<StageData> stageDataColumns;
}