namespace Content.Shared._Mono;

/// <summary>
/// Component that applies NoHack and NoDeconstruct to entities with Door and/or VendingMachine components on a grid.
/// </summary>
[RegisterComponent]
public sealed partial class GridRaiderComponent : Component
{
    /// <summary>
    /// The list of entities that have been given NoHack and NoDeconstruct by this component.
    /// </summary>
    [DataField]
    public HashSet<EntityUid> ProtectedEntities = new();

    /// <summary>
    /// Whether to protect entities with Door components.
    /// </summary>
    [DataField]
    public bool ProtectDoors = true;

    /// <summary>
    /// Whether to protect entities with VendingMachine components.
    /// </summary>
    [DataField]
    public bool ProtectVendingMachines = true;

    /// <summary>
    /// The server time at which the next settings check will occur.
    /// </summary>
    [DataField]
    public TimeSpan NextSettingsCheck = TimeSpan.Zero;

    /// <summary>
    /// How often to check for setting changes (5 seconds).
    /// </summary>
    [DataField]
    public TimeSpan SettingsCheckInterval = TimeSpan.FromSeconds(5);
}
