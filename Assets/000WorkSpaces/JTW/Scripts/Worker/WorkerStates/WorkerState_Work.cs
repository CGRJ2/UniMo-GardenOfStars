using System.Diagnostics;

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
        IWorkStation workstation = WorkerData.CurWorkstation.Value;

        if (!workstation.GetWorkableState()) return false;

        if (workstation is InsertArea)
        {
            if (WorkerData.IngrediantStack.Count == 0) return false;
        }
        else if (workstation is ProductGenerater || workstation is ProdsArea)
        {
            if (WorkerData.IngrediantStack.Count >= WorkerData.MaxCapacity) return false;
        }
        else if (workstation is WorkArea)
        {
            if ((workstation as WorkArea).curWorker != null
                && (workstation as WorkArea).curWorker != WorkerData)
            {
                return false;
            }
        }

        return true;
    }
}
