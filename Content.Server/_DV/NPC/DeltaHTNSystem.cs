namespace Content.Server._DV.NPC;

public sealed class DeltaHTNSystem : EntitySystem
{
    [Access(typeof(DeltaHTNSensorBase<,>))]
    public void Update<TSensor, TSystem>(Entity<DeltaHTNComponent> self, TSensor sensor)
        where TSensor : DeltaHTNSensorBase<TSensor, TSystem>
        where TSystem : DeltaHTNSensorSystem<TSystem, TSensor>
    {
        EntityManager.System<TSystem>().Update(self, sensor);
    }
}
