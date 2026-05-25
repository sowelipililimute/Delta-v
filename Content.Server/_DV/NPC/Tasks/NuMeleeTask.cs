using System.Threading;
using System.Threading.Tasks;
using Content.Server.NPC.Components;
using Content.Shared.CombatMode;

namespace Content.Server._DV.NPC.Tasks;

public sealed partial class NuMeleeTask : NuHTNTaskBase<NuMeleeTask, NuMeleePlan, NuMeleeSystem>
{
    [DataField]
    public MemoryId<EntityUid> TargetMemory;
}

public sealed partial class NuMeleePlan : NuHTNPlanBase<NuMeleePlan, NuMeleeTask, NuMeleeSystem>;

public sealed class NuMeleeSystem : NuHTNTaskSystem<NuMeleeSystem, NuMeleeTask, NuMeleePlan>
{
    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;

    public override async Task<NuMeleePlan?> Plan(Entity<NuHTNComponent> self, NuMeleeTask task, CancellationToken token)
    {
        if (!HTN.TryGetMemory(self, task.TargetMemory, out _))
            return null;

        return new NuMeleePlan
        {
            Task = task,
        };
    }

    public override void Start(Entity<NuHTNComponent> self, NuMeleePlan plan)
    {
        if (!HTN.TryGetMemory(self, plan.Task.TargetMemory, out var target))
            return;

        var combat = EnsureComp<NPCMeleeCombatComponent>(self);
        combat.Steer = false;
        combat.Target = target;
        _combatMode.SetInCombatMode(self, true);
    }

    public override NuHTNTaskResult Update(Entity<NuHTNComponent> self, NuMeleePlan plan, float frameTime)
    {
        if (!TryComp<NPCMeleeCombatComponent>(self, out var combat))
            return NuHTNTaskResult.Replan;

        if (HTN.TryGetMemory(self, plan.Task.TargetMemory, out var target))
        {
            combat.Target = target;
        }

        return NuHTNTaskResult.ContinueTask;
    }

    public override void Stop(Entity<NuHTNComponent> self, NuMeleePlan plan)
    {
        RemComp<NPCMeleeCombatComponent>(self);
        _combatMode.SetInCombatMode(self, false);
    }
}
