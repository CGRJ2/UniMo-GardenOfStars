using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorkerStateBase : BaseState<WorkerStates>
{
    protected WorkerRuntimeData WorkerData { get; private set; }

    public WorkerStateBase(StateMachine<WorkerStates> stateMachine, WorkerRuntimeData data) : base(stateMachine)
    {
        WorkerData = data;
    }
}
