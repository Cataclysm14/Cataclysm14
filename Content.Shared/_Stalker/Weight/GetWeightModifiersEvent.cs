namespace Content.Shared._Stalker.Weight;

public sealed class GetWeightModifiersEvent : EventArgs
{
    public float Inside;
    public float Self;

    public GetWeightModifiersEvent(float inside, float self)
    {
        Inside = inside;
        Self = self;
    }
}
