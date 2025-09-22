using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerState_Stun : WorkerStateBase
{
    private float _timer;
    private WaitForSeconds _delay = new WaitForSeconds(1f);

    public WorkerState_Stun(StateMachine<WorkerStates> stateMachine, WorkerRuntimeData data) : base(stateMachine, data)
    {
    }

    public override void Enter()
    {
        _timer = 0;

        Manager.Audio.SfxPlay("WorkerStun", WorkerData.transform);

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
        if (WorkerData.IsAwake.Value) return;

        if(WorkerData.IsPlayerTriggered.Value)
        {
            WorkerData.StartCoroutine(AwakeCoroutine());
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
        WorkerData.IsStun.Value = false;
    }

    private IEnumerator AwakeCoroutine()
    {
        WorkerData.IsAwake.Value = true;
        WorkerData.IsStun.Value = false;

        yield return _delay;

        WorkerData.IsAwake.Value = false;
        StateMachine.ChangeState(WorkerStates.Idle);
    }
}
