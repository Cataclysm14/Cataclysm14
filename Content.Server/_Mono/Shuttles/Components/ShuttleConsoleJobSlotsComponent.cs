using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._Mono.Shuttles.Components;

/// <summary>
/// Component that stores the original job slot counts for a shuttle grid.
/// </summary>
[RegisterComponent]
public sealed partial class ShuttleConsoleJobSlotsComponent : Component
{
    /// <summary>
    /// Dictionary storing the original job slot counts before power was lost.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<JobPrototype>, int?> SavedJobSlots = new();

    /// <summary>
    /// The station entity that this grid's job slots belong to.
    /// </summary>
    [DataField]
    public EntityUid? OwningStation;
}
