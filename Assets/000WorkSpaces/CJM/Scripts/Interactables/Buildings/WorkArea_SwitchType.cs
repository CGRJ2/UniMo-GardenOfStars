using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class WorkArea_SwitchType : InteractableBase, IWorkStation
{
    public bool isWorkable { get { return (ownerInstance.ingrediantStack.Count > 0); } }
    public bool isReserved;
    public bool GetWorkableState() { return isWorkable; }
    public bool GetReserveState() { return isReserved; }
    public void SetReserveState(bool reserve) { isReserved = reserve; }
    public Vector3 GetPosition() { return transform.position; }

    [HideInInspector] public ManufactureBuilding ownerInstance;

    [SerializeField] Slider taskProgressBar;
    [SerializeField] CircularProgressUI prepareProgressBar;

    // 소요 시간 = (produceTime * PrepareTime) / 작업 속도

    float curCharacterProdSpeed;

    float calculatedPrepareTime => (ownerInstance.prepareTime * ownerInstance.ProdTime) / curCharacterProdSpeed;
    float calculatedProduceTime => (ownerInstance.ProdTime * (1 - ownerInstance.prepareTime)) / curCharacterProdSpeed;


    float prepareProgressedTime = 0f;   // 준비 단계 진행도

    bool isOperating;

    // 현재 작업 중인 일꾼 정보
    public CharaterRuntimeData curWorker;

    GUID _Guid;

    public void Init(ManufactureBuilding instance)
    {
        this.ownerInstance = instance;
        Manager.buildings.workStatinLists.workAreas_SwitchType.Add(this);
        prepareProgressBar.gameObject.SetActive(false);
        taskProgressBar.gameObject.SetActive(false);

        _Guid = GUID.Generate();
    }

    // 작업 준비 단계
    IEnumerator PrepareTask()
    {
        isReserved = false;
        curWorker = characterRD;
        curWorker.IsWork.Value = true;
        curCharacterProdSpeed = characterRD.GetProductionSpeed();

        while (curWorker == personalTaskOwner) // 현재 작업자가 있는 동안 계속 실행
        {
            yield return null;

            // 작업 영역 밖으로 나가는 경우
            if (curWorker != personalTaskOwner) break;

            // "작업"이 진행중인 경우
            if (isOperating) 
            {
                prepareProgressedTime = 0; // 진행도 초기화
                prepareProgressBar.gameObject.SetActive(false);
                continue;
            }

            // 작업 준비 진행 중에, 영역 내에서 움직인 경우
            if (curWorker.IsMove.Value)
            {
                prepareProgressedTime = 0; // 진행도 초기화
                prepareProgressBar.gameObject.SetActive(false);
                continue;
            }

            // 재료 소진 시
            if (ownerInstance.ingrediantStack.Count <= 0)
            {
                prepareProgressedTime = 0; // 진행도 초기화
                prepareProgressBar.gameObject.SetActive(false);
                continue;
            }
            
            // 쌓여있는 재료가 있을때만 실행
            if (ownerInstance.ingrediantStack.Count > 0)
            {
                // 준비 시작 시, 진행도 표기
                prepareProgressBar.gameObject.SetActive(true);
                prepareProgressedTime += Time.deltaTime;

                if (calculatedPrepareTime < prepareProgressedTime)
                {
                    //CompleteTask(); // 결과물 생성
                    StartCoroutine(ProgressingTask());
                    isOperating = true;
                    prepareProgressedTime = 0; // 진행도 초기화
                    prepareProgressBar.gameObject.SetActive(false); // 진행도 표기 비활성화

                    continue;
                }

                // 진행도 게이지 업데이트
                prepareProgressBar.SetValue(prepareProgressedTime / calculatedPrepareTime);

                yield return null;
            }
            else yield return null;
        }

        yield return null;

        // 준비 작업 종료 시, 진행도 표기 비활성화
        prepareProgressBar.gameObject.SetActive(false);

        // 준비 작업 종료 시, 현재 작업자 정보 초기화
        if (curWorker != null)
        {
            curWorker.IsWork.Value = false;
            curWorker = null;
        }
    }
    IEnumerator ProgressingTask()
    {
        // 작업이 가동될 때 까지 대기
        yield return new WaitUntil(() => isOperating);

        // 작업 시작 시, 진행도 표기
        taskProgressBar.gameObject.SetActive(true);

        while (isOperating)
        {
            if (ownerInstance.progressedTime == 0)
            {
                // SFX 실행
                Manager.Audio.SfxPlayLoop(_Guid.ToString(), "SFX_ManufactureBuilding", transform);
            }

            ownerInstance.progressedTime += Time.deltaTime;

            if (calculatedProduceTime < ownerInstance.progressedTime)
            {
                CompleteTask(); // 결과물 생성
                ownerInstance.progressedTime = 0; // 진행도 초기화

                // SFX 끄기
                Manager.Audio.SfxStopLoop(_Guid.ToString(), 0.5f);

                isOperating = false; // 작업 처리 정지
            }

            // 진행도 게이지 업데이트
            taskProgressBar.value = ownerInstance.progressedTime / calculatedProduceTime;
            yield return null;
        }

        // 작업 완료 시, 진행도 표기 비활성화
        taskProgressBar.gameObject.SetActive(false);
    }

    public void CompleteTask()
    {
        // 결과물 인스턴스 생성(활성화) ----> 이거를 단일 개체에 쌓임 형태로 할건지 아직 미정임
        //GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // 회수영역에 개수 늘려주기
        ownerInstance.prodsArea.ProdsCount.Value += 1;

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

    protected override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        Manager.buildings?.workStatinLists.workAreas_SwitchType?.Remove(this);
    }
}
