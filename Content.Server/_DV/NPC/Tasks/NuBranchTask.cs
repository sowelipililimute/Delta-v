using System.Threading;
using System.Threading.Tasks;

namespace Content.Server._DV.NPC.Tasks;

public sealed partial class NuBranchTask : NuHTNTaskBase<NuBranchTask, NuBranchPlan, NuBranchSystem>
{
    [DataField(required: true)]
    public List<NuBranch> Branches;
}

[DataDefinition]
public sealed partial class NuBranch
{
    [DataField(required: true)]
    public NuHTNTask Task;
}

public sealed partial class NuBranchPlan : NuHTNPlanBase<NuBranchPlan, NuBranchTask, NuBranchSystem>
{
    [DataField]
    public NuHTNPlan Plan;
}

public sealed class NuBranchSystem : NuHTNTaskSystem<NuBranchSystem, NuBranchTask, NuBranchPlan>
{
    public override async Task<NuBranchPlan?> Plan(Entity<NuHTNComponent> self, NuBranchTask task, CancellationToken token)
    {
        foreach (var branch in task.Branches)
        {
            if (await branch.Task.Plan(self, HTN, token) is { } plan)
            {
                return new NuBranchPlan
                {
                    Task = task,
                    Plan = plan,
                };
            }
        }

        return null;
    }

    public override void Start(Entity<NuHTNComponent> self, NuBranchPlan plan)
    {
        plan.Plan.Start(self, HTN);
    }

    public override NuHTNTaskResult Update(Entity<NuHTNComponent> self, NuBranchPlan plan, float frameTime)
    {
        return plan.Plan.Update(self, HTN, frameTime);
    }

    public override void Stop(Entity<NuHTNComponent> self, NuBranchPlan plan)
    {
        plan.Plan.Stop(self, HTN);
    }
}
