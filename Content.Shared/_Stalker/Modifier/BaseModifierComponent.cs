namespace Content.Shared._Stalker.Modifier;

public abstract partial class BaseModifierComponent<T> : Component
{
    [DataField, ViewVariables]
    public T Modifier { get; set; }
}
