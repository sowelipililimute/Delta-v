using Content.Shared.Damage.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;

namespace Content.Server._DV.NPC.EntityConsiderations;

/// <summary>
/// Considers the health of the entity, with 0 damage being 100% considered, and the target state being 0% considered
/// </summary>
public sealed partial class NuHealthConsideration : NuHTNEntityConsiderationBase<NuHealthConsideration, NuHealthConsiderationSystem>
{
    [DataField]
    public MobState TargetState = MobState.Invalid;
}

public sealed class NuHealthConsiderationSystem : NuHTNEntityConsiderationSystem<NuHealthConsiderationSystem, NuHealthConsideration>
{
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

    public override float Consideration(Entity<NuHTNComponent> self, EntityUid target, NuHealthConsideration consideration)
    {
        if (!TryComp<DamageableComponent>(target, out var damageable))
            return 0f;

        if (consideration.TargetState != MobState.Invalid && _mobThreshold.TryGetPercentageForState(target,
                consideration.TargetState,
                damageable.TotalDamage,
                out var percentage))
            return Math.Clamp(1f - percentage.Value.Float(), 0f, 1f);

        if (_mobThreshold.TryGetIncapPercentage(target, damageable.TotalDamage, out var incapacitationPercentage))
            return Math.Clamp(1f - incapacitationPercentage.Value.Float(), 0f, 1f);

        return 0f;
    }
}
