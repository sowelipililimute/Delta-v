using System.Threading;
using Content.Shared.NPC;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Timing;

namespace Content.Server._DV.NPC;

public sealed partial class NuHTNSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    private readonly JobQueue _queue = new(0.004);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _queue.Process();

        var query = EntityQueryEnumerator<NuHTNComponent>();
        while (query.MoveNext(out var uid, out var htn))
        {
            UpdateSensor((uid, htn));

            if (htn.PlanningJob is { } job)
                UpdateActiveJob((uid, htn), job);

            if (htn.Plan is { } plan)
                UpdateActivePlan((uid, htn), plan, frameTime);
            else
                UpdateIdle((uid, htn));
        }
    }

    private void UpdateSensor(Entity<NuHTNComponent> self)
    {
        if (_timing.CurTime <= self.Comp.NextSense)
            return;

        self.Comp.NextSense = _timing.CurTime + self.Comp.SensingInterval;
        foreach (var sensor in self.Comp.Sensors)
        {
            sensor.Update(self, this);
        }
    }

    private void UpdateActiveJob(Entity<NuHTNComponent> self, NuHTNPlanJob job)
    {
        if (job.Exception is { } exception)
        {
            Log.Fatal($"Planning job for {ToPrettyString(self)} ended with an exception");
            self.Comp.PlanningJob = null;
            throw exception;
        }

        if (job.Status != JobStatus.Finished)
            return;

        self.Comp.Plan = job.Result;
        self.Comp.Plan?.Start(self, this);
        self.Comp.PlanningJob = null;
        self.Comp.PlanningJobToken = null;

        if (self.Comp.Plan is not null)
            AddComp<ActiveNPCComponent>(self);
    }

    private void UpdateActivePlan(Entity<NuHTNComponent> self, NuHTNPlan plan, float frameTime)
    {
        if (plan.Update(self, this, frameTime) == NuHTNTaskResult.ContinueTask)
            return;

        plan.Stop(self, this);
        self.Comp.Plan = null;
    }

    private void UpdateIdle(Entity<NuHTNComponent> self)
    {
        if (_timing.CurTime <= self.Comp.NextPlan)
            return;

        self.Comp.NextPlan = _timing.CurTime + self.Comp.PlanningInterval;

        var token = new CancellationTokenSource();
        var job = new NuHTNPlanJob(self.Comp.Task, self, this, 0.02f, token.Token);

        _queue.EnqueueJob(job);

        self.Comp.PlanningJob = job;
        self.Comp.PlanningJobToken = token;
        RemComp<ActiveNPCComponent>(self);
    }
}
