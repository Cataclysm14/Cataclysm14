namespace Content.Shared._Cataclysm14.Pulp;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class PulpableComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsPulped { get; set; } = false;
}
