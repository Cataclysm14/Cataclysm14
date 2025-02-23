using System.Numerics;
using Content.Shared._Mono.Radar;
using Content.Shared.Shuttles.Components;

namespace Content.Server._Mono.Radar;

public sealed partial class RadarBlipSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RequestBlipsEvent>(OnBlipsRequested);
    }

    private void OnBlipsRequested(RequestBlipsEvent ev, EntitySessionEventArgs args)
    {
        if (!TryGetEntity(ev.Radar, out var radarUid))
            return;

        if (!TryComp<RadarConsoleComponent>(radarUid, out var radar))
            return;

        var blips = AssembleBlipsReport((EntityUid)radarUid, radar);

        var giveEv = new GiveBlipsEvent(blips);
        RaiseNetworkEvent(giveEv, args.SenderSession);
    }

    private List<(Vector2, float, Color)> AssembleBlipsReport(EntityUid uid, RadarConsoleComponent? component = null)
    {
        var blips = new List<(Vector2, float, Color)>();

        if (Resolve(uid, ref component))
        {
            var blipQuery = EntityQueryEnumerator<RadarBlipComponent, TransformComponent>();

            while (blipQuery.MoveNext(out var blipUid, out var blip, out var _))
            {
                if (!blip.Enabled)
                    continue;

                var distance = (_xform.GetWorldPosition(blipUid) - _xform.GetWorldPosition(uid)).Length();
                if (distance > component.MaxRange)
                    continue;

                var blipGrid = _xform.GetGrid(blipUid);
                var radarGrid = _xform.GetGrid(uid);

                if (blip.RequireNoGrid)
                {
                    if (blipGrid != null)
                        continue;
                }
                else
                {
                    if (blipGrid != radarGrid)
                        continue;
                }

                blips.Add((_xform.GetWorldPosition(blipUid), blip.Scale, blip.Color));
            }
        }

        return blips;
    }
}
