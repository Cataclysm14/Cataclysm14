using Robust.Shared.Map;

namespace Content.Shared._Cataclysm.Projectiles;

[RegisterComponent]
public sealed partial class ProjectileDamageFalloffComponent : Component
{
	// These are baseline values if not defined by YAML type. Weapons without the component won't use this btw
    [DataField("falloffStart")]
    public float FalloffStart = 2f; //Damage falloff starts at 2 tiles

    [DataField("falloffPerTile")]
    public float FalloffPerTile = 0.1f; //Loses 10% each tile traveled

    [DataField("minimumMultiplier")]
    public float MinimumMultiplier = 0.5f; //Minimum the damage will drop to, in this case; 50% is cap

    [DataField("stepByTile")]
    public bool StepByTile; //Whether to calculate strictly by tile or not; set false to have fractional and more precise

    public MapCoordinates Origin;

    public bool OriginInitialized;
}