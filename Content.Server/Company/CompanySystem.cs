using Content.Shared.Company;
using Content.Shared.GameTicking;
using Content.Shared.Preferences.Loadouts;

namespace Content.Server.Company;

/// <summary>
/// System that adds the CompanyComponent to players when they join the round.
/// </summary>
public sealed class CompanySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }
    
    // When a player spawns, add the CompanyComponent with their selected company
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        // Get their selected company from their profile
        var company = args.Profile.Company;
        
        // Add the company component
        var companyComp = EnsureComp<CompanyComponent>(args.Mob);
        companyComp.Company = company;
        
        // Apply any company-specific effects if needed
        // (none for now, but can be extended)
        
        Dirty(args.Mob, companyComp);
    }
} 