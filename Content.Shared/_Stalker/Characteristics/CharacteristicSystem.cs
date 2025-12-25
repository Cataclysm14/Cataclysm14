using System.Runtime.CompilerServices;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stalker.Characteristics;

public sealed class CharacteristicSystem : EntitySystem
{
    [Robust.Shared.IoC.Dependency] private readonly IPrototypeManager _prototype = default!;

    public IReadOnlyDictionary<CharacteristicType, Characteristic> Characteristics => _characteristics;

    private readonly Dictionary<CharacteristicType, Characteristic> _characteristics = new();

    public override void Initialize()
    {
        base.Initialize();

        EnumerateCharacteristics();

        _prototype.PrototypesReloaded += OnProtoReload;
    }

    // TODO: It'd be better to check influence of this attribute here on perf :)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnumerateCharacteristics()
    {
        var prototypes = _prototype.EnumeratePrototypes<CharacteristicPrototype>();
        foreach (var prototype in prototypes)
        {
            var characteristic = new Characteristic(prototype);
            _characteristics.Add(prototype.Type, characteristic);
        }
    }

    private void OnProtoReload(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<CharacteristicPrototype>())
            return;

        EnumerateCharacteristics();
    }
}
