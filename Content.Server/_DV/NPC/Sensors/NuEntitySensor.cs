using System.Linq;
using Robust.Shared.Utility;

namespace Content.Server._DV.NPC.Sensors;

public sealed partial class NuEntitySensor : NuHTNSensorBase<NuEntitySensor, NuEntitySensorSystem>
{
    [DataField(required: true)]
    public MemoryId<EntityUid> Memory;

    [DataField(required: true)]
    public NuHTNEntityQuery Query;

    [DataField]
    public List<NuHTNEntityConsideration> Considerations = new();
}

public sealed class NuEntitySensorSystem : NuHTNSensorSystem<NuEntitySensorSystem, NuEntitySensor>
{
    public override void Update(Entity<NuHTNComponent> self, NuEntitySensor sensor)
    {
        var entities = sensor.Query.Query(self, HTN);
        var dictionary = new Dictionary<EntityUid, float>();

        foreach (var entity in entities)
        {
            var considerationTally = 1f;

            foreach (var consideration in sensor.Considerations)
            {
                var score = consideration.Consideration(self, entity, HTN);
                DebugTools.Assert(score is >= 0f and <= 1f, $"Score from consideration {consideration.GetType()} needs to be between 0-1, but was {score}");
                var adjusted = 1f - (1f - score) / sensor.Considerations.Count;
                considerationTally *= adjusted;
            }

            dictionary[entity] = MathF.Pow(considerationTally, 1f / sensor.Considerations.Count);
        }

        if (dictionary.Count == 0)
        {
            HTN.RemoveMemory(self, sensor.Memory, out _);
            return;
        }

        HTN.SetMemory(self, sensor.Memory, dictionary.MaxBy(kvp => kvp.Value).Key);
    }
}
