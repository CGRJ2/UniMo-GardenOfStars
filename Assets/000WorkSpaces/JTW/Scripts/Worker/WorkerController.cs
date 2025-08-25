using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorkerStates
{
    Idle, Move, Work, Stun, Size
}

public class WorkerController : MonoBehaviour
{
    private StateMachine<WorkerStates> _stateMachine = new StateMachine<WorkerStates>();

    private void Awake()
    {
        WorkerRuntimeData data = GetComponent<WorkerRuntimeData>();

        _stateMachine.AddState(WorkerStates.Idle, new WorkerState_Idle(_stateMachine, data));
        _stateMachine.AddState(WorkerStates.Move, new WorkerState_Move(_stateMachine, data));
        _stateMachine.AddState(WorkerStates.Work, new WorkerState_Work(_stateMachine, data));
        _stateMachine.AddState(WorkerStates.Stun, new WorkerState_Stun(_stateMachine, data));

        _stateMachine.ChangeState(WorkerStates.Idle);
    }

    private void Update()
    {
        _stateMachine.Update();
    }

    private void FixedUpdate()
    {
        _stateMachine.FixedUpdate();
    }
}
