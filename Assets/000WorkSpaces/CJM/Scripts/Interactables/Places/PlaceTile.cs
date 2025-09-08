using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class PlaceTile : InteractableBase
{
    [SerializeField] Slider progressBar;
    string buildingId;

    [SerializeField] float installingTime;
    [SerializeField] float progressedTime;
    [SerializeField] Transform attachPoint;

    [Header("상태 디버그용")]
    [SerializeField] PlaceTileState state;
    public void Init()
    {
        progressBar.gameObject.SetActive(false);
    }

    IEnumerator ProgressingTask()
    {
        // 정지 상태까지 대기했다가 작업 실행
        yield return new WaitUntil(() => !characterRD.IsMove.Value);

        // 작업 시작 시, 진행도 표기
        progressBar.gameObject.SetActive(true);

        while (characterRD != null) // 영역 안에 있을 때 진행
        {
            // 작업 진행 중, 영역 내에서 움직인 경우 대기
            if (characterRD.IsMove.Value)
            {
                progressBar.gameObject.SetActive(false);
                progressedTime = 0; // 진행도 초기화
                yield return new WaitUntil(() => !characterRD.IsMove.Value);
                progressBar.gameObject.SetActive(true);
            }


            IngrediantInstance ownedBuilding = characterRD.IngrediantStack.Peek();
            // 손에 건물이 없을 시, continue
            if (ownedBuilding == null) { continue; } // 손에 든 재료가 없을 때
            else { if (!(ownedBuilding is Item_Building)) continue; } // <- 손에 든 재료가 건물이 아닐 때


            // 작업 영역 밖으로 나가는 경우
            if (characterRD == null)
            {
                progressedTime = 0; // 진행도 초기화
                break;
            }

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

            yield return null;
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
            buildingId = buildingItem.buildingId;
            ChangeState(PlaceTileState.Constructed);
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

        StartCoroutine(ProgressingTask());
    }
}

public enum PlaceTileState
{
    Deactivated, Activated, Constructed
}