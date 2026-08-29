using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HoodedClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public string EquippedPrefix = "raised";

    [DataField, AutoNetworkedField]
    public string VisualLayerKey = "jumpsuit";

    [DataField, AutoNetworkedField]
    public string BaseState = "equipped-INNERCLOTHING";
}
