using Content.Shared.DoAfter;
using Content.Shared.Actions;

namespace Content.Shared._DV.Mindflayer;

public sealed class MindflayerCamfectingBugSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindflayerCamfectingBugComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<MindflayerComponent, MindflayerCamfectingBugEvent>(OnCamfectingBug);
        SubscribeLocalEvent<MindflayerCamfectingBugComponent, MindflayerCamfectingBugDoAfterEvent>(OnCamfectingBugDoAfter);
        SubscribeLocalEvent<MindflayerCamfectingBugComponent, MindflayerOpenHackedCamerasEvent>(OnOpenHackedCameras);
    }

    private void OnComponentInit(Entity<MindflayerCamfectingBugComponent> ent, ref ComponentInit args)
    {
        _actions.AddAction(ent, ref ent.Comp.ViewCamerasActionEntity, ent.Comp.ViewCamerasAction);
        Dirty(ent);
    }

    private void OnCamfectingBug(Entity<MindflayerComponent> ent, ref MindflayerCamfectingBugEvent args)
    {
        EnsureComp<MindflayerCamfectingBugComponent>(ent);

        var doArgs = new DoAfterArgs(EntityManager, ent, args.HackingDuration, new MindflayerCamfectingBugDoAfterEvent(), ent, args.Target)
        {
            DistanceThreshold = 2f,
            BreakOnHandChange = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnDropItem = true,
        };
        args.Handled = _doAfter.TryStartDoAfter(doArgs);
    }

    private void OnCamfectingBugDoAfter(Entity<MindflayerCamfectingBugComponent> ent, ref MindflayerCamfectingBugDoAfterEvent args)
    {
        if (args.Args.Target is not { } target)
            return;

        ent.Comp.TrackedEntities.Add(target);
        Dirty(ent);
    }

    private void OnOpenHackedCameras(Entity<MindflayerCamfectingBugComponent> ent, ref MindflayerOpenHackedCamerasEvent args)
    {
    }
}
