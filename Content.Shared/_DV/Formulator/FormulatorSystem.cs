using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Administration.Logs;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Chemistry;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Labels.EntitySystems;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Storage;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._DV.Formulator;

public sealed class FormulatorSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedLabelSystem _label = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
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

        Subs.BuiEvents<FormulatorComponent>(FormulatorUiKey.Key,
            subr =>
            {
                subr.Event<FormulatorMessageFormulate>(OnMessageFormulate);
            });
    }

    static private Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> RequirementsDict(List<FormulaReagentQuantity> amounts, int quantity)
    {
        var requirements = new Dictionary<ProtoId<ReagentPrototype>, FixedPoint2>();
        foreach (var element in amounts)
        {
            if (!requirements.ContainsKey(element.Reagent))
            {
                requirements[element.Reagent] = FixedPoint2.Zero;
            }
            requirements[element.Reagent] += element.Quantity * quantity;
        }
        return requirements;
    }

    private bool HasFormulaRequirements(EntityUid uid, Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> requirements)
    {
        var contained = GetInventoryDict(uid);
        foreach (var (reagent, amount) in requirements)
        {
            if (!contained.ContainsKey(reagent))
            {
                return false;
            }
            if (contained[reagent] < amount)
            {
                return false;
            }
        }
        return true;
    }

    private Solution? ExtractRequiredSolution(EntityUid uid, List<FormulaReagentQuantity> amounts, int quantity)
    {
        var requirements = RequirementsDict(amounts, quantity);
        var requiredArray = requirements.Keys.Select(key => key.ToString()).ToArray();

        if (!HasFormulaRequirements(uid, requirements))
            return null;

        if (!TryComp<StorageComponent>(uid, out var storage))
            return null;

        var items = new Dictionary<ProtoId<ReagentPrototype>, FixedPoint2>();
        var requiredSolution = new Solution();

        foreach (var (storedContainer, storageLocation) in storage.StoredItems)
        {
            if (!_solutionContainer.TryGetDrainableSolution(storedContainer, out var soln, out var solution))
            {
                continue;
            }

            var solutionOfInterest = _solutionContainer.SplitSolutionWithOnly(soln!.Value, solution.Volume, requiredArray);
            var reagentsOfInterest = solutionOfInterest.Select(it => it.Reagent).ToList(); // avoid concurrent modified exception

            foreach (var reagent in reagentsOfInterest)
            {
                if (!requirements.ContainsKey(reagent.Prototype))
                {
                    continue;
                }

                var removed = solutionOfInterest.RemoveReagent(reagent, requirements[reagent.Prototype]);
                requirements[reagent.Prototype] -= removed;
                requiredSolution.AddReagent(reagent, removed);

                DebugTools.Assert(requirements[reagent.Prototype] >= 0);
                if (requirements[reagent.Prototype] == 0)
                {
                    requirements.Remove(reagent.Prototype);
                }
            }

            var ok = _solutionContainer.TryAddSolution(soln!.Value, solutionOfInterest);
            DebugTools.Assert(ok);
        }

        DebugTools.Assert(requirements.Count == 0);
        return requiredSolution;
    }

    private void OnMessageFormulate(Entity<FormulatorComponent> ent, ref FormulatorMessageFormulate args)
    {
        var outputContainer = _itemSlots.GetItemOrNull(ent, FormulatorComponent.ContainerSlotName);
        if (outputContainer is not {} container)
            return;

        if (args.Formula is PillFormula pills)
        {
            if (!TryComp(container, out StorageComponent? storage))
                return;

            if (pills.PillQuantity == 0 || !_storage.HasSpace((container, storage)))
                return;

            var dosage = pills.ReagentQuantities.Sum(reagent => reagent.Quantity.Int());
            if (dosage == 0 || dosage > ent.Comp.MaxPillDosage)
                return;

            if (pills.Name.Length > SharedChemMaster.LabelMaxLength)
                return;

            if (ExtractRequiredSolution(ent, pills.ReagentQuantities, pills.PillQuantity) is not {} requiredSolution)
                return;

            _label.Label(container, pills.Name);
            if (!_netManager.IsClient) for (int i = 0; i < pills.PillQuantity; i++)
            {
                var item = Spawn(FormulatorComponent.PillPrototypeId, Transform(container).Coordinates);
                _storage.Insert(container, item, out _, user: args.Actor, storage);
                _label.Label(item, pills.Name);

                _solutionContainer.EnsureSolutionEntity(item, SharedChemMaster.PillSolutionName, out var itemSolution, dosage);
                if (!itemSolution.HasValue)
                    return;

                _solutionContainer.TryAddSolution(itemSolution.Value, requiredSolution.SplitSolution(dosage));

                var pill = EnsureComp<PillComponent>(item);
                pill.PillType = pills.PillType;
                Dirty(item, pill);

                _adminLogger.Add(LogType.Action, LogImpact.Low,
                    $"{ToPrettyString(args.Actor):user} printed {ToPrettyString(item):pill} {SharedSolutionContainerSystem.ToPrettyString(itemSolution.Value.Comp.Solution)}");
            }
        }
        else if (args.Formula is BottleFormula bottle)
        {
            if (!TryGetInsertedSolution(container, out var soln, out var solution))
                return;

            var dosage = bottle.ReagentQuantities.Sum(reagent => reagent.Quantity.Int());
            if (dosage == 0 || dosage > solution.AvailableVolume)
                return;

            if (bottle.Name.Length > SharedChemMaster.LabelMaxLength)
                return;

            if (ExtractRequiredSolution(ent, bottle.ReagentQuantities, 1) is not {} requiredSolution)
                return;

            _label.Label(container, bottle.Name);
            _solutionContainer.TryAddSolution(soln.Value, requiredSolution);

            // Log bottle creation by a user
            _adminLogger.Add(LogType.Action, LogImpact.Low,
                $"{ToPrettyString(args.Actor):user} bottled {ToPrettyString(container):bottle} {SharedSolutionContainerSystem.ToPrettyString(solution)}");
        }

        UpdateUi(ent);
        ClickSound(ent, args.Actor);
    }

    private bool TryGetInsertedSolution(EntityUid container, [NotNullWhen(true)] out Entity<SolutionComponent>? entity, [NotNullWhen(true)] out Solution? solution)
    {
        if (_solutionContainer.TryGetSolution(container, SharedChemMaster.BottleSolutionName, out entity, out solution))
            return true;

        if (_solutionContainer.TryGetFitsInDispenser(container, out entity, out solution))
            return true;

        return false;
    }

    private void OnFormulatorInit(Entity<FormulatorComponent> ent, ref MapInitEvent args)
    {
        _itemSlots.AddItemSlot(ent, FormulatorComponent.ContainerSlotName, ent.Comp.ContainerSlot);
    }

    private void OnFormulatorShutdown(Entity<FormulatorComponent> ent, ref ComponentShutdown args)
    {
        _itemSlots.RemoveItemSlot(ent, ent.Comp.ContainerSlot);
    }

    private void ClickSound(Entity<FormulatorComponent> ent, EntityUid? user)
    {
        _audio.PlayPredicted(ent.Comp.ClickSound, ent, user, AudioParams.Default.WithVolume(-2f));
    }

    private void UpdateUi(Entity<FormulatorComponent> ent)
    {
        if (!_uiSystem.TryGetOpenUi(ent.Owner, FormulatorUiKey.Key, out var bui))
            return;

        bui.Update();
    }

    private void OnItemInserted(Entity<FormulatorComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        UpdateUi(ent);
    }

    private void OnItemRemoved(Entity<FormulatorComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        UpdateUi(ent);
    }

    private void OnFormulatorAfterState(Entity<FormulatorComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateUi(ent);
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
        if (TryGetInsertedSolution(container, out _, out var solution))
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
            _solutionContainer.TryGetSolution(pill, SharedChemMaster.PillSolutionName, out _, out var solution);
            var quantity = solution?.Volume ?? FixedPoint2.Zero;
            return (Name(pill), quantity);
        })).ToList();

        return new ContainerInfo(Name(container), _storage.GetCumulativeItemAreas((container, storage)), storage.Grid.GetArea())
        {
            Entities = pills
        };
    }

    private Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> GetInventoryDict(EntityUid uid)
    {
        if (!TryComp<StorageComponent>(uid, out var storage))
        {
            return new();
        }

        var items = new Dictionary<ProtoId<ReagentPrototype>, FixedPoint2>();

        foreach (var (storedContainer, storageLocation) in storage.StoredItems)
        {
            if (!_solutionContainer.TryGetDrainableSolution(storedContainer, out _, out var soln))
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

        return items;
    }

    public List<FormulaReagentQuantity> GetInventory(EntityUid uid)
    {
        var items = GetInventoryDict(uid);
        return items.OrderBy(it => it.Key).Select(it => new FormulaReagentQuantity() { Reagent = it.Key, Quantity = it.Value }).ToList();
    }
}
