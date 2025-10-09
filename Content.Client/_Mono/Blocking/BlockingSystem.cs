using Content.Client.Interactable.Components;
using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Blocking;
using Content.Shared.Toggleable;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client._Mono.Blocking;

public sealed class BlockingSystemVisuals : EntitySystem
{
    [Dependency] private readonly BlockingSystem _blocking = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!; // Goobstation

    private ShaderInstance _shader = default!;

    public override void Initialize()
    {
        base.Initialize();

        _shader = _protoMan.Index<ShaderPrototype>("ShieldingOutline").InstanceUnique();
        SubscribeLocalEvent<BlockingComponent, ComponentStartup>(OnStartup);
    }

    private void SetShader(EntityUid uid, bool enabled, BlockingComponent? component = null, SpriteComponent? sprite = null)
    {
        if (!Resolve(uid, ref component, ref sprite, false))
            return;
        var user = component.User;

        sprite.PostShader = enabled ? _shader : null;
        sprite.GetScreenTexture = enabled;
        sprite.RaiseShaderEvent = enabled;
    }
    private void OnStartup(EntityUid uid, BlockingComponent component, ComponentStartup args)
    {
        SetShader(uid, true, component);
    }

    private void OnShutdown(EntityUid uid, BlockingComponent component, ComponentShutdown args)
    {
        if (!Terminating(uid))
            SetShader(uid, false, component);
    }
}
