using System;
using Robust.Shared.GameStates;

namespace Content.Shared._Cataclysm14.Zombies.Smoker;

/// <summary>
/// Runtime placed on a victim while a Smoker has them hooked
/// Escape progress is networked so the caught victim can see the struggle bar
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SmokerTonguedComponent : Component
{
    // Serverside state
    public EntityUid? Smoker;
    public float ProgressPerPress = 6f;
    public TimeSpan NextAcceptedPress;
    public TimeSpan NextProgressNetworkUpdate;

    // Serverside reference to the temporary tongue cuffs
    // Keeping this separate means releasing the tongue never removes real handcuffs
    public EntityUid? TongueCuffs;

    [AutoNetworkedField]
    public float EscapeProgress;

    [AutoNetworkedField]
    public float RequiredProgress = 100f;
}
