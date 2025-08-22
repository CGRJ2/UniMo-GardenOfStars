using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class WorkArea : InteractableBase
{
    // 작업영역에 두개 이상의 오브젝트가 위치한 상태에서, 처음 작업중이던 객체가 나갔을 때 정상작동하는지 테스트 필요

    [HideInInspector] public ManufactureBuilding instance;

    PlayerController curWorker; // 임시. 일꾼까지 포함한 변수로 수정 필요
    [SerializeField] Slider progressBar;

    public void Init(ManufactureBuilding instance)
    {
        this.instance = instance;
    }

    IEnumerator ProgressingTask()
    {
        curWorker = characterRuntimeData; // 임시. 일꾼까지 포함한 변수로 수정 필요
        while (characterRuntimeData != null)
        {
            // 쌓여있는 재료가 있을때만 실행
            if (instance.ingrediantStack.Count > 0)
            {
                instance.progressedTime += Time.deltaTime;

                if (instance.runtimeData.productionTime < instance.progressedTime)
                {
                    CompleteTask(); // 결과물 생성
                    instance.progressedTime = 0; // 진행도 초기화
                }
                
                // 진행도 게이지 업데이트
                progressBar.value = instance.progressedTime / instance.runtimeData.productionTime;

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
        //GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // 회수영역에 개수 늘려주기
        instance.prodsArea.prodsCount += 1;

        // 재료 소모
        instance.ingrediantStack.Pop().Despawn();
    }


    public override void EnterInteract(PlayerController characterRuntimeData)
    {
        base.EnterInteract(characterRuntimeData);
        //Debug.Log($"건물작업영역({buildingInstance.name}): 즉발형 상호작용 실행");

        StartCoroutine(ProgressingTask());
    }

    public override void ExitInteract(PlayerController characterRuntimeData)
    {
        base.ExitInteract(characterRuntimeData);
        //Debug.Log($"건물작업영역({buildingInstance.name}): 팝업형 상호작용 비활성화");
    }
}
