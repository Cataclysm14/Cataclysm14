using Robust.Shared.Serialization;

namespace Content.Shared._Stalker.Characteristics;

[Serializable, NetSerializable]
public struct Characteristic
{
    public readonly CharacteristicPrototype Proto;

    public string ProtoId => Proto.ID;

    public string Name => Proto.Name;
    public string Description => Proto.Description;

    public CharacteristicAccess Access => Proto.Access;
    public CharacteristicType Type => Proto.Type;

    public int MinLevel => Proto.MinLevel;
    public int MaxLevel => Proto.MaxLevel;
    public int BaseLevel => Proto.BaseLevel;
    public int DefaultLevel => Proto.DefaultLevel;

    public int Value => Level - BaseLevel;

    public int Level { get; private set; }

    public Characteristic(CharacteristicPrototype proto)
    {
        Proto = proto;
        Level = DefaultLevel;
    }

    public Characteristic(Characteristic characteristic)
    {
        Proto = characteristic.Proto;
        Level = characteristic.Level;
    }

    public Characteristic WithLevel(int level)
    {
        return new(this) { Level = level };
    }
}
