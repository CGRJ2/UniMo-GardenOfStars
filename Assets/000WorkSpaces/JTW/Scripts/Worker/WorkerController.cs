using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorkerStates
{
    Idle, Move, Work, Stun, Size
}

public class WorkerController : MonoBehaviour
{
    private StateMachine<WorkerStates> _stateMachine;

    private void Awake()
    {
        
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
