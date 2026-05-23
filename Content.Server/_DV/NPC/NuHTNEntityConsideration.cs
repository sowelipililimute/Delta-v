namespace Content.Server._DV.NPC;

[ImplicitDataDefinitionForInheritors]
public abstract partial class NuHTNEntityConsideration
{
    public abstract float Consideration(Entity<NuHTNComponent> self, EntityUid target, NuHTNSystem system);
}

public abstract class NuHTNEntityConsiderationSystem<TSelf, TConsideration> : EntitySystem
    where TSelf : NuHTNEntityConsiderationSystem<TSelf, TConsideration>
    where TConsideration : NuHTNEntityConsiderationBase<TConsideration, TSelf>
{
    [Dependency] protected readonly NuHTNSystem HTN = default!;

    public abstract float Consideration(Entity<NuHTNComponent> self, EntityUid target, TConsideration consideration);
}

public abstract partial class NuHTNEntityConsiderationBase<TSelf, TSystem> : NuHTNEntityConsideration
    where TSelf : NuHTNEntityConsiderationBase<TSelf, TSystem>
    where TSystem : NuHTNEntityConsiderationSystem<TSystem, TSelf>
{
    public override float Consideration(Entity<NuHTNComponent> self, EntityUid target, NuHTNSystem system)
    {
        if (this is not TSelf tSelf)
            throw new InvalidOperationException();

        return system.Consideration<TSelf, TSystem>(self, target, tSelf);
    }
}
