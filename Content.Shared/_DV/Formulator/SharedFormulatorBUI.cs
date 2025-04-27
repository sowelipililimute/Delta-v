namespace Content.Shared._DV.Formulator;

public abstract class SharedFormulatorBUI : BoundUserInterface
{
    public abstract void UpdateFormulas();
    public abstract void UpdateInsertedContainer();
    public abstract void UpdateContents();

    public SharedFormulatorBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }
}
