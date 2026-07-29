using Content.Client._Cataclysm14.UserInterface.Systems;
using Content.Client.Gameplay;
using Content.Client._Shitmed.UserInterface.Systems.PartStatus.Widgets;
using Content.Shared._Shitmed.Targeting;
using Content.Client._Shitmed.Targeting;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.Player;
using Robust.Shared.Utility;
using Robust.Client.Graphics;


namespace Content.Client._Shitmed.UserInterface.Systems.PartStatus;

public sealed partial class PartStatusUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<TargetingSystem>
{
    [Dependency] private IEntityManager _entManager = default!;
    [Dependency] private IEntityNetworkManager _net = default!;
    private SpriteSystem _spriteSystem = default!;
    private TargetingComponent? _targetingComponent;
    private PartStatusControl? PartStatusControl => UIManager.GetActiveUIWidgetOrNull<PartStatusControl>();
    private CataclysmSidebar? CataclysmSidebar => UIManager.GetActiveUIWidgetOrNull<CataclysmSidebar>(); // Cataclysm14 Change

    public void OnSystemLoaded(TargetingSystem system)
    {
        system.PartStatusStartup += AddPartStatusControl;
        system.PartStatusShutdown += RemovePartStatusControl;
        system.PartStatusUpdate += UpdatePartStatusControl;
    }

    public void OnSystemUnloaded(TargetingSystem system)
    {
        system.PartStatusStartup -= AddPartStatusControl;
        system.PartStatusShutdown -= RemovePartStatusControl;
        system.PartStatusUpdate -= UpdatePartStatusControl;
    }

    public void OnStateEntered(GameplayState state)
    {
        if (PartStatusControl != null)
        {
            PartStatusControl.SetVisible(_targetingComponent != null);

            if (_targetingComponent != null)
                PartStatusControl.SetTextures(_targetingComponent.BodyStatus);
        }
        // Cataclysm14 SideBar
        if (CataclysmSidebar != null)
        {
            CataclysmSidebar.SetVisibleStatusDoll(_targetingComponent != null);

            if (_targetingComponent != null)
                CataclysmSidebar.SetStatusDoll(_targetingComponent.BodyStatus);
        }
        // Cataclysm14 SideBar
    }

    public void AddPartStatusControl(TargetingComponent component)
    {
        _targetingComponent = component;

        if (PartStatusControl != null)
        {
            PartStatusControl.SetVisible(_targetingComponent != null);

            if (_targetingComponent != null)
                PartStatusControl.SetTextures(_targetingComponent.BodyStatus);
        }
        // Cataclysm14 SideBar
        if (CataclysmSidebar != null)
        {
            CataclysmSidebar.SetVisibleStatusDoll(_targetingComponent != null);

            if (_targetingComponent != null)
                CataclysmSidebar.SetStatusDoll(_targetingComponent.BodyStatus);
        }
        // Cataclysm14 SideBar
    }

    public void RemovePartStatusControl()
    {
        if (PartStatusControl != null)
            PartStatusControl.SetVisible(false);

        // Cataclysm14 SideBar
        if (CataclysmSidebar != null)
            CataclysmSidebar.SetVisibleStatusDoll(false);
        // Cataclysm14 SideBar

        _targetingComponent = null;
    }

    public void UpdatePartStatusControl(TargetingComponent component)
    {
        if (PartStatusControl != null && _targetingComponent != null)
            PartStatusControl.SetTextures(_targetingComponent.BodyStatus);

        // Cataclysm14 SideBar
        if (CataclysmSidebar != null && _targetingComponent != null)
            CataclysmSidebar.SetStatusDoll(_targetingComponent.BodyStatus);
        // Cataclysm14 SideBar
    }

    public Texture GetTexture(SpriteSpecifier specifier)
    {
        if (_spriteSystem == null)
            _spriteSystem = _entManager.System<SpriteSystem>();

        return _spriteSystem.Frame0(specifier);
    }
}
