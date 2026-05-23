namespace Content.Server._DV.NPC;

[ImplicitDataDefinitionForInheritors]
public abstract partial class NuHTNEntityQuery
{
    public abstract HashSet<EntityUid> Query(Entity<NuHTNComponent> self, NuHTNSystem system);
}

public abstract class NuHTNEntityQuerySystem<TSelf, TQuery> : EntitySystem
    where TSelf : NuHTNEntityQuerySystem<TSelf, TQuery>
    where TQuery : NuHTNEntityQueryBase<TQuery, TSelf>
{
    [Dependency] protected readonly NuHTNSystem HTN = default!;

    public abstract HashSet<EntityUid> Query(Entity<NuHTNComponent> self, TQuery query);
}

public abstract partial class NuHTNEntityQueryBase<TSelf, TSystem> : NuHTNEntityQuery
    where TSelf : NuHTNEntityQueryBase<TSelf, TSystem>
    where TSystem : NuHTNEntityQuerySystem<TSystem, TSelf>
{
    public override HashSet<EntityUid> Query(Entity<NuHTNComponent> self, NuHTNSystem system)
    {
        if (this is not TSelf tSelf)
            throw new InvalidOperationException();

        return system.Query<TSelf, TSystem>(self, tSelf);
    }
}
