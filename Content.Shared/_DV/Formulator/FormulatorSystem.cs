using System.Linq;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Chemistry;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.FixedPoint;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Storage;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Formulator;

public sealed class FormulatorSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FormulatorComponent, MapInitEvent>(OnFormulatorInit);
        SubscribeLocalEvent<FormulatorComponent, ComponentShutdown>(OnFormulatorShutdown);

        SubscribeLocalEvent<FormulatorComponent, EntInsertedIntoContainerMessage>(OnItemInserted, after: [typeof(SharedStorageSystem)]);
        SubscribeLocalEvent<FormulatorComponent, EntRemovedFromContainerMessage>(OnItemRemoved, after: [typeof(SharedStorageSystem)]);

        SubscribeLocalEvent<FormulatorComponent, AfterAutoHandleStateEvent>(OnFormulatorAfterState);
    }

    private void OnFormulatorInit(Entity<FormulatorComponent> ent, ref MapInitEvent args)
    {
        _itemSlots.AddItemSlot(ent, FormulatorComponent.ContainerSlotName, ent.Comp.ContainerSlot);
    }

    private void OnFormulatorShutdown(Entity<FormulatorComponent> ent, ref ComponentShutdown args)
    {
        _itemSlots.RemoveItemSlot(ent, ent.Comp.ContainerSlot);
    }

    private void OnItemInserted(Entity<FormulatorComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!_uiSystem.TryGetOpenUi<SharedFormulatorBUI>(ent.Owner, FormulatorUiKey.Key, out var bui))
            return;

        bui.UpdateInsertedContainer();
        bui.UpdateContents();
    }

    private void OnItemRemoved(Entity<FormulatorComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!_uiSystem.TryGetOpenUi<SharedFormulatorBUI>(ent.Owner, FormulatorUiKey.Key, out var bui))
            return;

        bui.UpdateInsertedContainer();
        bui.UpdateContents();
    }

    private void OnFormulatorAfterState(Entity<FormulatorComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!_uiSystem.TryGetOpenUi(ent.Owner, FormulatorUiKey.Key, out var bui))
            return;

        bui.Update();
    }

    public (EntityUid, ContainerInfo)? GetInsertedContainer(EntityUid uid)
    {
        var outputContainer = _itemSlots.GetItemOrNull(uid, FormulatorComponent.ContainerSlotName);
        if (outputContainer is not {} container)
            return null;

        if (BuildContainerInfo(container) is not {} info)
            return null;

        return (container, info);
    }

    private ContainerInfo? BuildContainerInfo(EntityUid container)
    {
        if (_solutionContainerSystem.TryGetFitsInDispenser(container, out _, out var solution))
        {
            return new ContainerInfo(Name(container), solution.Volume, solution.MaxVolume)
            {
                Reagents = solution.Contents
            };
        }

        if (!TryComp(container, out StorageComponent? storage))
            return null;

        var pills = storage.Container.ContainedEntities.Select((Func<EntityUid, (string, FixedPoint2 quantity)>) (pill =>
        {
            _solutionContainerSystem.TryGetSolution(pill, SharedChemMaster.PillSolutionName, out _, out var solution);
            var quantity = solution?.Volume ?? FixedPoint2.Zero;
            return (Name(pill), quantity);
        })).ToList();

        return new ContainerInfo(Name(container), _storage.GetCumulativeItemAreas((container, storage)), storage.Grid.GetArea())
        {
            Entities = pills
        };
    }

    public List<FormulaReagentQuantity> GetInventory(EntityUid uid)
    {
        if (!TryComp<StorageComponent>(uid, out var storage))
        {
            return new();
        }

        var items = new Dictionary<ProtoId<ReagentPrototype>, FixedPoint2>();

        foreach (var (storedContainer, storageLocation) in storage.StoredItems)
        {
            if (!_solutionContainerSystem.TryGetDrainableSolution(storedContainer, out _, out var soln))
            {
                continue;
            }

            foreach (var (reagent, quantity) in soln)
            {
                if (!items.ContainsKey(reagent.Prototype))
                {
                    items[reagent.Prototype] = FixedPoint2.Zero;
                }
                items[reagent.Prototype] += quantity;
            }
        }

        return items.OrderBy(it => it.Key).Select(it => new FormulaReagentQuantity() { Reagent = it.Key, Quantity = it.Value }).ToList();
    }
}
