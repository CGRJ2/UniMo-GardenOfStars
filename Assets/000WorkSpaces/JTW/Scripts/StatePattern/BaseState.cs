using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// T에는 각 상태를 표현할 Enum 클래스를 넣어주면 됩니다.
public abstract class BaseState<T>
{
    protected StateMachine<T> StateMachine;
    public bool HasPhysics;

    public BaseState(StateMachine<T> stateMachine)
    {
        StateMachine = stateMachine;
    }

    public abstract void Enter();

    public abstract void Update();

    public virtual void FixedUpdate() { }

    public abstract void Exit();
}
