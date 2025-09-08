using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum WorkerStates
{
    Idle, Move, Work, Stun, Size
}

public class WorkerController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

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

        if (_text != null)
        {
            string text = _stateMachine.CurStateEnum.ToString();

            if (text == "Stun")
            {
                _text.color = Color.red;
            }
            else
            {
                _text.color = Color.black;
            }

            _text.text = text;
        }
    }

    private void FixedUpdate()
    {
        _stateMachine.FixedUpdate();
    }

    public WorkerStates GetCurState()
    {
        return _stateMachine.CurStateEnum;
    }

    public void Stun()
    {
        _stateMachine.ChangeState(WorkerStates.Stun);
    }
}
