using Robust.Shared.Audio;

namespace Content.Server._Mono.CombatMusic;

/// <summary>
/// Component that tracks combat music state for a grid.
/// </summary>
[RegisterComponent]
public sealed partial class CombatMusicComponent : Component
{
    /// <summary>
    /// The time when the last shot was fired from any gunnery control on this grid.
    /// </summary>
    [DataField]
    public TimeSpan LastFiringTime = TimeSpan.Zero;

    /// <summary>
    /// Whether combat music is currently playing.
    /// </summary>
    [DataField]
    public bool MusicPlaying;

    /// <summary>
    /// The audio stream entity for the combat music.
    /// </summary>
    [ViewVariables]
    public EntityUid? MusicStream = null;

    /// <summary>
    /// The sounds to play for combat music. One will be randomly selected each time music starts.
    /// </summary>
    [DataField]
    public List<SoundSpecifier> CombatMusicSounds = new()
    {
        new SoundPathSpecifier("/Audio/Expedition/deadline.ogg"),
        new SoundPathSpecifier("/Audio/Expedition/tension_session.ogg"),
    };

    /// <summary>
    /// How long to wait after the last shot before stopping the music (in seconds).
    /// </summary>
    [DataField]
    public float MusicTimeout = 15f;

    /// <summary>
    /// The volume of the combat music.
    /// </summary>
    [DataField]
    public float Volume = -5f;
}

