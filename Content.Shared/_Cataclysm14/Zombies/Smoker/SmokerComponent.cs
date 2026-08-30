using System;
using Robust.Shared.Audio;

namespace Content.Shared._Cataclysm14.Zombies.Smoker;

/// <summary>
/// Configuration and runtime for Smoker tongue
/// Fields below are server authoritative; the component itself is YAML configured
/// </summary>
[RegisterComponent]
public sealed partial class SmokerComponent : Component
{
    [DataField]
    public float TongueMinRange = 2f;

    [DataField]
    public float TongueMaxRange = 10f;

    [DataField]
    public float TongueBreakRange = 10f;

    [DataField]
    public float TonguePullSpeed = 5f;

    [DataField]
    public float TongueStopDistance = 0.85f;

    [DataField]
    public float StrugglePressInterval = 0.06f;

    [DataField]
    public float StruggleRequiredProgress = 100f;

    [DataField]
    public float StruggleProgressPerPress = 6f;

    [DataField]
    public float StruggleDecayPerSecond = 9f;

    [DataField]
    public float StruggleNetworkUpdateInterval = 0.10f;

    [DataField]
    public float SmokeDeathDelay = 5f;

    [DataField]
    public string SmokeEmitterPrototype = "SmokerSmokeEmitter";

    [DataField]
    public string TongueCuffPrototype = "SmokerTongueCuffs";

    [DataField]
    public SoundSpecifier CoughSound = new SoundCollectionSpecifier("SmokerLurker");

    [DataField]
    public SoundSpecifier AlertSound = new SoundCollectionSpecifier("SmokerAlert");

    [DataField]
    public SoundSpecifier LaunchTongueSound = new SoundPathSpecifier(
        "/Audio/_Cataclysm14/Zombies/Smoker/smoker_launchtongue_01.ogg");

    public EntityUid? SmokeEmitter;
    public EntityUid? TongueTarget;
    public TimeSpan? StopSmokeAt;
    public bool DeathHandled;

    public bool TongueArmed;
    public TimeSpan TongueReadyAt;
}
