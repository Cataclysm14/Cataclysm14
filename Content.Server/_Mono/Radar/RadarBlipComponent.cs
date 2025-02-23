namespace Content.Server._Mono.Radar;

/// <summary>
/// These handle objects which should be represented by radar blips.
/// </summary>
[RegisterComponent]
public sealed partial class RadarBlipComponent : Component
{
    /// <summary>
    /// Color of the blip.
    /// </summary>
    [DataField]
    public Color Color = Color.Red;

    /// <summary>
    /// Scale of the blip.
    /// </summary>
    [DataField]
    public float Scale = 1;

    /// <summary>
    /// Whether this blip should be shown even when parented to a grid.
    /// </summary>
    [DataField]
    public bool RequireNoGrid = false;

    [DataField]
    public bool Enabled = true;
} 