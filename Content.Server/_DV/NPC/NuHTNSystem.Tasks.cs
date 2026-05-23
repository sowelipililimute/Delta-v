using System.Threading;
using System.Threading.Tasks;

namespace Content.Server._DV.NPC;

public sealed partial class NuHTNSystem
{
    public async Task<NuHTNPlan?> Plan<TTask, TPlan, TSystem>(Entity<NuHTNComponent> self, TTask task, CancellationToken token)
        where TTask : NuHTNTaskBase<TTask, TPlan, TSystem>
        where TPlan : NuHTNPlanBase<TPlan, TTask, TSystem>
        where TSystem : NuHTNTaskSystem<TSystem, TTask, TPlan>
    {
        return await EntityManager.System<TSystem>().Plan(self, task, token);
    }

    public void Start<TTask, TPlan, TSystem>(Entity<NuHTNComponent> self, TPlan plan)
        where TTask : NuHTNTaskBase<TTask, TPlan, TSystem>
        where TPlan : NuHTNPlanBase<TPlan, TTask, TSystem>
        where TSystem : NuHTNTaskSystem<TSystem, TTask, TPlan>
    {
        EntityManager.System<TSystem>().Start(self, plan);
    }

    public NuHTNTaskResult Execute<TTask, TPlan, TSystem>(Entity<NuHTNComponent> self, TPlan plan, float frameTime)
        where TTask : NuHTNTaskBase<TTask, TPlan, TSystem>
        where TPlan : NuHTNPlanBase<TPlan, TTask, TSystem>
        where TSystem : NuHTNTaskSystem<TSystem, TTask, TPlan>
    {
        return EntityManager.System<TSystem>().Update(self, plan, frameTime);
    }

    public void Stop<TTask, TPlan, TSystem>(Entity<NuHTNComponent> self, TPlan plan)
        where TTask : NuHTNTaskBase<TTask, TPlan, TSystem>
        where TPlan : NuHTNPlanBase<TPlan, TTask, TSystem>
        where TSystem : NuHTNTaskSystem<TSystem, TTask, TPlan>
    {
        EntityManager.System<TSystem>().Stop(self, plan);
    }
}
