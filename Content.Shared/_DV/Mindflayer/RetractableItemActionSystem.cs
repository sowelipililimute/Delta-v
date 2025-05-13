using Content.Shared.Containers.ItemSlots;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Mindflayer;

public sealed class RetractableItemActionSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RetractableItemActionComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<RetractableItemActionComponent, RetractItemActionEvent>(OnRetractItem);
        SubscribeLocalEvent<RetractableItemActiveItemComponent, ContainerGettingRemovedAttemptEvent>(OnDropAttempt);
    }

    private void OnComponentInit(Entity<RetractableItemActionComponent> ent, ref ComponentInit args)
    {
        _itemSlots.AddItemSlot(ent, RetractableItemActionComponent.SlotName, ent.Comp.Slot);
    }

    private EntityUid? ContainedItem(Entity<RetractableItemActionComponent> ent)
    {
        if (ent.Comp.Item is { } item)
            return item;

        ent.Comp.Item = _itemSlots.GetItemOrNull(ent, RetractableItemActionComponent.SlotName);
        return ent.Comp.Item;
    }

    private void OnRetractItem(Entity<RetractableItemActionComponent> ent, ref RetractItemActionEvent args)
    {
        if (ContainedItem(ent) is not { } item)
            return;

        if (RemComp<RetractableItemActiveItemComponent>(item))
        {
            if (!_itemSlots.TryInsert(ent, RetractableItemActionComponent.SlotName, item, args.Performer))
                AddComp<RetractableItemActiveItemComponent>(item);
            else
                args.Handled = true;

            return;
        }

        if (!_hands.TryGetEmptyHand(args.Performer, out var hand))
            return;

        if (!_hands.TryPickup(args.Performer, item, hand))
            return;

        EnsureComp<RetractableItemActiveItemComponent>(item);
        args.Handled = true;
    }

    private void OnDropAttempt(Entity<RetractableItemActiveItemComponent> ent, ref ContainerGettingRemovedAttemptEvent args)
    {
        args.Cancel();
    }
}
