namespace Content.Shared._Stalker.Modifier;

public sealed class UpdatedFloatModifierEvent<TComponent>(float modifier) : EntityEventArgs where TComponent : BaseFloatModifierComponent
{
    public readonly float Modifier = modifier;
}
