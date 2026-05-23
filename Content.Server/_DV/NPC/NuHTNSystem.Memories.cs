namespace Content.Server._DV.NPC;

public sealed partial class NuHTNSystem
{
    public void Update<TSensor, TSystem>(Entity<NuHTNComponent> self, TSensor sensor)
        where TSensor : NuHTNSensorBase<TSensor, TSystem>
        where TSystem : NuHTNSensorSystem<TSystem, TSensor>
    {
        EntityManager.System<TSystem>().Update(self, sensor);
    }

    public TMemory GetMemory<TMemory>(Entity<NuHTNComponent> ent, MemoryId<TMemory> id)
    {
        return ent.Comp.Memories[typeof(TMemory)].GetMemories<TMemory>()[id.Id];
    }

    public bool TryGetMemory<TMemory>(Entity<NuHTNComponent> ent, MemoryId<TMemory> id, out TMemory? memory)
    {
        if (!ent.Comp.Memories.TryGetValue(typeof(TMemory), out var memories))
        {
            memory = default;
            return false;
        }

        return memories.GetMemories<TMemory>().TryGetValue(id.Id, out memory);
    }

    public void SetMemory<TMemory>(Entity<NuHTNComponent> ent, MemoryId<TMemory> id, TMemory memory)
    {
        if (!ent.Comp.Memories.TryGetValue(typeof(TMemory), out var memories))
        {
            memories = new MemoryKindData<TMemory>();
            ent.Comp.Memories[typeof(TMemory)] = memories;
        }

        memories.GetMemories<TMemory>()[id.Id] = memory;
    }

    public bool RemoveMemory<TMemory>(Entity<NuHTNComponent> ent, MemoryId<TMemory> id, out TMemory? memory)
    {
        if (!ent.Comp.Memories.TryGetValue(typeof(TMemory), out var memories))
        {
            memory = default;
            return false;
        }

        return memories.GetMemories<TMemory>().Remove(id.Id, out memory);
    }
}
