using System.Threading;
using System.Threading.Tasks;

namespace Content.Server._DV.NPC.Tasks;

public sealed partial class NuParallelTask : NuHTNTaskBase<NuParallelTask, NuParallelPlan, NuParallelSystem>
{
    [DataField]
    public List<NuHTNTask> Tasks;
}

public sealed partial class NuParallelPlan : NuHTNPlanBase<NuParallelPlan, NuParallelTask, NuParallelSystem>
{
    [DataField]
    public List<NuHTNPlan> Plans;
}

public sealed class NuParallelSystem : NuHTNTaskSystem<NuParallelSystem, NuParallelTask, NuParallelPlan>
{
    public override async Task<NuParallelPlan?> Plan(Entity<NuHTNComponent> self, NuParallelTask task, CancellationToken token)
    {
        var plans = new List<NuHTNPlan>();

        foreach (var subtask in task.Tasks)
        {
            if (await subtask.Plan(self, HTN, token) is not { } subplan)
                return null;

            plans.Add(subplan);
        }

        return new NuParallelPlan()
        {
            Task = task,
            Plans = plans,
        };
    }

    public override void Start(Entity<NuHTNComponent> self, NuParallelPlan plan)
    {
        foreach (var subplan in plan.Plans)
        {
            subplan.Start(self, HTN);
        }
    }

    public override NuHTNTaskResult Update(Entity<NuHTNComponent> self, NuParallelPlan plan, float frameTime)
    {
        foreach (var subplan in plan.Plans)
        {
            var subResult = subplan.Update(self, HTN, frameTime);
            if (subResult == NuHTNTaskResult.ContinueTask)
                continue;

            return subResult;
        }

        return NuHTNTaskResult.ContinueTask;
    }

    public override void Stop(Entity<NuHTNComponent> self, NuParallelPlan plan)
    {
        foreach (var subplan in plan.Plans)
        {
            subplan.Stop(self, HTN);
        }
    }
}
