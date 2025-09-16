using KYS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        Addressables.LoadAssetsAsync<BuildingData>("Data", null, true).Completed += task =>
        {
            foreach (BuildingData bd in task.Result)
            {
                buildingDatas.Add(bd.ID, bd); // 건물 데이터 추가
            }

            // 데이터베이스에서 유저가 보유중인 건물 => 업그레이드 딕셔너리 해당하는 애들만 체크
            //Manager.buildings.upgradeDataDic

        };
    }
}
