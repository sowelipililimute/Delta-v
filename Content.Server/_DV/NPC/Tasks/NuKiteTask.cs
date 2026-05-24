using System.Threading;
using System.Threading.Tasks;
using Content.Server.NPC.Components;
using Content.Server.NPC.Systems;
using Robust.Shared.Map;

namespace Content.Server._DV.NPC.Tasks;

public sealed partial class NuKiteTask : NuHTNTaskBase<NuKiteTask, NuKitePlan, NuKiteSystem>
{
    [DataField]
    public MemoryId<EntityUid> TargetMemory;
}

public sealed partial class NuKitePlan : NuHTNPlanBase<NuKitePlan, NuKiteTask, NuKiteSystem>;

public sealed class NuKiteSystem : NuHTNTaskSystem<NuKiteSystem, NuKiteTask, NuKitePlan>
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly NPCSteeringSystem _npcSteering = default!;

    public override async Task<NuKitePlan?> Plan(Entity<NuHTNComponent> self, NuKiteTask task, CancellationToken token)
    {
        if (!HTN.TryGetMemory(self, task.TargetMemory, out var target))
            return null;

        return new NuKitePlan
        {
            Task = task,
        };
    }

    public override NuHTNTaskResult Update(Entity<NuHTNComponent> self, NuKitePlan plan, float frameTime)
    {
        if (!HTN.TryGetMemory(self, plan.Task.TargetMemory, out var target))
            return NuHTNTaskResult.Replan;

        var selfCoords = _transform.GetMapCoordinates(self);
        var targetCoords = _transform.GetMapCoordinates(target);

        var targetToSelf = selfCoords.Position - targetCoords.Position;
        targetToSelf.Normalize();
        targetToSelf *= 2f;

        var actualPosition = targetCoords.Position + targetToSelf;
        var actualCoordinates = new MapCoordinates(actualPosition, targetCoords.MapId);

        var steeringPosition = _transform.ToCoordinates(target, actualCoordinates);
        _npcSteering.Register(self, steeringPosition);

        var steering = Comp<NPCSteeringComponent>(self);
        return steering.Status switch
        {
            SteeringStatus.NoPath => NuHTNTaskResult.Replan,
            SteeringStatus.Moving or SteeringStatus.InRange => NuHTNTaskResult.ContinueTask,
            _ => throw new InvalidOperationException($"{ToPrettyString(self)} has invalid steering status {steering.Status}"),
        };
    }
}
