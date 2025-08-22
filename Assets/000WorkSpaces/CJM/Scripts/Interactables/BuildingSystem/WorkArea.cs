using System.Collections;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.UI;

public class WorkArea : InteractableBase
{
    public bool isWorkable { get { return curWorker == null & instance.ingrediantStack.Count > 0; } }

    [HideInInspector] public ManufactureBuilding instance;

    CharaterRuntimeData curWorker; // 임시. 일꾼까지 포함한 변수로 수정 필요
    [SerializeField] Slider progressBar;

    public void Init(ManufactureBuilding instance)
    {
        this.instance = instance;
    }

    IEnumerator ProgressingTask()
    {
        curWorker = characterRD; // 임시. 일꾼까지 포함한 변수로 수정 필요

        // 정지 상태까지 대기했다가 작업 실행
        yield return new WaitUntil(() => !curWorker.IsMove.Value);

        while (!curWorker.IsMove.Value) // 정지 상태일 동안은 지속
        {
            // 작업 영역 밖으로 나가는 경우
            if (curWorker != characterRD) break;

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

        curWorker = null;
    }

    public void CompleteTask()
    {
        // 결과물 인스턴스 생성(활성화) ----> 이거를 단일 개체에 쌓임 형태로 할건지 아직 미정임
        //GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // 회수영역에 개수 늘려주기
        instance.prodsArea.ProdsCount += 1;

        // 재료 소모
        instance.ingrediantStack.Pop().Despawn();
    }


    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);
        //Debug.Log($"건물작업영역({buildingInstance.name}): 즉발형 상호작용 실행");
        if (curWorker == null)
        {
            StartCoroutine(ProgressingTask());
        }
    }

    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);
        //Debug.Log($"건물작업영역({buildingInstance.name}): 팝업형 상호작용 비활성화");
    }
}
