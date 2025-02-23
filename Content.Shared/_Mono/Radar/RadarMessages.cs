using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Shared._Mono.Radar;

[Serializable, NetSerializable]
public sealed class GiveBlipsEvent : EntityEventArgs
{
    /// <summary>
    /// Blips are position, scale, and color.
    /// </summary>
    public readonly List<(Vector2, float, Color)> Blips;
    public GiveBlipsEvent(List<(Vector2, float, Color)> blips)
    {
        Blips = blips;
    }
}

[Serializable, NetSerializable]
public sealed class RequestBlipsEvent : EntityEventArgs
{
    public NetEntity Radar;
    public RequestBlipsEvent(NetEntity radar)
    {
        Radar = radar;
    }
}
