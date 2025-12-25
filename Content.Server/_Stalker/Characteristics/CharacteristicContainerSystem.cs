using System.Diagnostics.CodeAnalysis;
using Content.Shared._Stalker.Characteristics;

namespace Content.Server._Stalker.Characteristics;

public sealed partial class CharacteristicContainerSystem : SharedCharacteristicContainerSystem
{
    [Dependency] private readonly CharacteristicSystem _characteristic = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeDatabase();

        InitializeCommands();

        SubscribeLocalEvent<CharacteristicContainerComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<CharacteristicContainerComponent> container, ref ComponentStartup args)
    {
        foreach (var (type, characteristic) in _characteristic.Characteristics)
        {
            TryAddCharacteristic(container, type, characteristic);
        }
    }

    public IReadOnlyDictionary<CharacteristicType, Characteristic> GetAllCharacteristic(
        EntityUid uid,
        CharacteristicAccess access = CharacteristicAccess.Replicated)
    {
        return !TryComp<CharacteristicContainerComponent>(uid, out var comp)
            ? new Dictionary<CharacteristicType, Characteristic>()
            : GetAllCharacteristic((uid, comp), access);
    }

    public IReadOnlyDictionary<CharacteristicType, Characteristic> GetAllCharacteristic(
        Entity<CharacteristicContainerComponent> container,
        CharacteristicAccess access = CharacteristicAccess.Replicated)
    {
        var dictionary = new Dictionary<CharacteristicType, Characteristic>();

        foreach (var (type, characteristic) in container.Comp.Characteristics)
        {
            if (characteristic.Access != access)
                continue;

            dictionary.TryAdd(type, characteristic);
        }

        return dictionary;
    }

    public bool TryGetValue(EntityUid uid, CharacteristicType type, out int level)
    {
        level = 0;
        return TryComp<CharacteristicContainerComponent>(uid, out var comp) &&
               TryGetValue((uid, comp), type, out level);
    }

    public bool TryGetValue(Entity<CharacteristicContainerComponent> container, CharacteristicType type, out int level)
    {
        level = 0;
        if (!TryGetCharacteristic(container, type, out var characteristic))
            return false;

        level = characteristic.Value.Value;
        return true;
    }

    public bool TryAddCharacteristic(Entity<CharacteristicContainerComponent> container, CharacteristicType type,
        Characteristic characteristic)
    {
        if (!container.Comp.Characteristics.TryAdd(type, characteristic))
            return false;

        RaiseUpdatedEvent(container, characteristic);
        return true;
    }

    public bool TryInitCharacteristic(Entity<CharacteristicContainerComponent> container, CharacteristicType type, int level)
    {
        if (!container.Comp.Characteristics.TryGetValue(type, out var value))
            return false;

        var oldLevel = value.Level;

        container.Comp.Characteristics[type] = value.WithLevel(level);

        RaiseUpdatedEvent(container, container.Comp.Characteristics[type], oldLevel);
        return true;
    }
    public bool TrySetCharacteristic(Entity<CharacteristicContainerComponent> container, CharacteristicType type, int level, DateTime? trainTime = null)
    {
        if (!container.Comp.Characteristics.TryGetValue(type, out var value))
            return false;

        var oldLevel = value.Level;

        container.Comp.Characteristics[type] = value.WithLevel(level);
        SaveCharacteristicAsync(container, type, level, trainTime).ContinueWith((_) =>
        {
            RaiseUpdatedEvent(container, container.Comp.Characteristics[type], oldLevel);
        });
        return true;
    }

    public bool TryGetCharacteristic(Entity<CharacteristicContainerComponent> container, CharacteristicType type,
        [NotNullWhen(true)] out Characteristic? characteristic)
    {
        characteristic = null;

        if (!container.Comp.Characteristics.TryGetValue(type, out var value))
            return false;

        characteristic = value;
        return true;
    }


    private void RaiseUpdatedEvent(Entity<CharacteristicContainerComponent> container, Characteristic characteristic, int oldLevel = 0)
    {
        var ev = new CharacteristicUpdatedEvent(characteristic, oldLevel, characteristic.Level);
        RaiseLocalEvent(container.Comp.Owner, ev);
    }
}
