using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorkArea_SwitchType : InteractableBase, IWorkStation
{
    public bool isWorkable { get { return (curWorker == null & ownerInstance.ingrediantStack.Count > 0 && !isOperating); } }
    public bool isReserved;
    public bool GetWorkableState() { return isWorkable; }
    public bool GetReserveState() { return isReserved; }
    public void SetReserveState(bool reserve) { isReserved = reserve; }
    public Vector3 GetPosition() { return transform.position; }

    [HideInInspector] public ManufactureBuilding ownerInstance;

    CharaterRuntimeData curWorker; // 임시. 일꾼까지 포함한 변수로 수정 필요
    [SerializeField] Slider progressBar;
    [SerializeField] Slider temp_PrepareBar;
    [Header("작업을 활성화시키는 시간")]
    [SerializeField] float prepareTime = 3f;
    float prepareProgressedTime = 0f;   // 준비 단계 진행도


    [SerializeField] bool isOperating;

    

    public void Init(ManufactureBuilding instance)
    {
        this.ownerInstance = instance;
        Manager.buildings.workStatinLists.workAreas_SwitchType.Add(this);
        temp_PrepareBar.gameObject.SetActive(false);
        progressBar.gameObject.SetActive(false);
    }

    // 작업 준비 단계
    IEnumerator PrepareTask()
    {
        curWorker = characterRD; // 임시. 일꾼까지 포함한 변수로 수정 필요
        curWorker.IsWork.Value = true;

        // 정지 상태 && 기존작업 완료 상태까지 대기했다가 작업 실행
        yield return new WaitUntil(() => !curWorker.IsMove.Value && !isOperating);

        while (curWorker == characterRD) // 현재 작업자가 있는 동안 계속 실행
        {
            yield return new WaitUntil(() => !isOperating);
            
            // 준비 시작 시, 진행도 표기
            temp_PrepareBar.gameObject.SetActive(true);

            // 작업 진행 중, 영역 내에서 움직인 경우 대기
            if (curWorker.IsMove.Value)
            {
                prepareProgressedTime = 0; // 진행도 초기화
                temp_PrepareBar.gameObject.SetActive(false);
                yield return new WaitUntil(() => !curWorker.IsMove.Value);
                temp_PrepareBar.gameObject.SetActive(true);
            }

            // 재료 소진 시, 재료가 채워질 때 까지 대기
            if (ownerInstance.ingrediantStack.Count <= 0)
                yield return new WaitUntil(() => ownerInstance.ingrediantStack.Count > 0);

            // 작업 영역 밖으로 나가는 경우
            if (curWorker != characterRD) break;

            // 쌓여있는 재료가 있을때만 실행
            if (ownerInstance.ingrediantStack.Count > 0)
            {
                prepareProgressedTime += Time.deltaTime;

                if (prepareTime < prepareProgressedTime)
                {
                    //CompleteTask(); // 결과물 생성
                    StartCoroutine(ProgressingTask());
                    isOperating = true;
                    prepareProgressedTime = 0; // 진행도 초기화
                    temp_PrepareBar.gameObject.SetActive(false); // 진행도 표기 비활성화
                    continue;
                }

                // 진행도 게이지 업데이트
                temp_PrepareBar.value = prepareProgressedTime / prepareTime;

                yield return null;
            }
            else yield return null;
        }

        // 작업 종료 시, 현재 작업자 정보 초기화
        yield return null;
        curWorker.IsWork.Value = false;
        curWorker = null;

        // 준비 작업 종료 시, 진행도 표기 비활성화
        temp_PrepareBar.gameObject.SetActive(false);
    }
    IEnumerator ProgressingTask()
    {
        // 작업이 가동될 때 까지 대기
        yield return new WaitUntil(() => isOperating);

        // 작업 시작 시, 진행도 표기
        progressBar.gameObject.SetActive(true);

        while (isOperating)
        {
            ownerInstance.progressedTime += Time.deltaTime;

            if (ownerInstance.runtimeData.productionTime < ownerInstance.progressedTime)
            {
                CompleteTask(); // 결과물 생성
                ownerInstance.progressedTime = 0; // 진행도 초기화
                isOperating = false; // 작업 처리 정지
            }

            // 진행도 게이지 업데이트
            progressBar.value = ownerInstance.progressedTime / ownerInstance.runtimeData.productionTime;
            yield return null;
        }

        // 작업 완료 시, 진행도 표기 비활성화
        progressBar.gameObject.SetActive(false);
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
            StartCoroutine(PrepareTask());
        }
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        Manager.buildings?.workStatinLists.workAreas_SwitchType?.Remove(this);
    }
}
