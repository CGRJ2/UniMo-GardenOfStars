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

        if (WorkerData.CurWorkstation.Value == null) return;

        WorkerData.CurWorkstation.Value.SetReserveState(false);

        if (WorkerData.IsPlayerTriggered.Value)
        {
            StateMachine.ChangeState(WorkerStates.Idle);
            return;
        }

        WorkerData.IsPlayerTriggered.Subscribe(WakeUp);
    }

    public override void Update()
    {
        _timer += Time.deltaTime;

        if(_timer >= WorkerData.StunTime)
        {
            StateMachine.ChangeState(WorkerStates.Idle);
        }

        Debug.DrawRay(WorkerData.transform.position, Vector3.up * 10f, Color.red);
    }

    public override void Exit()
    {
        WorkerData.IsPlayerTriggered.Unsubscribe(WakeUp);
    }

    private void WakeUp(bool value)
    {
        if (!value) return;

        StateMachine.ChangeState(WorkerStates.Idle);
    }
}
