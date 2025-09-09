using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WorkerState_Move : WorkerStateBase
{
    private NavMeshAgent _navAgent;
    private IWorkStation CurWorkstation => WorkerData.CurWorkstation.Value;

    private Coroutine tryCoroutine;

    private bool _isRePath;

    public WorkerState_Move(StateMachine<WorkerStates> stateMachine, WorkerRuntimeData data) : base(stateMachine, data)
    {
        _navAgent = WorkerData.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        NavMeshHit hit;

        _navAgent.speed = WorkerData.MoveSpeed;

        // 목적지 중에 ProductGenerater 같은 경우 영역 중간이 막혀있을 것,
        // 따라서 중간에서 가장 가까운 갈 수 있는 위치를 지정해준다.
        if (NavMesh.SamplePosition(CurWorkstation.GetPosition(), out hit, 3f, NavMesh.AllAreas))
        {
            _navAgent.SetDestination(hit.position);
        }
        else
        {
            Debug.Log($"[NavMesh]{WorkerData.gameObject.name}이 적절한 목적지를 찾지 못하였습니다.");
        }

        _navAgent.isStopped = false;

        _navAgent.avoidancePriority = 10 + WorkerData.NavMeshPriority;

        WorkerData.IsMove.Value = true;

        tryCoroutine = WorkerData.StartCoroutine(TryDetour());
    }

    public override void Update()
    {
        if (!CanWork())
        {
            StateMachine.ChangeState(WorkerStates.Idle);
        }
        else if (_navAgent.remainingDistance < 0.01f && !_isRePath)
        {
            StateMachine.ChangeState(WorkerStates.Work);
        }
    }

    public override void Exit()
    {
        _navAgent.isStopped = true;
        _navAgent.avoidancePriority = 30;
        WorkerData.CurWorkstation.Value.SetReserveState(false);
        WorkerData.IsMove.Value = false;
        WorkerData.StopCoroutine(tryCoroutine);
    }

    private bool CanWork()
    {
        if (!CurWorkstation.GetWorkableState()) return false;

        if (CurWorkstation is WorkArea_SwitchType)
        {
            if ((CurWorkstation as WorkArea_SwitchType).curWorker != null
                && (CurWorkstation as WorkArea_SwitchType).curWorker != WorkerData)
            {
                return false;
            }
        }
        else if (CurWorkstation is ProdsArea || CurWorkstation is ProductGenerater)
        {
            if (WorkerData.IngrediantStack.Count >= WorkerData.MaxCapacity)
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator TryDetour()
    {
        WaitForSeconds _delay = new WaitForSeconds(2f);

        while (true)
        {
            yield return _delay;

            yield return new WaitUntil(() => _navAgent.velocity.magnitude < 0.01f);

            Debug.Log("TryDetour");
            _isRePath = true;

            Vector3 destination = _navAgent.destination;

            _navAgent.ResetPath();

            Vector3 offset = Random.insideUnitSphere * 2.0f;
            offset.y = 0;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(WorkerData.transform.position + offset, out hit, 3f, NavMesh.AllAreas))
            {
                _navAgent.SetDestination(hit.position);
            }

            yield return new WaitForSeconds(0.5f);

            _navAgent.SetDestination(destination);
            _isRePath = false;
        }

        
    }
}
