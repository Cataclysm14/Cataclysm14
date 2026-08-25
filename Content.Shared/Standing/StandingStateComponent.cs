using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Standing;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StandingStateComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public SoundSpecifier DownSound { get; private set; } = new SoundCollectionSpecifier("BodyFall");

    // WD EDIT START
    [DataField, AutoNetworkedField]
    public StandingState CurrentState { get; set; } = StandingState.Standing;
    // WD EDIT END

    /// <summary>
    /// M̶o̶n̶o̶:̶ ̶C̶h̶a̶n̶c̶e̶ ̶f̶o̶r̶ ̶a̶ ̶p̶r̶o̶j̶e̶c̶t̶i̶l̶e̶ ̶t̶o̶ ̶m̶i̶s̶s̶ ̶t̶h̶e̶ ̶t̶a̶r̶g̶e̶t̶ ̶i̶f̶ ̶t̶h̶e̶y̶ ̶a̶r̶e̶ ̶n̶o̶t̶ ̶s̶t̶a̶n̶d̶i̶n̶g̶
	/// Cata14 Tweak: This shit's actually kinda aids at 50%, so 0f = 0% chance, fuck this ghey shit holy
    /// </summary>
    [DataField, AutoNetworkedField]
    public float LyingDodgeChance = 0f;

    /// <summary>
    /// Mono: Range between shooter and target at where projectiles will always hit
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HitRange = 3f;

    [DataField, AutoNetworkedField]
    public bool Standing { get; set; } = true;

    /// <summary>
    ///     List of fixtures that had their collision mask changed when the entity was downed.
    ///     Required for re-adding the collision mask.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<string> ChangedFixtures = new();
}
// WD EDIT START
public enum StandingState
{
    Lying,
    GettingUp,
    Standing,
}
// WD EDIT END
