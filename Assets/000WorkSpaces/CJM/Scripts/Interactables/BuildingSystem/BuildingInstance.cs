using UnityEngine;

public class BuildingInstance : InteractableBase
{
    [SerializeField] protected BuildingData _OriginData;           // CSV or Sheet로 변경 예정
    [SerializeField] protected BuildingActivePopUI activatePopUI;

    protected void BIBaseInit()
    {
        if (_OriginData != null)
            Manager.buildings.AddBiTransformData(transform, _OriginData.ID);

        if (activatePopUI != null)
        {
            activatePopUI.GetComponent<Canvas>().worldCamera = Camera.main;
            activatePopUI.gameObject.SetActive(false);
        }
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();

        if (_OriginData != null)
            Manager.buildings?.RemoveBiTransformData(transform);

        if (activatePopUI != null)
            activatePopUI.gameObject.SetActive(false);
    }

    public void CheckUpgradable()
    {
        int curLevel_ProdTime = Manager.buildings.GetUpgradeData(_OriginData.ID).level_ProdTime;
        int curLevel_StackCount = Manager.buildings.GetUpgradeData(_OriginData.ID).level_Capacity;
        int curMoney = Manager.player.Data.Money.Value;

        // 생산형 건물일 때
        if (_OriginData is HarvestBD harvest)
        {
            bool upgradable = false;

            if (harvest.Stat_ProdTime.MaxLevel > curLevel_ProdTime)
            {
                if (curMoney > harvest.Stat_ProdTime.cost[curLevel_ProdTime])
                    upgradable = true;
            }

            if (upgradable)
            {
                // 상호작용 버튼을 업그레이드 모양으로 바꾸기
                activatePopUI.ActiveUpgradeBtnView();
            }
            else
            {
                // 상호작용 버튼을 정보 모양으로 바꾸기
                activatePopUI.ActiveInfoBtnView();
            }
        }
        else if (_OriginData is ManufactureBD mnfct)
        {
            bool upgradable = false;
            // 두 스탯 중 업그레이드 비용이 충족될 때
            if (mnfct.Stat_Capacity.MaxLevel > curLevel_StackCount)
            {
                if (curMoney > mnfct.Stat_Capacity.cost[curLevel_StackCount])
                    upgradable = true;
            }
            if (mnfct.Stat_ProdTime.MaxLevel > curLevel_ProdTime)
            {
                if (curMoney > mnfct.Stat_ProdTime.cost[curLevel_ProdTime])
                    upgradable = true;
            }

            if (upgradable)
            {
                // 상호작용 버튼을 업그레이드 모양으로 바꾸기
                activatePopUI.ActiveUpgradeBtnView();
            }
            else
            {
                // 상호작용 버튼을 정보 모양으로 바꾸기
                activatePopUI.ActiveInfoBtnView();
            }
        }
    }

    // 건물 활성화 범위 상호작용
    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);

        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            if (activatePopUI != null)
            {
                // for test
                if(_OriginData!=null)
                    CheckUpgradable();

                activatePopUI.gameObject.SetActive(true);  // 기본 상호작용 팝업 활성화 (존재 한다면)
            }
        }
    }

    // 건물 활성화 범위 상호작용
    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);

        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            if (activatePopUI != null)
                activatePopUI.gameObject.SetActive(false); // 기본 상호작용 팝업 비활성화 (존재 한다면)
        }
    }
}


