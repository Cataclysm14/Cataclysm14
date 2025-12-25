using Content.Shared._Stalker.Characteristics;
using Content.Shared.Movement.Systems;

namespace Content.Server._Stalker.Characteristics.Modifiers.MovementSpeed;

public sealed class CharacteristicModifierMovementSpeedSystem : EntitySystem
{
    [Dependency] private readonly CharacteristicContainerSystem _characteristicContainer = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CharacteristicModifierMovementSpeedComponent, CharacteristicUpdatedEvent>(OnUpdate);
        SubscribeLocalEvent<CharacteristicModifierMovementSpeedComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeedModifiers);
    }

    private void OnUpdate(Entity<CharacteristicModifierMovementSpeedComponent> modifier, ref CharacteristicUpdatedEvent args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(modifier);
    }

    private void OnRefreshMovementSpeedModifiers(Entity<CharacteristicModifierMovementSpeedComponent> modifier, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!_characteristicContainer.TryGetValue(modifier, CharacteristicType.Dexterity, out var level))
            return;

        if (level == 0)
            return;

        var value = Math.Abs((float)level);
        var mod = level > 0
            ? modifier.Comp.PositiveModifier
            : modifier.Comp.NegativeModifier;

        var speed = Math.Clamp(1f + value * mod, modifier.Comp.MinBonus, modifier.Comp.MaxBonus);
        args.ModifySpeed(speed, speed);
    }
}
