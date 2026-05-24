using System.Threading;
using System.Threading.Tasks;

namespace Content.Server._DV.NPC;

[ImplicitDataDefinitionForInheritors]
public abstract partial class NuHTNTask
{
    public abstract Task<NuHTNPlan?> Plan(Entity<NuHTNComponent> self, NuHTNSystem system, CancellationToken token);
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class NuHTNPlan
{
    public abstract void Start(Entity<NuHTNComponent> self, NuHTNSystem system);
    public abstract NuHTNTaskResult Update(Entity<NuHTNComponent> self, NuHTNSystem system, float frameTime);
    public abstract void Stop(Entity<NuHTNComponent> self, NuHTNSystem system);
}

public abstract partial class NuHTNTaskBase<TSelf, TPlan, TSystem> : NuHTNTask
    where TSelf : NuHTNTaskBase<TSelf, TPlan, TSystem>
    where TPlan : NuHTNPlanBase<TPlan, TSelf, TSystem>
    where TSystem : NuHTNTaskSystem<TSystem, TSelf, TPlan>
{
    public override Task<NuHTNPlan?> Plan(Entity<NuHTNComponent> self, NuHTNSystem system, CancellationToken token)
    {
        if (this is not TSelf tSelf)
            throw new InvalidOperationException();

        return system.Plan<TSelf, TPlan, TSystem>(self, tSelf, token);
    }
}

public abstract partial class NuHTNPlanBase<TSelf, TTask, TSystem> : NuHTNPlan
    where TSelf : NuHTNPlanBase<TSelf, TTask, TSystem>
    where TTask : NuHTNTaskBase<TTask, TSelf, TSystem>
    where TSystem : NuHTNTaskSystem<TSystem, TTask, TSelf>
{
    [DataField]
    public TTask Task;

    public override void Start(Entity<NuHTNComponent> self, NuHTNSystem system)
    {
        if (this is not TSelf tSelf)
            throw new InvalidOperationException();

        system.Start<TTask, TSelf, TSystem>(self, tSelf);
    }

    public override NuHTNTaskResult Update(Entity<NuHTNComponent> self, NuHTNSystem system, float frameTime)
    {
        if (this is not TSelf tSelf)
            throw new InvalidOperationException();

        return system.Execute<TTask, TSelf, TSystem>(self, tSelf, frameTime);
    }

    public override void Stop(Entity<NuHTNComponent> self, NuHTNSystem system)
    {
        if (this is not TSelf tSelf)
            throw new InvalidOperationException();

        system.Stop<TTask, TSelf, TSystem>(self, tSelf);
    }
}

public abstract class NuHTNTaskSystem<TSelf, TTask, TPlan> : EntitySystem
    where TSelf : NuHTNTaskSystem<TSelf, TTask, TPlan>
    where TTask : NuHTNTaskBase<TTask, TPlan, TSelf>
    where TPlan : NuHTNPlanBase<TPlan, TTask, TSelf>
{
    [Dependency] protected readonly NuHTNSystem HTN = default!;

    public abstract Task<TPlan?> Plan(Entity<NuHTNComponent> self, TTask task, CancellationToken token);

    public virtual void Start(Entity<NuHTNComponent> self, TPlan plan)
    {
    }
    public abstract NuHTNTaskResult Update(Entity<NuHTNComponent> self, TPlan plan, float frameTime);

    public virtual void Stop(Entity<NuHTNComponent> self, TPlan plan)
    {
    }
}

public enum NuHTNTaskResult
{
    ContinueTask,
    Replan,
    TaskComplete,
}


