using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WorkerState_Move : WorkerStateBase
{
    private NavMeshAgent _navAgent;
    private bool _isArrive;
    private IWorkStation CurWorkstation => WorkerData.CurWorkstation.Value;

    public WorkerState_Move(StateMachine<WorkerStates> stateMachine, WorkerRuntimeData data) : base(stateMachine, data)
    {
        _navAgent = WorkerData.GetComponent<NavMeshAgent>();
        _navAgent.speed = WorkerData.MoveSpeed;
    }

    public override void Enter()
    {
        NavMeshHit hit;

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

        WorkerData.IsMove.Value = true;
    }

    public override void Update()
    {
        if (!CanWork())
        {
            StateMachine.ChangeState(WorkerStates.Idle);
        }
        else if (_navAgent.remainingDistance < 0.01f)
        {
            StateMachine.ChangeState(WorkerStates.Work);
        }
    }

    public override void Exit()
    {
        _navAgent.isStopped = true;
        WorkerData.IsMove.Value = false;
    }

    private bool CanWork()
    {
        if (!CurWorkstation.GetWorkableState()) return false;

        if(CurWorkstation is WorkArea)
        {
            if((CurWorkstation as WorkArea).curWorker != null
                && (CurWorkstation as WorkArea).curWorker != WorkerData)
            {
                return false;
            }
        }

        return true;
    }
}
