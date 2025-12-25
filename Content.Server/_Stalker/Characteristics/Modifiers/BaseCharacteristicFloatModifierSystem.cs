using Content.Shared._Stalker.Characteristics;
using Content.Shared._Stalker.Modifier;

namespace Content.Server._Stalker.Characteristics.Modifiers;

public abstract class
    BaseCharacteristicFloatModifierSystem<TCharacteristicModifierComponent, TModifierComponent, TModifierSystem> : EntitySystem
        where TCharacteristicModifierComponent : BaseCharacteristicFloatModifierComponent
        where TModifierComponent : BaseFloatModifierComponent
        where TModifierSystem : BaseFloatModifierSystem<TModifierComponent>
{
    [Dependency] private readonly TModifierSystem _modifierSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TCharacteristicModifierComponent, CharacteristicUpdatedEvent>(OnCharacteristicUpdate);
        SubscribeLocalEvent<TCharacteristicModifierComponent, FloatModifierRefreshEvent<TModifierComponent>>(OnModifierRefresh);
    }

    private void OnCharacteristicUpdate(Entity<TCharacteristicModifierComponent> ent, ref CharacteristicUpdatedEvent args)
    {
        if (args.Characteristic.Type != ent.Comp.AllowedType)
            return;

        ent.Comp.Value = GetModifier(ent, args.NewLevel);

        _modifierSystem.RefreshModifiers(ent);
    }

    private void OnModifierRefresh(Entity<TCharacteristicModifierComponent> ent, ref FloatModifierRefreshEvent<TModifierComponent> args)
    {
        args.Modify(ent.Comp.Value);
    }

    protected virtual float GetModifier(Entity<TCharacteristicModifierComponent> ent, int value)
    {
        return Math.Clamp(1f + Math.Abs(value) * ent.Comp.Modifier, ent.Comp.MinModifier, ent.Comp.MaxModifier);
    }
}
