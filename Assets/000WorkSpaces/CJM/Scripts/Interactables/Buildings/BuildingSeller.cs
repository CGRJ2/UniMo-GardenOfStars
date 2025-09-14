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

        if (Manager.firebase.UserData.CurStage.Value == "Tutorial")
        {
            HideWaitingTile();
        }
    }
    


    // 테스트용 코드
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            SpawnBuildingItem("b10011");
        }
    }

    public bool IsOnHand()
    {
        // 손 스택 먼저 체크
        if (characterRD.IngrediantStack.Count > 0)
        {
            // 손에 뭐가 있으면 구매 불가 판정
            Debug.LogWarning("손에 이미 물건이 있어서 구매 못함");
            return true;
        }
        else return false;
    }

    // 구매 확정 후 건물(재료) 인스턴스 생성하기
    public void SpawnBuildingItem(string buildingId)
    {
        // 건축모드 활성화
        Manager.buildings.BuildModEvent?.Invoke(true);

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

        // 튜토리얼 씬에서 구매한 경우 (구매버튼을 눌러 건물(재료)가 나온 시점)
        if (Manager.firebase.UserData.CurStage.Value == "Tutorial")
        {
            TutorialManager.Instance.SequenceEnd(); // 시퀀스02 종료(저장)

            // 부동산 상호작용 발판 제거
            HideWaitingTile();
        }
    }


    public void ShowWaitingTile()
    {
        interactTile.gameObject.SetActive(true);
    }
    public void HideWaitingTile()
    {
        interactTile.gameObject.SetActive(false);
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


    private bool isTutorialPopDone;
    // 접근 시 튜토리얼 팝업 메세지(Squence02)
    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);
        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            if (Manager.firebase.UserData.CurStage.Value == "Tutorial")
            {
                if (TutorialManager.Instance.Sequence.Value != 2) return; // 튜토 진행도는 Firebase에서 관리. 추후에 수정해야됨

                // 1회만 나오도록 막아주는 용도
                if (isTutorialPopDone) return;
                isTutorialPopDone = true;

                // 플레이어 조작 막기
                Manager.player.IsControl = false;

                Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_questSquence02-2", () =>
                {
                    Debug.LogWarning("팝업 닫음 콜백 함수 실행");

                    // 플레이어 조작 활성화
                    Manager.player.IsControl = true;
                }, (msg) =>
                {
                    // 부동산 상호작용 발판 활성화
                    Manager.buildings.buildingSeller.ShowWaitingTile();
                });

                return;
            }
        }
    }

    // 건물 활성화 범위 상호작용
    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);
    }
}
