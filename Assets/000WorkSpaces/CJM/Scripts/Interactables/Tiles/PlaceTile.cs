using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlaceTile : InteractableBase
{
    [SerializeField] CircularProgressUI progressBar;
    [SerializeField] CanvasGroup activatedView;
    [SerializeField] CanvasGroup deactivatedView;
    public string tileId => gameObject.name;
    float progressedTime;
    [SerializeField] Transform attachPoint;

    [Header("설치 시간")]
    [SerializeField] float installingTime;

    [Header("설치 가능 건물 제한")]
    [SerializeField] string buildableID;
    
    [Header("기본 건물 여부 (스테이지 첫 진입 시, 건물을 기본으로 설치해둘 것인지)")]
    [SerializeField] bool isDefaultBuilding;

    PlaceTileState state;

    // 건물 설치 FX 효과
    ObjectPool _Pool_FX_Construct;
    GameObject _FX_Construct;


    [HideInInspector] public PlaceTileGroup _parentGroup;

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

        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData != null);
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.IsInit);
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.PlaceTileList.IsInit);

        //yield return new WaitForSeconds(1f);
        Init();
    }

    public void Init()
    {
        Debug.Log("스테이지 데이터 생성 후 초기화");
        progressBar.gameObject.SetActive(false);

        // 초기화할 땐 비활성화 상태로
        state = PlaceTileState.Deactivated;
        UpdateViewByState();

        // 건설영역 타일 View 이벤트 연동
        Manager.buildings.BuildModEvent += OnBuildModChanged;

        // 데이터베이스에서 현재 스테이지 저장소에 본인이 있는지 체크, 없으면 데이터 생성
        PlaceTileData placeTileData = Manager.firebase.UserData.CurStageData.PlaceTileList.Get(tileId);
        if (placeTileData == null)
        {
            Manager.firebase.UserData.CurStageData.PlaceTileList.Add(tileId);

            if (isDefaultBuilding)
                StartCoroutine(DefaultBuildingFirstInit());
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
                    //ChangeState(PlaceTileState.Constructed); // `건설됨` 상태로 변경
                };
            }
        }

        // FX 불러온 후, 풀로 반환 (없으면 풀 생성)
        Addressables.LoadAssetAsync<GameObject>("FX/Constructing.prefab").Completed += task =>
        {
            _Pool_FX_Construct = Manager.pool.GetPoolBundle(task.Result, 1).instancePool;
        };
    }

    IEnumerator DefaultBuildingFirstInit()
    {
        // 첫 초기화인데, 기본 건물이라면.
        string curStageID = Manager.firebase.UserData.CurStage.Value;
        string[] buildingIDs = Manager.data.Stage.Values[curStageID].GetBuildingIdList();
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.PlaceTileList.Get(tileId) != null);

        // 해당 스테이지 DB에 첫번째 건물 넣어주기
        Manager.firebase.UserData.CurStageData.PlaceTileList.Get(tileId).BuildingID.Value = buildingIDs[0];

        // 인스턴스도 생성
        Addressables.LoadAssetAsync<GameObject>(buildingIDs[0]).Completed += task =>
        {
            GameObject buildingObject = Instantiate(task.Result, transform.position, transform.rotation);
        };
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

            // 재가동 시, 사운드 이펙트 실행
            if (progressedTime == 0)
            {
                Manager.Audio.SfxPlayLoop("Contruct", "SFX_ManufactureBuilding", transform);
                _FX_Construct = _Pool_FX_Construct.DisposePooledObj(transform.position, transform.rotation);
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
            progressBar.SetValue(progressedTime / installingTime);
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

            // 배치 완료 시,
            // 건물 배치 정보 DB에 업데이트
            // 스테이지 데이터 -> PlaceTile리스트 -> 현재 타일 id에 경로에 건물 id 저장
            PlaceTileData placeTileData = Manager.firebase.UserData.CurStageData.PlaceTileList.Get(tileId);
            placeTileData.BuildingID.Value = buildingItem.buildingId;

            // 구매한 건물 ID => DB에서 초기화
            Manager.firebase.UserData.CurStageData.PurchasedBuildingID.Value = "";

            // 건축모드 비활성화
            Manager.buildings.BuildModEvent?.Invoke(false, null);

            // 설치 SFX 종료
            Manager.Audio.SfxStopLoop("Contruct", 0.5f);

            // 설치 FX 효과 비활성화
            _Pool_FX_Construct.ReturnPooledObj(_FX_Construct);

        });


        // 튜토리얼 씬에서 설치 완료한 경우 
        if (Manager.firebase.UserData.CurStage.Value == "Tutorial")
        {
            TutorialManager.Instance.SequenceEnd(); // 시퀀스03 종료(저장)
        }
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

    private void OnDestroy()
    {
        if (Manager.buildings != null)
            Manager.buildings.BuildModEvent -= OnBuildModChanged;
    }

    public void OnBuildModChanged(bool isBuildMod, string buildingID)
    {
        if (isBuildMod)
        {
            // 해당 건물의 그룹이 언락된 상태가 아니라면 return
            if (!_parentGroup.IsUnlocked()) return;

            // 해당 건물이 타겟이 아니라면 return
            if (buildableID != buildingID) return;

            if (string.IsNullOrEmpty(GetBuildingID()))
                state = PlaceTileState.Activated;
            else
                state = PlaceTileState.Constructed;
        }
        else
        {
            state = PlaceTileState.Deactivated;
        }

        UpdateViewByState();
    }
    public void UpdateViewByState()
    {
        switch (state)
        {
            case PlaceTileState.Deactivated: // 건설모드가 아님
                activatedView.gameObject.SetActive(false);
                deactivatedView.gameObject.SetActive(false);
                break;

            case PlaceTileState.Activated: // 건설모드 On && 건설 가능 영역
                activatedView.gameObject.SetActive(true);
                deactivatedView.gameObject.SetActive(false);
                break;

            case PlaceTileState.Constructed: // 건설모드 On && 건설 불가능 영역
                activatedView.gameObject.SetActive(false);
                deactivatedView.gameObject.SetActive(true);
                break;

            default: break;
        }
    }
    string GetBuildingID()
    {
        PlaceTileData placeTileData = Manager.firebase.UserData.CurStageData.PlaceTileList.Get(tileId);
        string buildingID = placeTileData.BuildingID.Value;
        if (!string.IsNullOrEmpty(buildingID))
        {
            return buildingID;
        }
        else return null;
    }

}

public enum TileType
{
    All, Harvest, Manufacture
}

public enum PlaceTileState
{
    Deactivated, Activated, Constructed
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