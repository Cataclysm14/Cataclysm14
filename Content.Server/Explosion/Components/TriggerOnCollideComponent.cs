namespace Content.Server.Explosion.Components
{
    [RegisterComponent]
    public sealed partial class TriggerOnCollideComponent : Component
    {
        [DataField("fixtureID", required: true)]
        public string FixtureID = String.Empty;

        /// <summary>
        ///     Doesn't trigger if the other colliding fixture is nonhard.
        /// </summary>
        [DataField("ignoreOtherNonHard")]
        public bool IgnoreOtherNonHard = true;

        /// <summary>
        ///     Ignores Fixtures for the purposes of triggering - Mono.
        /// </summary>
        [DataField("ignoreFixtureCheck")]
        public bool IgnoreFixtureCheck = false;
    }
}
