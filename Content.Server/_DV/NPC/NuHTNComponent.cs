using System.Threading;

namespace Content.Server._DV.NPC;

[RegisterComponent]
public sealed partial class NuHTNComponent : Component
{
    [DataField]
    public Dictionary<Type, AnyMemoryKindData> Memories = new();

    [DataField]
    public List<NuHTNSensor> Sensors = new();

    [DataField(required: true)]
    public NuHTNTask Task;

    [DataField]
    public TimeSpan SensingInterval = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan NextSense = TimeSpan.Zero;

    [DataField]
    public TimeSpan PlanningInterval = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan NextPlan = TimeSpan.Zero;

    [DataField]
    public NuHTNPlan? Plan = null;

    [ViewVariables]
    public NuHTNPlanJob? PlanningJob;

    [ViewVariables]
    public CancellationTokenSource? PlanningJobToken;
}

public readonly record struct MemoryId<TMemory>(string Id);

[ImplicitDataDefinitionForInheritors]
public abstract partial class AnyMemoryKindData
{
    public abstract Dictionary<string, TMemory> GetMemories<TMemory>();
}

public sealed partial class MemoryKindData<TKind> : AnyMemoryKindData
{
    [DataField]
    public Dictionary<string, TKind> Memories = new();

    public override Dictionary<string, TMemory> GetMemories<TMemory>()
    {
        if (Memories is not Dictionary<string, TMemory> memories)
            throw new InvalidOperationException($"The memory kind for {typeof(TKind)} has been used to retrieve memories for {typeof(TMemory)}");

        return memories;
    }
}
