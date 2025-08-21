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
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
    }
}
