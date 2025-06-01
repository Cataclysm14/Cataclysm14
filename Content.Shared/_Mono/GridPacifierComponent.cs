using Robust.Shared.GameStates;

namespace Content.Shared._Mono;

/// <summary>
/// Component that applies Pacified status to all organic entities on a grid.
/// Entities with company affiliations matching the exempt companies will not be pacified.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class GridPacifierComponent : Component
{
    /// <summary>
    /// The list of entities that have been pacified by this component.
    /// </summary>
    [DataField]
    public HashSet<EntityUid> PacifiedEntities = new();

    /// <summary>
    /// Entities that are pending pacification with their entry timestamps.
    /// After 1 second, they will be moved to PacifiedEntities.
    /// </summary>
    [DataField]
    public Dictionary<EntityUid, TimeSpan> PendingEntities = new();

    /// <summary>
    /// First company name that is exempt from pacification.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ExemptCompany1 = string.Empty;

    /// <summary>
    /// Second company name that is exempt from pacification.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ExemptCompany2 = string.Empty;

    /// <summary>
    /// Third company name that is exempt from pacification.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ExemptCompany3 = string.Empty;

    /// <summary>
    /// The time when the next periodic update should occur
    /// </summary>
    [DataField, AutoPausedField]
    public TimeSpan NextUpdate;

    /// <summary>
    /// How frequently to check all entities on the grid for changes (in seconds)
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(5);
}
