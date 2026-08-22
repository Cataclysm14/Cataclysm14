using Robust.Shared.GameStates;

namespace Content.Shared._Cataclysm14.Zombies.Smoker;

/// <summary>
/// Networked marker present on a Smoker only while its tongue is attached to a victim
/// The target is networked so the client can render a continuous, subtile tongue directly
/// between the Smoker and victim instead of using tile based Beam entities
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SmokerTongueActiveComponent : Component
{
    [AutoNetworkedField]
    public EntityUid Target;
}
