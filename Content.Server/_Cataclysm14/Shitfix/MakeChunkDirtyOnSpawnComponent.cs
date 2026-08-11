namespace Content.Server._Cataclysm14.Shitfix;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class MakeChunkDirtyOnSpawnComponent : Component
{
    [DataField]
    public int Left { get; set; } = -2;

    [DataField]
    public int Right { get; set; } = 2;

    [DataField]
    public int Top { get; set; } = 2;

    [DataField]
    public int Bottom { get; set; } = -2;
}
