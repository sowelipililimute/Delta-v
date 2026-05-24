using System.Threading;
using System.Threading.Tasks;

namespace Content.Server._DV.NPC.Tasks;

public sealed partial class NuSequenceTask : NuHTNTaskBase<NuSequenceTask, NuSequencePlan, NuSequenceSystem>
{
    [DataField(required: true)]
    public List<NuHTNTask> Tasks;
}

public sealed partial class NuSequencePlan : NuHTNPlanBase<NuSequencePlan, NuSequenceTask, NuSequenceSystem>
{
    [DataField]
    public NuHTNPlan? CurrentPlan;

    [DataField]
    public Queue<NuHTNPlan> Plans;
}

public sealed class NuSequenceSystem : NuHTNTaskSystem<NuSequenceSystem, NuSequenceTask, NuSequencePlan>
{
    public override async Task<NuSequencePlan?> Plan(Entity<NuHTNComponent> self, NuSequenceTask task, CancellationToken token)
    {
        if (task.Tasks.Count < 2)
            throw new InvalidOperationException($"The sequence for {ToPrettyString(self)} had less than 2 tasks");

        var queue = new Queue<NuHTNPlan>();
        foreach (var subtask in task.Tasks)
        {
            var subplan = await subtask.Plan(self, HTN, token);
            if (subplan is null)
                return null;

            queue.Enqueue(subplan);
        }

        return new NuSequencePlan
        {
            Task = task,
            Plans = queue,
        };
    }

    public override void Start(Entity<NuHTNComponent> self, NuSequencePlan plan)
    {
        plan.CurrentPlan = plan.Plans.Dequeue();
        plan.CurrentPlan.Start(self, HTN);
    }

    public override NuHTNTaskResult Update(Entity<NuHTNComponent> self, NuSequencePlan plan, float frameTime)
    {
        if (plan.CurrentPlan is null)
            throw new InvalidOperationException($"Trying to update {ToPrettyString(self)}, but it has no queued plans");

        switch (plan.CurrentPlan.Update(self, HTN, frameTime))
        {
            case NuHTNTaskResult.ContinueTask:
                return NuHTNTaskResult.ContinueTask;
            case NuHTNTaskResult.Replan:
                plan.CurrentPlan.Stop(self, HTN);
                plan.CurrentPlan = null;
                return NuHTNTaskResult.Replan;
            case NuHTNTaskResult.TaskComplete:
                break;
            default:
                throw new InvalidOperationException($"Subtask {plan.CurrentPlan} returned invalid plan result {plan.CurrentPlan}");
        }

        plan.CurrentPlan.Stop(self, HTN);
        plan.CurrentPlan = null;
        if (!plan.Plans.TryDequeue(out var nextSubplan))
            return NuHTNTaskResult.TaskComplete;

        plan.CurrentPlan = nextSubplan;
        plan.CurrentPlan.Start(self, HTN);
        return plan.CurrentPlan.Update(self, HTN, frameTime);
    }

    public override void Stop(Entity<NuHTNComponent> self, NuSequencePlan plan)
    {
        plan.CurrentPlan?.Stop(self, HTN);
    }
}
