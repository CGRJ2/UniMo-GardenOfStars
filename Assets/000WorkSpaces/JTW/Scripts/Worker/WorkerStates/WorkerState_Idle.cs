using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerState_Idle : WorkerStateBase
{
    public WorkerState_Idle(StateMachine<WorkerStates> stateMachine, WorkerRuntimeData data) 
        : base(stateMachine, data)
    {
    }
    public override void Enter()
    {
        WorkerData.CurWorkstation.Value = null;
    }

    public override void Update()
    {
        if(WorkerData.CurWorkstation.Value != null)
        {
            StateMachine.ChangeState(WorkerStates.Move);
        }
    }

    public override void Exit()
    {
    }
}
