using Content.Shared.Shuttles.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class IFFConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public IFFFlags AllowedFlags;
    public IFFFlags Flags;

    /// <summary>
    /// the current cloak heat
    /// </summary>
    public float CloakHeat;

    public IFFConsoleBoundUserInterfaceState(float cloakHeat = 0)
    {
        CloakHeat = cloakHeat;
    }
}

[Serializable, NetSerializable]
public enum IFFConsoleUiKey : byte
{
    Key,
}
