using Robust.Shared.GameStates;
using Content.Shared.Preferences.Loadouts;

namespace Content.Shared.Company;

/// <summary>
/// This component stores the company a player belongs to, as chosen in their character loadout.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CompanyComponent : Component
{
    /// <summary>
    /// The company affiliation of this entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public CompanyAffiliation Company = CompanyAffiliation.Neutral;
} 