using Content.Shared.Body.Components;
using Content.Shared.Body.Organ; // Cataclysm14 Change
using Content.Shared.Gibbing.Events; // Shitmed Change
using JetBrains.Annotations;

namespace Content.Server.Destructible.Thresholds.Behaviors
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class GibBehavior : IThresholdBehavior
    {
        [DataField] public GibType GibType = GibType.Gib; // Shitmed Change
        [DataField] public GibContentsOption GibContents = GibContentsOption.Drop; // Shitmed Change
        [DataField] public bool DestroyOrgans = false;
        [DataField("recursive")] private bool _recursive = true;

        public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
        {
            if (system.EntityManager.TryGetComponent(owner, out BodyComponent? body))
            {
                // Cataclysm14 Begin
                var gibbedParts = system.BodySystem.GibBody(owner, _recursive, body, gib: GibType, contents: GibContents); // Shitmed Change
                foreach (var gibbedPart in gibbedParts)
                {
                    if (system.EntityManager.TryGetComponent(gibbedPart, out OrganComponent? organ))
                        system.EntityManager.DeleteEntity(gibbedPart);
                }
                // Cataclysm14 End
            }
        }
    }
}
