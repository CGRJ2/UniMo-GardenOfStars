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
        _timer = 0;

        WorkerData.IsAwake.Value = false;
        WorkerData.IsStun.Value = true;

        if (WorkerData.CurWorkstation.Value == null) return;

        WorkerData.CurWorkstation.Value.SetReserveState(false);

        if (WorkerData.IsPlayerTriggered.Value)
        {
            StateMachine.ChangeState(WorkerStates.Idle);
            return;
        }

    }

    public override void Update()
    {
        if(WorkerData.IsPlayerTriggered.Value)
        {
            StateMachine.ChangeState(WorkerStates.Idle);
            return;
        }

        _timer += Time.deltaTime;

        if(_timer >= WorkerData.StunTime)
        {
            StateMachine.ChangeState(WorkerStates.Idle);
        }

        Debug.DrawRay(WorkerData.transform.position, Vector3.up * 10f, Color.red);
    }

    public override void Exit()
    {
        WorkerData.IsAwake.Value = true;
        WorkerData.IsStun.Value = false;
    }
}
