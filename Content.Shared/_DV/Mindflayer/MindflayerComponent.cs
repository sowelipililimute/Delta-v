using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Mindflayer;

[RegisterComponent, NetworkedComponent]
public sealed partial class MindflayerComponent : Component;

public sealed partial class MindflayerShopActionEvent : InstantActionEvent;

public sealed partial class MindflayerAddSolutionToBloodstreamEvent : InstantActionEvent
{
    [DataField(required: true)]
    public Solution Solution;
}
