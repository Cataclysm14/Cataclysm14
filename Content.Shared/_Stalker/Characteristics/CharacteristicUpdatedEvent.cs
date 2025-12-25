namespace Content.Shared._Stalker.Characteristics;

public sealed partial class CharacteristicUpdatedEvent(Characteristic characteristic, int oldLevel, int newLevel) : EntityEventArgs
{
    public readonly Characteristic Characteristic = characteristic;
    public readonly int OldLevel = oldLevel;
    public readonly int NewLevel = newLevel;
}
