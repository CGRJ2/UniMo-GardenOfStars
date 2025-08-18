using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorkerStates
{
    Idle, Move, Work, Stun, Size
}

public class WorkerController : MonoBehaviour
{
    private InteractableBase _curWorkstation;
    private StateMachine<WorkerStates> _stateMachine;
    
    private WorkerData _workerData;
    public WorkerData WorkData => _workerData;

    public bool IsSetWorkstation => _curWorkstation != null;

    private void Awake()
    {
        // TODO : _stateMachine에 상태 추가 및 기본 상태로 설정.
    }

    private void Update()
    {
        _stateMachine.Update();
    }

    private void FixedUpdate()
    {
        _stateMachine.FixedUpdate();
    }

    public void SetWorkerData(WorkerData data)
    {
        _workerData = data;
    }

    public void SetWorkstation(InteractableBase workstation)
    {
        _curWorkstation = workstation;
    }
}
