using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Interactable_Work : InteractableBase
{
    // 작업영역에 두개 이상의 오브젝트가 위치한 상태에서, 처음 작업중이던 객체가 나갔을 때 정상작동하는지 테스트 필요

    BuildingInstance_Work buildingInstance;
    PlayerController curWorker; // 임시. 일꾼까지 포함한 변수로 수정 필요
    [SerializeField] Slider progressBar;
    ObjectPool _Pool;

    public void Init(BuildingInstance_Work buildingInstance)
    {
        this.buildingInstance = buildingInstance;
        _Pool = Manager.pool.GetPoolBundle(buildingInstance.resultProdData.InstancePrefab).instancePool;
    }

    IEnumerator ProgressingTask()
    {
        curWorker = pc; // 임시. 일꾼까지 포함한 변수로 수정 필요
        while (pc != null)
        {
            // 쌓여있는 재료가 있을때만 실행
            if (buildingInstance.prodsStack.Count > 0)
            {
                buildingInstance.progressedTime += Time.deltaTime;

                if (buildingInstance.taskTime < buildingInstance.progressedTime)
                {
                    CompleteTask(); // 결과물 생성
                    buildingInstance.progressedTime = 0; // 진행도 초기화
                }
                
                // 진행도 게이지 업데이트
                progressBar.value = buildingInstance.progressedTime / buildingInstance.taskTime;

                yield return null;
            }
            else yield return null;
        }
        // 작업자 사라지면 curWorker 초기화
        curWorker = null;
    }

    public void CompleteTask()
    {
        // 결과물 인스턴스 생성(활성화) ----> 이거를 단일 개체에 쌓임 형태로 할건지 아직 미정임
        // 아예 생성결과물을 저장하는 영역에 데이터만 넣어주는 것도 괜찮을듯
        GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // 생산물 정보 저장
        //_SpawnedProduct = disposedObject.GetComponent<IngrediantInstance>();

        // 재료 소모
        buildingInstance.prodsStack.Pop().Despawn();
    }


    public override void EnterInteract_Player()
    {
        base.EnterInteract_Player();
        //Debug.Log($"건물작업영역({buildingInstance.name}): 즉발형 상호작용 실행");

        StartCoroutine(ProgressingTask());
    }

    public override void DeactiveInteract_Player()
    {
        base.DeactiveInteract_Player();
        //Debug.Log($"건물작업영역({buildingInstance.name}): 팝업형 상호작용 비활성화");
    }
}
