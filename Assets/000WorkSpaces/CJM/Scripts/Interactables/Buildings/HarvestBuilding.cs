using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class HarvestBuilding : BuildingInstance
{
    [SerializeField] Transform prodsParentTransform;
    [HideInInspector] public HarvestBD originData;

    ProductGenerater[] productGeneraters;
    ObjectPool _Pool;

    private void Awake() => Init();
    
    public override void Init()
    {
        base.Init();

        if (Manager.data.Building.Values[ID] is HarvestBD harvestBD) originData = harvestBD;

        activatePopUI.Init(this);

        productGeneraters = prodsParentTransform.GetComponentsInChildren<ProductGenerater>();
        SetIngrediantToGeneraters();
    }

    public void SetIngrediantToGeneraters()
    {
        string productId = originData.ProductID;
        Addressables.LoadAssetAsync<GameObject>(productId).Completed += task =>
        {
            GameObject product = task.Result;

            _Pool = Manager.pool.GetPoolBundle(product).instancePool;
            foreach (ProductGenerater prodsGenerater in productGeneraters)
            {
                prodsGenerater.Init(product, originData);
            }
        };
    }

    public override void Enter_PersonalTask(CharaterRuntimeData singleInteracter)
    {
        base.Enter_PersonalTask(singleInteracter);

        // 리팩토링 필요 => 전부 TutorialManager에서 처리할 수 있도록

        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (singleInteracter is PlayerRunTimeData)
        {
            // 튜토리얼 NPC면 바로 첫대화 진행
            if (Manager.firebase.UserData.CurStage.Value == "Tutorial")
            {
                if (Manager.firebase.UserData.TutorialSequence.Value != 1) return; // 튜토 진행도는 Firebase에서 관리. 추후에 수정해야됨

                // 퀘스트 발판 활성화
                TutorialManager.Instance.tutorialNPC.UpdateQuestData();

                // 화살표 활성화
                TutorialManager.Instance.arrows[1].SetActive(false);
                TutorialManager.Instance.arrows[2].SetActive(true);

                // 강조효과 추가
                TutorialManager.Instance.PlayHighLightFX(TutorialManager.Instance.tutorialNPC.transform, true);
            }
        }
    }
}
