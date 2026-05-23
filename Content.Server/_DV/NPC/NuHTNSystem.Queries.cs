namespace Content.Server._DV.NPC;

public sealed partial class NuHTNSystem
{
    public HashSet<EntityUid> Query<TQuery, TSystem>(Entity<NuHTNComponent> self, TQuery query)
        where TQuery : NuHTNEntityQueryBase<TQuery, TSystem>
        where TSystem : NuHTNEntityQuerySystem<TSystem, TQuery>
    {
        return EntityManager.System<TSystem>().Query(self, query);
    }
}
