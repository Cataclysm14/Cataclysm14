namespace Content.Shared._Mono;

/// <summary>
/// Component that applies NoHack and NoDeconstruct to all entities with Door components on a grid.
/// </summary>
[RegisterComponent]
public sealed partial class GridRaiderComponent : Component
{
    /// <summary>
    /// The list of entities that have been given NoHack and NoDeconstruct by this component.
    /// </summary>
    [DataField]
    public HashSet<EntityUid> ProtectedEntities = new();
}
