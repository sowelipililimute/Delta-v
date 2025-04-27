using Content.Shared._DV.Formulator;
using Robust.Client.UserInterface;

namespace Content.Client._DV.Formulator;

public sealed class FormulatorBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    private readonly FormulatorSystem _formulator;

    private FormulatorWindow? _window;

    public FormulatorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
        _formulator = _entMan.System<FormulatorSystem>();
        Update();
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<FormulatorWindow>();
        _window.OnFormulate += OnFormulate;

        Update();
    }

    private void OnFormulate(Formula formula)
    {
        SendPredictedMessage(new FormulatorMessageFormulate(formula));
    }

    public override void Update()
    {
        base.Update();

        if (_window is null)
            return;

        UpdateFormulas();
        UpdateInsertedContainer();
        UpdateContents();
    }

    public void UpdateFormulas()
    {
        if (_window is null)
            return;

        if (!_entMan.TryGetComponent<FormulatorComponent>(Owner, out var formulator))
            return;

        _window.UpdateFormulas(formulator.Formulas);
    }

    public void UpdateInsertedContainer()
    {
        if (_window is null)
            return;

        if (_formulator.GetInsertedContainer(Owner) is not (var owner, var container))
        {
            _window.ClearContainerInfo();
        }
        else
        {
            _window.UpdateContainerInfo(owner, container);
        }
    }

    public void UpdateContents()
    {
        if (_window is null)
            return;

        _window.UpdateContents(_formulator.GetInventory(Owner));
    }
}
