using Content.Server.Body.Systems;
using Content.Server.Store.Systems;
using Content.Shared._DV.Mindflayer;

namespace Content.Server._DV.Mindflayer;

public sealed class MindflayerSystem : SharedMindflayerSystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly StoreSystem _store = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindflayerComponent, MindflayerShopActionEvent>(OnShopAction);
        SubscribeLocalEvent<MindflayerComponent, MindflayerAddSolutionToBloodstreamEvent>(OnAddSolutionToBloodstream);
    }

    private void OnShopAction(Entity<MindflayerComponent> ent, ref MindflayerShopActionEvent args)
    {
        _store.ToggleUi(args.Performer, ent);
    }

    private void OnAddSolutionToBloodstream(Entity<MindflayerComponent> ent, ref MindflayerAddSolutionToBloodstreamEvent args)
    {
        _bloodstream.TryAddToChemicals(ent, args.Solution);
    }
}
