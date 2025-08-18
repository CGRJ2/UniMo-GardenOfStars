using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// T에는 각 상태를 표현할 Enum 클래스를 넣어주면 됩니다.
public class StateMachine<T>
{
    public BaseState<T> CurState;
    private Dictionary<T, BaseState<T>> _stateDict = new Dictionary<T, BaseState<T>>();

    public void ChangeState(T changedStateEnum)
    {
        if (!_stateDict.ContainsKey(changedStateEnum))
        {
            Debug.LogError($"{changedStateEnum} 상태가 등록되지 않았습니다.");
            return;
        }

        BaseState<T> changedState = _stateDict[changedStateEnum];

        if (CurState == changedState) return;

        // 처음에 없을 수도 있으니 null 체크
        CurState?.Exit();
        CurState = changedState;
        CurState.Enter();
    }

    public void Update() => CurState.Update();

    public void FixedUpdate()
    {
        if (CurState.HasPhysics)
        {
            CurState.FixedUpdate();
        }
    }

    public void AddState(T stateEnum, BaseState<T> state)
    {
        _stateDict.TryAdd(stateEnum, state);
    }
}
