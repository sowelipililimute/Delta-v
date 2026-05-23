namespace Content.Server._DV.NPC;

[RegisterComponent]
public sealed partial class DeltaHTNComponent : Component
{
    // [DataField]
    // public
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class DeltaHTNSensor
{
    public abstract void Update(Entity<DeltaHTNComponent> self, DeltaHTNSystem system);
}

public abstract class DeltaHTNSensorSystem<TSelf, TSensor> : EntitySystem
    where TSelf : DeltaHTNSensorSystem<TSelf, TSensor>
    where TSensor : DeltaHTNSensorBase<TSensor, TSelf>
{
    public abstract void Update(Entity<DeltaHTNComponent> self, TSensor sensor);
}

public abstract partial class DeltaHTNSensorBase<TSelf, TSystem> : DeltaHTNSensor
    where TSelf : DeltaHTNSensorBase<TSelf, TSystem>
    where TSystem : DeltaHTNSensorSystem<TSystem, TSelf>
{
    public override void Update(Entity<DeltaHTNComponent> self, DeltaHTNSystem system)
    {
        if (this is not TSelf tSelf)
            return;
        
        system.Update<TSelf, TSystem>(self, tSelf);
    }
}

public readonly record struct MemoryId<T>(string Id);
