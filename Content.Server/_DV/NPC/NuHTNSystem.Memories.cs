namespace Content.Server._DV.NPC;

public sealed partial class NuHTNSystem
{
    public void Update<TSensor, TSystem>(Entity<NuHTNComponent> self, TSensor sensor)
        where TSensor : NuHTNSensorBase<TSensor, TSystem>
        where TSystem : NuHTNSensorSystem<TSystem, TSensor>
    {
        EntityManager.System<TSystem>().Update(self, sensor);
    }

    private TMemory Unwrap<TMemory>(MemoryId<TMemory> id, Memory<TMemory> memory)
    {
        if (memory.ExpiresAfter is { } expiry && _timing.CurTime <= expiry)
            throw new InvalidOperationException($"Memory {id.Id} expired at {expiry} vs {_timing.CurTime}");

        return memory.Value;
    }

    public TMemory GetMemory<TMemory>(Entity<NuHTNComponent> ent, MemoryId<TMemory> id)
    {
        return Unwrap(id, ent.Comp.Memories[typeof(TMemory)].GetMemories<TMemory>()[id.Id]);
    }

    public bool TryGetMemory<TMemory>(Entity<NuHTNComponent> ent, MemoryId<TMemory> id, out TMemory? memory)
    {
        if (!ent.Comp.Memories.TryGetValue(typeof(TMemory), out var memories)
            || !memories.GetMemories<TMemory>().TryGetValue(id.Id, out var brainMemory)
            || brainMemory.ExpiresAfter is { } expiry && _timing.CurTime <= expiry)
        {
            memory = default;
            return false;
        }

        memory = brainMemory.Value;
        return true;
    }

    public void SetMemory<TMemory>(Entity<NuHTNComponent> ent, MemoryId<TMemory> id, TMemory memory, TimeSpan? expiry = null)
    {
        if (!ent.Comp.Memories.TryGetValue(typeof(TMemory), out var memories))
        {
            memories = new MemoryKindData<TMemory>();
            ent.Comp.Memories[typeof(TMemory)] = memories;
        }

        memories.GetMemories<TMemory>()[id.Id] = new(expiry, memory);
    }

    public bool RemoveMemory<TMemory>(Entity<NuHTNComponent> ent, MemoryId<TMemory> id, out TMemory? memory)
    {
        if (!ent.Comp.Memories.TryGetValue(typeof(TMemory), out var memories)
            || !memories.GetMemories<TMemory>().Remove(id.Id, out var brainMemory)
            || brainMemory.ExpiresAfter is { } expiry && _timing.CurTime <= expiry)
        {
            memory = default;
            return false;
        }

        memory = brainMemory.Value;
        return true;
    }
}
