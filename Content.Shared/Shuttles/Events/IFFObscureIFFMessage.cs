using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events;

[Serializable, NetSerializable]
// Mono -  obscure iff class
public sealed class IFFObscureIFFMessage : BoundUserInterfaceMessage
{
    public bool Show;
}
