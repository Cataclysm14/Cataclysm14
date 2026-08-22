using System;
using System.Numerics;
using Content.Shared._Cataclysm14.Zombies.Smoker;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Utility;

namespace Content.Client._Cataclysm14.Zombies.Smoker;

/// <summary>
/// Installs the continuous Smoker tongue visual
/// </summary>
public sealed class SmokerTongueVisualSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private SmokerTongueOverlay? _overlay;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new SmokerTongueOverlay(EntityManager, _transform, _resourceCache);
        _overlayManager.AddOverlay(_overlay);
    }

    public override void Shutdown()
    {
        if (_overlay != null)
            _overlayManager.RemoveOverlay(_overlay);

        _overlay = null;
        base.Shutdown();
    }
}

/// <summary>
/// Draws the existing 32x32 tongue texture as one stretched and rotated world-space quad
/// </summary>
internal sealed class SmokerTongueOverlay : Overlay
{
    private static readonly ResPath TongueTexturePath =
        new("/Textures/_Cataclysm14/Effects/smoker_tongue.rsi/tongue.png");

    private readonly IEntityManager _entityManager;
    private readonly SharedTransformSystem _transform;
    private readonly Texture _tongueTexture;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public SmokerTongueOverlay(
        IEntityManager entityManager,
        SharedTransformSystem transform,
        IResourceCache resourceCache)
    {
        _entityManager = entityManager;
        _transform = transform;
        _tongueTexture = resourceCache.GetResource<TextureResource>(TongueTexturePath);

        // Draw above world sprite so the tongue does not disappear under the victim
        ZIndex = 10;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var targetXformQuery = _entityManager.GetEntityQuery<TransformComponent>();
        var query = _entityManager.EntityQueryEnumerator<SmokerTongueActiveComponent, TransformComponent>();

        while (query.MoveNext(out _, out var tongue, out var smokerXform))
        {
            if (!tongue.Target.IsValid() ||
                !targetXformQuery.TryGetComponent(tongue.Target, out var targetXform) ||
                smokerXform.MapID != args.MapId ||
                targetXform.MapID != args.MapId)
            {
                continue;
            }

            var start = _transform.GetWorldPosition(smokerXform);
            var end = _transform.GetWorldPosition(targetXform);
            var delta = end - start;
            var length = delta.Length();

            if (length <= 0.001f)
                continue;

            var midpoint = (start + end) * 0.5f;
            var angle = new Angle(Math.Atan2(delta.Y, delta.X));

            // tongue.png is a 32x32 sprite whose painted tongue occupies the horizontal center
            // giving it a one tile high destination quad preserves that ~5 px visual thickness while
            // stretching only the horizontal axis to the exact Smoker --> victim distance
            var rect = Box2.FromDimensions(
                midpoint - new Vector2(length * 0.5f, 0.5f),
                new Vector2(length, 1f));
            var rotated = new Box2Rotated(rect, angle, midpoint);

            args.WorldHandle.DrawTextureRect(_tongueTexture, in rotated);
        }
    }
}
