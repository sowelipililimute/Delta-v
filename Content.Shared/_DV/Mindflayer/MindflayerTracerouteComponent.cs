using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Mindflayer;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MindflayerTraceRouteComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? TaggedEntity;
}

public sealed partial class MindflayerTraceRouteActionEvent : EntityTargetActionEvent;

[RegisterComponent, NetworkedComponent]
public sealed partial class MindflayerTraceRouteTargetComponent : Component;
