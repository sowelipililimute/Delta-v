namespace Content.Server._DV.NPC;

public sealed partial class NuHTNSystem
{
    public float Consideration<TQuery, TSystem>(Entity<NuHTNComponent> self, EntityUid target, TQuery query)
        where TQuery : NuHTNEntityConsiderationBase<TQuery, TSystem>
        where TSystem : NuHTNEntityConsiderationSystem<TSystem, TQuery>
    {
        return EntityManager.System<TSystem>().Consideration(self, target, query);
    }
}
