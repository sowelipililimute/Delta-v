namespace Content.Server._DV.NPC.EntityConsiderations;

public sealed partial class NuDistanceConsideration : NuHTNEntityConsiderationBase<NuDistanceConsideration, NuDistanceConsiderationSystem>
{
    [DataField]
    public float Range = 5f;
}

public sealed class NuDistanceConsiderationSystem : NuHTNEntityConsiderationSystem<NuDistanceConsiderationSystem, NuDistanceConsideration>
{
    public override float Consideration(Entity<NuHTNComponent> self, EntityUid target, NuDistanceConsideration consideration)
    {
        var selfXform = Transform(self);
        var targetXform = Transform(target);

        if (selfXform.Coordinates.TryDistance(EntityManager, targetXform.Coordinates, out var distance))
            return 1f - Math.Clamp(distance / consideration.Range, 0f, 1f);

        return 0f;
    }
}
