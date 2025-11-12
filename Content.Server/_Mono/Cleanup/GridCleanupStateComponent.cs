namespace Content.Server._Mono.Cleanup;

/// <summary>
///     Stores at which time will we have to be still meeting cleanup conditions for this grid to get cleaned up.
/// </summary>
[RegisterComponent]
public sealed partial class CleanupImmuneComponent : Component
{
    [ViewVariables]
    TimeSpan? CleanupAtTime = null;
}
