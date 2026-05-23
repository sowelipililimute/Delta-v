namespace Content.Server._DV.NPC;

[ImplicitDataDefinitionForInheritors]
public abstract partial class NuHTNSensor
{
    public abstract void Update(Entity<NuHTNComponent> self, NuHTNSystem system);
}

public abstract class NuHTNSensorSystem<TSelf, TSensor> : EntitySystem
    where TSelf : NuHTNSensorSystem<TSelf, TSensor>
    where TSensor : NuHTNSensorBase<TSensor, TSelf>
{
    [Dependency] protected readonly NuHTNSystem HTN = default!;

    public abstract void Update(Entity<NuHTNComponent> self, TSensor sensor);
}

public abstract partial class NuHTNSensorBase<TSelf, TSystem> : NuHTNSensor
    where TSelf : NuHTNSensorBase<TSelf, TSystem>
    where TSystem : NuHTNSensorSystem<TSystem, TSelf>
{
    public override void Update(Entity<NuHTNComponent> self, NuHTNSystem system)
    {
        if (this is not TSelf tSelf)
            throw new InvalidOperationException();

        system.Update<TSelf, TSystem>(self, tSelf);
    }
}
