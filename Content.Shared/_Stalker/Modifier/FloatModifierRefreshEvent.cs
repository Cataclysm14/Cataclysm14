namespace Content.Shared._Stalker.Modifier;

public sealed partial class FloatModifierRefreshEvent<TComponent> : EntityEventArgs where TComponent : BaseFloatModifierComponent
{
    public float Modifier { get; private set; } = 1f;

    public void Modify(float modifier)
    {
        Modifier *= modifier;
    }
}
