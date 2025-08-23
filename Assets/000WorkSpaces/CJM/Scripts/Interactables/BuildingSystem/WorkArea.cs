using System.Collections;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.UI;

public class WorkArea : InteractableBase
{
    public bool isWorkable { get { return curWorker == null & ownerInstance.ingrediantStack.Count > 0; } }

    [HideInInspector] public ManufactureBuilding ownerInstance;

    CharaterRuntimeData curWorker; // 임시. 일꾼까지 포함한 변수로 수정 필요
    [SerializeField] Slider progressBar;

    public void Init(ManufactureBuilding instance)
    {
        this.ownerInstance = instance;
        Manager.buildings.workStatinLists.workAreas.Add(this);
    }

    IEnumerator ProgressingTask()
    {
        curWorker = characterRD; // 임시. 일꾼까지 포함한 변수로 수정 필요
        curWorker.IsWork.Value = true;

        // 정지 상태까지 대기했다가 작업 실행
        yield return new WaitUntil(() => !curWorker.IsMove.Value);

        while (curWorker == characterRD) // 현재 작업자가 있는 동안 계속 실행
        {
            // 작업 진행 중, 영역 내에서 움직인 경우 대기
            if (curWorker.IsMove.Value)
                yield return new WaitUntil(() => !curWorker.IsMove.Value);
            
            // 재료 소진 시, 재료가 채워질 때 까지 대기
            if (ownerInstance.ingrediantStack.Count <= 0)
                yield return new WaitUntil(() => ownerInstance.ingrediantStack.Count > 0);

            // 작업 영역 밖으로 나가는 경우
            if (curWorker != characterRD) break;

            // 쌓여있는 재료가 있을때만 실행
            if (ownerInstance.ingrediantStack.Count > 0)
            {
                ownerInstance.progressedTime += Time.deltaTime;

                if (ownerInstance.runtimeData.productionTime < ownerInstance.progressedTime)
                {
                    CompleteTask(); // 결과물 생성
                    ownerInstance.progressedTime = 0; // 진행도 초기화
                }
                
                // 진행도 게이지 업데이트
                progressBar.value = ownerInstance.progressedTime / ownerInstance.runtimeData.productionTime;

                yield return null;
            }
            else yield return null;
        }

        // 작업 종료 시, 현재 작업자 정보 초기화
        yield return null;
        curWorker.IsWork.Value = false;
        curWorker = null;
    }

    public void CompleteTask()
    {
        // 결과물 인스턴스 생성(활성화) ----> 이거를 단일 개체에 쌓임 형태로 할건지 아직 미정임
        //GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // 회수영역에 개수 늘려주기
        ownerInstance.prodsArea.ProdsCount += 1;

        // 재료 소모
        ownerInstance.ingrediantStack.Pop().Despawn();
    }

    public override void Enter_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter_PersonalTask(characterRuntimeData);

        if (curWorker == null)
        {
            StartCoroutine(ProgressingTask());
        }
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        Manager.buildings?.workStatinLists.workAreas?.Remove(this);
    }
}
