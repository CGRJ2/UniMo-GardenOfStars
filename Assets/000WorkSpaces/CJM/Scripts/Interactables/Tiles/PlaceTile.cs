using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class PlaceTile : InteractableBase
{
    [SerializeField] Slider progressBar;
    public string tileId => gameObject.name;


    [SerializeField] float installingTime;
    [SerializeField] float progressedTime;
    [SerializeField] Transform attachPoint;

    [Header("상태 디버그용")]
    [SerializeField] PlaceTileState state;

    private void Awake()
    {
        // 타이틀에서 스테이지 씬으로 전환될 때 실행
        //Init();

        // 테스트용으로 바로 스테이지 씬에서 시작할 때 실행
        StartCoroutine(WaitAndInit());
    }

    IEnumerator WaitAndInit()
    {
        yield return new WaitUntil(() => Manager.firebase.IsFirebaseInit);
        yield return new WaitUntil(() => Manager.firebase.UserData != null);
        yield return new WaitUntil(() => Manager.firebase.UserData.IsInit);
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.IsInit);
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.PlaceTileList.IsInit);

        //yield return new WaitForSeconds(1f);
        Init();
    }

    public void Init()
    {
        Debug.Log("스테이지 데이터 생성 후 초기화");
        progressBar.gameObject.SetActive(false);

        // 데이터베이스에서 현재 스테이지 저장소에 본인이 있는지 체크, 없으면 데이터 생성
        PlaceTileData placeTileData = Manager.firebase.UserData.CurStageData.PlaceTileList.Get(tileId);
        if (placeTileData == null)
        {
            Manager.firebase.UserData.CurStageData.PlaceTileList.Add(tileId);
        }
        else
        {
            string buildingID = placeTileData.BuildingID.Value;

            // 건물 정보가 있는 타일이라면, 건물 인스턴스 생성해주기
            if (!string.IsNullOrEmpty(buildingID))
            {
                Addressables.LoadAssetAsync<GameObject>(buildingID).Completed += task =>
                {
                    GameObject buildingObject = Instantiate(task.Result, transform.position, transform.rotation);
                    ChangeState(PlaceTileState.Constructed); // `건설됨` 상태로 변경
                };
            }
        }
    }

    IEnumerator ProgressingTask()
    {
        while (characterRD != null) // 영역 안에 있을 때 진행
        {
            yield return null;

            // 작업 영역 밖으로 나가는 경우
            if (characterRD == null)
            {
                progressedTime = 0; // 진행도 초기화
                break;
            }

            // 작업 진행 중, 영역 내에서 움직인 경우 대기
            if (characterRD.IsMove.Value)
            {
                progressBar.gameObject.SetActive(false);
                progressedTime = 0; // 진행도 초기화
                continue;
            }

            IngrediantInstance ownedBuilding;
            characterRD.IngrediantStack.TryPeek(out ownedBuilding);
            // 손에 건물이 없을 시, continue
            if (ownedBuilding == null) { continue; } // 손에 든 재료가 없을 때
            else { if (!(ownedBuilding is Item_Building)) continue; } // <- 손에 든 재료가 건물이 아닐 때
            

            // 작업 시작 시, 진행도 표기
            progressBar.gameObject.SetActive(true);

            progressedTime += Time.deltaTime;

            // 설치가 완료된 경우
            if (installingTime < progressedTime)
            {
                CompleteTask(); // 결과물 생성
                progressedTime = 0; // 진행도 초기화
                break;
            }

            // 진행도 게이지 업데이트
            progressBar.value = progressedTime / installingTime;
        }

        // 진행도 표기 비활성화
        progressBar.gameObject.SetActive(false);
        yield return null;
    }

    public void CompleteTask()
    {
        // 건물(재료) 스택에서 빼서 넣어주기
        Item_Building buildingItem = characterRD.IngrediantStack.Pop() as Item_Building;
        buildingItem.MoveToTargetAndShrink(attachPoint,
            // 공터에 건물(재료)가 들어가며 소멸할 때, 인스턴스 생성
            () => Addressables.LoadAssetAsync<GameObject>(buildingItem.buildingId).Completed += task =>
        {
            GameObject buildingObject = Instantiate(task.Result, transform.position, transform.rotation);
            ChangeState(PlaceTileState.Constructed);

            // 배치 완료 시,
            // 건물 배치 정보 DB에 업데이트
            // 스테이지 데이터 -> PlaceTile리스트 -> 현재 타일 id에 경로에 건물 id 저장
            PlaceTileData placeTileData = Manager.firebase.UserData.CurStageData.PlaceTileList.Get(tileId);
            placeTileData.BuildingID.Value = buildingItem.buildingId;

            // 구매한 건물 ID => DB에서 초기화
            Manager.firebase.UserData.CurStageData.PurchasedBuildingID.Value = "";
        });
    }

    public void UpdateViewByState()
    {
        switch (state)
        {
            case PlaceTileState.Activated:
                // 발판 보여주기
                break;

            case PlaceTileState.Deactivated:
            case PlaceTileState.Constructed:
                // 발판 지우기
                break;

            default: break;
        }
    }

    public void ChangeState(PlaceTileState tileState)
    {
        state = tileState;
        UpdateViewByState();
    }

    public override void Enter_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter_PersonalTask(characterRuntimeData);

        if (state == PlaceTileState.Activated)
            StartCoroutine(ProgressingTask());
    }

    protected override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();

        StopAllCoroutines();
    }
}

public enum PlaceTileState
{
    Activated, Deactivated, Constructed
}

public class PlaceTileData : FirebaseData
{
    public FirebaseProperty<string> BuildingID;

    public PlaceTileData(string id, string parentPath = null) : base(id, parentPath)
    {
        BuildingID = new FirebaseProperty<string>("BuildingID", Path, "");
        InitList.Add(BuildingID);
    }
}