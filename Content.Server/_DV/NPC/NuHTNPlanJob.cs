using System.Threading;
using System.Threading.Tasks;
using Robust.Shared.CPUJob.JobQueues;

namespace Content.Server._DV.NPC;

public sealed class NuHTNPlanJob : Job<NuHTNPlan>
{
    private readonly NuHTNTask _task;
    private readonly Entity<NuHTNComponent> _self;
    private readonly NuHTNSystem _system;

    public NuHTNPlanJob(
        NuHTNTask task,
        Entity<NuHTNComponent> self,
        NuHTNSystem system,
        double maxTime,
        CancellationToken cancellation = default) : base(maxTime, cancellation)
    {
        _task = task;
        _self = self;
        _system = system;
    }

    protected override async Task<NuHTNPlan?> Process()
    {
        return await WaitAsyncTask(_task.Plan(_self, _system, Cancellation));
    }
}
