using Content.Shared.Actions;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Mindflayer;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RetractableItemActionComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Item;

    [DataField]
    public ItemSlot Slot = new();

    public const string SlotName = "retractable_slot";
}

[RegisterComponent, NetworkedComponent]
public sealed partial class RetractableItemActiveItemComponent : Component;

public sealed partial class RetractItemActionEvent : InstantActionEvent;
