using Content.Client._Cataclysm14.UserInterface.Systems;
using Content.Client.Gameplay;
using Content.Client._Shitmed.UserInterface.Systems.Targeting.Widgets;
using Content.Shared._Shitmed.Targeting;
using Content.Client._Shitmed.Targeting;
using Content.Shared._Shitmed.Targeting.Events;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.Player;

namespace Content.Client._Shitmed.UserInterface.Systems.Targeting;

public sealed partial class TargetingUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<TargetingSystem>
{
    [Dependency] private IEntityManager _entManager = default!;
    [Dependency] private IEntityNetworkManager _net = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    private TargetingComponent? _targetingComponent;
    private TargetingControl? TargetingControl => UIManager.GetActiveUIWidgetOrNull<TargetingControl>();
    private CataclysmSidebar? CataclysmSidebar => UIManager.GetActiveUIWidgetOrNull<CataclysmSidebar>();

    public void OnSystemLoaded(TargetingSystem system)
    {
        system.TargetingStartup += AddTargetingControl;
        system.TargetingShutdown += RemoveTargetingControl;
        system.TargetChange += CycleTarget;
    }

    public void OnSystemUnloaded(TargetingSystem system)
    {
        system.TargetingStartup -= AddTargetingControl;
        system.TargetingShutdown -= RemoveTargetingControl;
        system.TargetChange -= CycleTarget;
    }

    public void OnStateEntered(GameplayState state)
    {
        if (TargetingControl == null && CataclysmSidebar == null) // Cataclysm14
            return;

        TargetingControl?.SetTargetDollVisible(_targetingComponent != null);
        CataclysmSidebar?.SetTargetDollVisible(_targetingComponent != null); // Cataclysm14

        // cataclysm14 begin
        if (_targetingComponent != null)
        {
            TargetingControl?.SetBodyPartsVisible(_targetingComponent.Target);
            CataclysmSidebar?.SetBodyPartsVisible(_targetingComponent.Target);
        }
        // cataclysm14 end
    }

    public void AddTargetingControl(TargetingComponent component)
    {
        _targetingComponent = component;

        // cataclysm14 begin
        if (TargetingControl != null || CataclysmSidebar != null)
        {
            TargetingControl?.SetTargetDollVisible(_targetingComponent != null);
            CataclysmSidebar?.SetTargetDollVisible(_targetingComponent != null);

            if (_targetingComponent != null)
            {
                TargetingControl?.SetBodyPartsVisible(_targetingComponent.Target);
                CataclysmSidebar?.SetBodyPartsVisible(_targetingComponent.Target);
            }
        }
        // cataclysm14 end
    }

    public void RemoveTargetingControl()
    {
        TargetingControl?.SetTargetDollVisible(false); // Cataclysm14
        CataclysmSidebar?.SetTargetDollVisible(false); // Cataclysm14

        _targetingComponent = null;
    }

    public void CycleTarget(TargetBodyPart bodyPart)
    {
        if (_playerManager.LocalEntity is not { } user
            || _entManager.GetComponent<TargetingComponent>(user) is not { } targetingComponent
            || (TargetingControl == null && CataclysmSidebar == null)) // Cataclysm14
            return;

        var player = _entManager.GetNetEntity(user);
        if (bodyPart != targetingComponent.Target)
        {
            var msg = new TargetChangeEvent(player, bodyPart);
            _net.SendSystemNetworkMessage(msg);
            TargetingControl?.SetBodyPartsVisible(bodyPart);
            CataclysmSidebar?.SetBodyPartsVisible(bodyPart); // Cataclysm14
        }
    }
}
