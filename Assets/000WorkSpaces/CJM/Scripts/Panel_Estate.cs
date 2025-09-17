using KYS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Panel_Estate : BaseUI
{
    [SerializeField] Dictionary<string, BuildingData> buildingDatas;

    protected override void Awake()
    {
        base.Awake();

        // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
        if (layerType == UILayerType.Panel) // BaseUI의 기본값
        {
            layerType = UILayerType.Panel;
        }

        Init();
    }

    public void Init()
    {
        // 현재 스테이지에 판매 중인 건물들만 불러와서 딕셔너리로 저장
        string curStageID = Manager.firebase.UserData.CurStage.Value;

        string buildingIDs = Manager.data.Stage.Values[curStageID].BuildingIDs;
        string[] buildingIdArray = 
            buildingIDs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

        buildingDatas = new();

        foreach (string id in buildingIdArray)
        {
            buildingDatas.Add(id, Manager.data.Building[id]); // 건물 데이터 추가
        }
    }
}
