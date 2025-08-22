using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerState_Move : WorkerStateBase
{
    private bool _isArrive;

    public WorkerState_Move(StateMachine<WorkerStates> stateMachine, WorkerRuntimeData data) : base(stateMachine, data)
    {
    }

    public override void Enter()
    {
        // TODO : 네브메쉬를 이용하여 목적지로 이동을 시작하는 함수 추가.
        WorkerData.IsMove.Value = true;
    }

    public override void Update()
    {
        // TODO : 일이 가능한 상태인지 체크하는 bool 변수로 변경.
        if (!WorkerData.CurWorkstation.Value.gameObject.activeSelf)
        {
            StateMachine.ChangeState(WorkerStates.Idle);
        }
        // TODO : NevMesh를 이용하여 목적지에 도달했을 때.
        else if (_isArrive)
        {
            StateMachine.ChangeState(WorkerStates.Work);
        }
    }

    public override void Exit()
    {
        WorkerData.IsMove.Value = false;
    }

}
