using Content.Shared.Chemistry.Reagent;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Formulator;

[Serializable, NetSerializable, DataRecord]
public sealed class FormulaReagentQuantity
{
    public ProtoId<ReagentPrototype> Reagent;
    public FixedPoint2 Quantity;
}

[Serializable, NetSerializable, DataRecord]
public abstract class Formula
{
    public string Name = string.Empty;
    public List<FormulaReagentQuantity> ReagentQuantities = new();
}

[Serializable, NetSerializable, DataRecord]
public sealed class PillFormula : Formula
{
    public int PillQuantity;
    public uint PillType = 0;
}

[Serializable, NetSerializable, DataRecord]
public sealed class BottleFormula : Formula
{
}

[Serializable, NetSerializable, DataRecord]
public sealed class InjectorFormula : Formula
{
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true, fieldDeltas: true)]
public sealed partial class FormulatorComponent : Component
{
    [DataField]
    public ItemSlot ContainerSlot = new();

    public const string ContainerSlotName = "container_slot";

    [DataField, AutoNetworkedField]
    public List<Formula> Formulas = new()
    {
        new PillFormula()
            {
                Name = "soretizone 10u",
                ReagentQuantities = new() {
                    new() { Reagent = "Soretizone", Quantity = 10 }
                },
                PillType = 1,
                PillQuantity = 10,
            },
        new BottleFormula()
            {
                Name = "revivopine 10u",
                ReagentQuantities = new() {
                    new() { Reagent = "Revivopine", Quantity = 10 }
                }
            },
        new BottleFormula()
            {
                Name = "burn mix 20u",
                ReagentQuantities = new() {
                    new() { Reagent = "Oxandrolone", Quantity = 10 },
                    new() { Reagent = "Dermaline", Quantity = 10 }
                }
            },
    };
}

[Serializable, NetSerializable]
public enum FormulatorUiKey
{
    Key
}
