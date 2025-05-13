using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Mindflayer;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MindflayerCamfectingBugComponent : Component
{
    [DataField]
    public EntProtoId ViewCamerasAction = "ActionMindFlayerViewCameras";

    [DataField, AutoNetworkedField]
    public EntityUid? ViewCamerasActionEntity;

    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> TrackedEntities = new();

    [DataField, AutoNetworkedField]
    public EntityUid? CurrentlyViewing;

    [DataField, AutoNetworkedField]
    public int MaximumCameras = 6;
}

public sealed partial class MindflayerCamfectingBugEvent : EntityTargetActionEvent
{
    [DataField]
    public TimeSpan HackingDuration = TimeSpan.FromSeconds(5);
}

[Serializable, NetSerializable]
public sealed partial class MindflayerCamfectingBugDoAfterEvent : SimpleDoAfterEvent;

public sealed partial class MindflayerOpenHackedCamerasEvent : InstantActionEvent;
