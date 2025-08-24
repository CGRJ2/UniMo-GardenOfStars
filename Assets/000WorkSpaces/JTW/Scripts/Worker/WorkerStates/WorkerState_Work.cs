public class WorkerState_Work : WorkerStateBase
{
    public WorkerState_Work(StateMachine<WorkerStates> stateMachine, WorkerRuntimeData data) : base(stateMachine, data)
    {
    }

    public override void Enter()
    {
    }

    public override void Update()
    {
        if (!CanWork())
        {
            StateMachine.ChangeState(WorkerStates.Idle);
        }
    }

    public override void Exit()
    {
    }

    private bool CanWork()
    {
        // TODO : 일이 가능한 상태인지 체크하는 bool 변수로 변경.
        if (!WorkerData.CurWorkstation.Value.gameObject.activeSelf) return false;

        if (WorkerData.CurWorkstation.Value is InsertArea)
        {
            if (WorkerData.IngrediantStack.Count == 0) return false;
        }

        return true;
    }
}
