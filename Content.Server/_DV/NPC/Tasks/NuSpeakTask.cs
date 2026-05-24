using System.Threading;
using System.Threading.Tasks;
using Content.Shared.Chat;

namespace Content.Server._DV.NPC.Tasks;

public sealed partial class NuSpeakTask : NuHTNTaskBase<NuSpeakTask, NuSpeakPlan, NuSpeakSystem>
{
    [DataField(required: true)]
    public string Text;
}

public sealed partial class NuSpeakPlan : NuHTNPlanBase<NuSpeakPlan, NuSpeakTask, NuSpeakSystem>;

public sealed class NuSpeakSystem : NuHTNTaskSystem<NuSpeakSystem, NuSpeakTask, NuSpeakPlan>
{
    [Dependency] private readonly SharedChatSystem _chat = default!;

    public override Task<NuSpeakPlan?> Plan(Entity<NuHTNComponent> self, NuSpeakTask task, CancellationToken token)
    {
        return Task.FromResult<NuSpeakPlan?>(new NuSpeakPlan
        {
            Task = task,
        });
    }

    public override NuHTNTaskResult Update(Entity<NuHTNComponent> self, NuSpeakPlan plan, float frameTime)
    {
        _chat.TrySendInGameICMessage(self, plan.Task.Text, InGameICChatType.Speak, true);
        return NuHTNTaskResult.TaskComplete;
    }
}
