using Content.Shared.NPC.Systems;

namespace Content.Server._DV.NPC.EntityQueries;

public sealed partial class NuNearbyHostilesQuery : NuHTNEntityQueryBase<NuNearbyHostilesQuery, NuNearbyHostilesQuerySystem>
{
    [DataField]
    public float Range = 5f;
}

public sealed class NuNearbyHostilesQuerySystem : NuHTNEntityQuerySystem<NuNearbyHostilesQuerySystem, NuNearbyHostilesQuery>
{
    [Dependency] private readonly NpcFactionSystem _faction = default!;

    public override HashSet<EntityUid> Query(Entity<NuHTNComponent> self, NuNearbyHostilesQuery query)
    {
        return new(_faction.GetNearbyHostiles(self.Owner, query.Range));
    }
}
