using System.Numerics;
using Content.Shared._Mono.Radar;
using Robust.Shared.Timing;

namespace Content.Client._Mono.Radar;

public sealed partial class RadarBlipsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    private TimeSpan _lastUpdatedTime;
    private List<(Vector2, float, Color)> _blips = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<GiveBlipsEvent>(HandleReceiveBlips);
    }

    private void HandleReceiveBlips(GiveBlipsEvent ev, EntitySessionEventArgs args)
    {
        _blips = ev.Blips;
        _lastUpdatedTime = _timing.CurTime;
    }

    public void RequestBlips(EntityUid console)
    {
        var netConsole = GetNetEntity(console);

        var ev = new RequestBlipsEvent(netConsole);
        RaiseNetworkEvent(ev);
    }

    public List<(Vector2, float, Color)> GetCurrentBlips()
    {
        if (_timing.CurTime.TotalSeconds - _lastUpdatedTime.TotalSeconds > 1)
            return new List<(Vector2, float, Color)>();

        return _blips;
    }
} 