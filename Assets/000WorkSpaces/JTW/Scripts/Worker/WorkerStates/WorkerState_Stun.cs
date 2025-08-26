using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerState_Stun : WorkerStateBase
{
    private float _timer;

    public WorkerState_Stun(StateMachine<WorkerStates> stateMachine, WorkerRuntimeData data) : base(stateMachine, data)
    {
    }

    public override void Enter()
    {
        if (WorkerData.CurWorkstation.Value == null) return;

        WorkerData.CurWorkstation.Value.SetReserveState(false);
        if(WorkerData.CurWorkstation.Value is WorkArea)
        {
            (WorkerData.CurWorkstation.Value as WorkArea).curWorker = null;
        }
        WorkerData.CurWorkstation.Value = null;

        _timer = 0;
    }

    public override void Update()
    {
        _timer += Time.deltaTime;

        if(_timer >= WorkerData.StunTime)
        {
            StateMachine.ChangeState(WorkerStates.Idle);
        }
    }

    public override void Exit()
    {
    }
}
