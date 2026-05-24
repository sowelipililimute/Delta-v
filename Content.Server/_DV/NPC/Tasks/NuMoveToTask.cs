using System.Threading;
using System.Threading.Tasks;
using Content.Server.NPC.Components;
using Content.Server.NPC.Pathfinding;
using Content.Server.NPC.Systems;
using Robust.Shared.Map;

namespace Content.Server._DV.NPC.Tasks;

public sealed partial class NuMoveToTask : NuHTNTaskBase<NuMoveToTask, NuMoveToPlan, NuMoveToSystem>
{
    [DataField]
    public MemoryId<EntityCoordinates> TargetMemory;

    [DataField]
    public float FinishedWithin = 1f;

    [DataField]
    public PathFlags Flags = PathFlags.Climbing | PathFlags.Prying | PathFlags.Interact;
}

public sealed partial class NuMoveToPlan : NuHTNPlanBase<NuMoveToPlan, NuMoveToTask, NuMoveToSystem>
{
    public List<PathPoly> Path = new();
}

public sealed class NuMoveToSystem : NuHTNTaskSystem<NuMoveToSystem, NuMoveToTask, NuMoveToPlan>
{
    [Dependency] private readonly PathfindingSystem _pathfinding = default!;
    [Dependency] private readonly NPCSteeringSystem _npcSteering = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override async Task<NuMoveToPlan?> Plan(Entity<NuHTNComponent> self, NuMoveToTask task, CancellationToken token)
    {
        if (!HTN.TryGetMemory(self, task.TargetMemory, out var target))
            return null;

        var path = await _pathfinding.GetPath(
            self.Owner,
            Transform(self).Coordinates,
            target,
            task.FinishedWithin,
            token,
            task.Flags);

        if (path.Result != PathResult.Path)
            return null;

        return new()
        {
            Task = task,
            Path = path.Path,
        };
    }

    public override void Start(Entity<NuHTNComponent> self, NuMoveToPlan plan)
    {
        var target = HTN.GetMemory(self, plan.Task.TargetMemory);

        var selfCoordinates = _transform.ToMapCoordinates(Transform(self).Coordinates);

        _npcSteering.Register(self, target);
        Comp<NPCSteeringComponent>(self).Flags = plan.Task.Flags;
        _npcSteering.PrunePath(self, selfCoordinates, _transform.ToMapCoordinates(target).Position - selfCoordinates.Position, plan.Path);
    }

    public override NuHTNTaskResult Update(Entity<NuHTNComponent> self, NuMoveToPlan plan, float frameTime)
    {
        if (!TryComp<NPCSteeringComponent>(self, out var steering))
            return NuHTNTaskResult.Replan;

        if (HTN.TryGetMemory(self, plan.Task.TargetMemory, out var target))
        {
            _npcSteering.Register(self, target);
            Comp<NPCSteeringComponent>(self).Flags = plan.Task.Flags;
        }

        return steering.Status switch
        {
            SteeringStatus.InRange => NuHTNTaskResult.TaskComplete,
            SteeringStatus.NoPath => NuHTNTaskResult.Replan,
            SteeringStatus.Moving => NuHTNTaskResult.ContinueTask,
            _ => throw new InvalidOperationException($"{ToPrettyString(self)} has invalid steering status {steering.Status}"),
        };
    }

    public override void Stop(Entity<NuHTNComponent> self, NuMoveToPlan plan)
    {
        _npcSteering.Unregister(self.Owner);
    }
}
