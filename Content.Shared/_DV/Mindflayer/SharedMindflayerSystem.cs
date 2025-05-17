using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.DoAfter;
using Content.Shared.Implants;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC;
using Content.Shared.StatusEffect;

namespace Content.Shared._DV.Mindflayer;

public abstract class SharedMindflayerSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedSubdermalImplantSystem _subdermalImplant = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindflayerComponent, MindflayerSiphonMindActionEvent>(OnSiphonMind);
        SubscribeLocalEvent<MindflayerComponent, MindflayerSiphonMindDoAfterEvent>(OnSiphonMindDoAfter);
        SubscribeLocalEvent<MindflayerComponent, MindflayerAddImplantEvent>(OnMindflayerImplant);
    }

    private bool ValidSiphonTarget(EntityUid uid)
    {
        return !HasComp<ActiveNPCComponent>(uid) && _mobState.IsAlive(uid);
    }

    private void OnSiphonMind(Entity<MindflayerComponent> ent, ref MindflayerSiphonMindActionEvent args)
    {
        if (!ValidSiphonTarget(args.Target))
            return;

        var doArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.SiphonDuration, new MindflayerSiphonMindDoAfterEvent(), ent, args.Target)
        {
            DistanceThreshold = 2f,
            Hidden = true,
            BreakOnHandChange = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnDropItem = true,
        };
        args.Handled = _doAfter.TryStartDoAfter(doArgs);
    }

    protected virtual void OnSiphonMindDoAfter(Entity<MindflayerComponent> ent, ref MindflayerSiphonMindDoAfterEvent args)
    {
        if (args.Args.Target is not { } target)
            return;

        _statusEffects.TryAddStatusEffect<CosmicEntropyDebuffComponent>(target, "EntropicDegen", TimeSpan.FromSeconds(1), false);
        args.Repeat = ValidSiphonTarget(target);
    }

    private void OnMindflayerImplant(Entity<MindflayerComponent> ent, ref MindflayerAddImplantEvent args)
    {
        _subdermalImplant.AddImplant(ent, args.Implant);
    }
}
