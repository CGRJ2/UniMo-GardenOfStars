using KYS;
using UnityEngine;
using UnityEngine.AddressableAssets;


public class BuildingSeller : InteractableBase
{
    ObjectPool _Pool;
    [SerializeField] WaitingTile interactTile;
    private void Awake()
    {
        Manager.buildings.buildingSeller = this;

        interactTile.WaitingCompletedAction = OpenEstatePanel;
        interactTile.Init();
    }


    // 테스트용 코드
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            SpawnBuildingItem("b10011");
        }
    }

    // 건물(재료) 생성하기
    public void SpawnBuildingItem(string buildingId)
    {
        if (characterRD.IngrediantStack.Count > 0)
        {
            // 손에 뭐가 있으면 구매 불가 판정
            Debug.LogWarning("손에 이미 물건이 있어서 구매 못함");
            return;
        }

        // 모델 생성 (메쉬&매터리얼만 교체하는 방법으로 바꿔야함)
        Addressables.LoadAssetAsync<GameObject>($"it_{buildingId}").Completed += task =>
        {
            GameObject product = task.Result;
            _Pool = Manager.pool.GetPoolBundle(product, 3).instancePool;   // 해당 건물(재료) 인스턴스 풀 생성

            // 오브젝트 풀에서 활성화
            GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);
            Item_Building buildingItem = disposedObject.GetComponent<Item_Building>();

            buildingItem.buildingId = buildingId;
            buildingItem.AttachToTarget(characterRD.ProdsAttachPoint);
            characterRD.IngrediantStack.Push(buildingItem);

            // 구매한 건물 ID => DB에 갱신
            Manager.firebase.UserData.CurStageData.PurchasedBuildingID.Value = buildingId;
        };
    }

    // 부동산 패널 열기
    public void OpenEstatePanel()
    {
        Debug.Log("부동산 패널 열기");

        if (UIManager.Instance == null)
        {
            Debug.LogError("[부동산 패널] UIManager.Instance가 null입니다!");
            return;
        }

        // 이미 부동산 패널이 열려있는지 확인
        var existingPanels = Manager.ui.GetUIsByLayer(UILayerType.Panel);
        foreach (var panel in existingPanels)
        {
            if (panel is PropertyPanel)
            {
                //Debug.Log("[부동산 패널] 이미 부동산 패널이 열려있습니다. 중복 호출 무시");
                return;
            }
        }

        // 부동산 패널 열기
        Manager.ui.ShowPanelAsync<PropertyPanel>((panel) =>
        {
            if (panel != null)
            {
                //Debug.Log("[부동산 패널] 부동산 패널 성공적으로 열림");

                /*if (buildingInstance is HarvestBuilding harvesst)
                    panel.SetUpgradeData(harvesst.originData);*/
            }
            else
            {
                //Debug.LogError("[부동산 패널]  열기 실패");
            }
        });
    }


    // 건물 활성화 범위 상호작용
    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);
    }

    // 건물 활성화 범위 상호작용
    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);
    }
}
