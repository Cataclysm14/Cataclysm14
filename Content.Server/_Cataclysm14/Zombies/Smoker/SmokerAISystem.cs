using System;
using Content.Server.NPC.HTN;
using Content.Shared._Cataclysm14.Zombies.Smoker;
using Content.Shared.Actions;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Timing;

namespace Content.Server._Cataclysm14.Zombies.Smoker;

/// <summary>
/// Lets NPC Smokers use the Smoker tongue action against the current target
/// selected by their existing zombie HTN
/// </summary>
public sealed class SmokerAISystem : EntitySystem
{
    private const float ThinkInterval = 0.25f;

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private float _thinkAccumulator;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _thinkAccumulator += frameTime;
        if (_thinkAccumulator < ThinkInterval)
            return;

        _thinkAccumulator = 0f;

        var query = EntityQueryEnumerator<SmokerComponent, HTNComponent>();
        while (query.MoveNext(out var smoker, out var smokerComp, out var htn))
        {
            if (smokerComp.TongueTarget != null || !IsAlive(smoker))
                continue;

            if (!htn.Blackboard.TryGetValue<EntityUid>("Target", out var target, EntityManager) ||
                Deleted(target) ||
                !IsAlive(target) ||
                HasComp<SmokerTonguedComponent>(target))
            {
                continue;
            }

            var smokerPos = _transform.GetMapCoordinates(smoker);
            var targetPos = _transform.GetMapCoordinates(target);
            if (smokerPos.MapId != targetPos.MapId)
                continue;

            var distance = (targetPos.Position - smokerPos.Position).Length();
            if (distance < smokerComp.TongueMinRange || distance > smokerComp.TongueMaxRange)
                continue;

            // require unobstructed LOS before the AI even attempts
            if (!_interaction.InRangeUnobstructed(smoker, target, smokerComp.TongueMaxRange, popup: false))
                continue;

            if (!TryGetReadyTongueAction(smoker, out var actionId, out var targetAction, out var tongueEvent))
                continue;

            if (!_actions.ValidateEntityTarget(smoker, target, (actionId, targetAction)))
                continue;

            smokerComp.TongueArmed = true;
            smokerComp.TongueReadyAt = _timing.CurTime;

            tongueEvent.Target = target;

            _actions.PerformAction(
                smoker,
                null,
                actionId,
                targetAction,
                tongueEvent,
                _timing.CurTime,
                predicted: false);

            if (!tongueEvent.Handled)
            {
                smokerComp.TongueArmed = false;
                smokerComp.TongueReadyAt = TimeSpan.Zero;
            }
        }
    }

    private bool TryGetReadyTongueAction(
        EntityUid smoker,
        out EntityUid actionId,
        out EntityTargetActionComponent targetAction,
        out SmokerTongueActionEvent tongueEvent)
    {
        foreach (var (id, action) in _actions.GetActions(smoker))
        {
            if (!HasComp<SmokerTongueActionComponent>(id) ||
                action is not EntityTargetActionComponent entityAction ||
                entityAction.Event is not SmokerTongueActionEvent smokerEvent ||
                !_actions.ValidAction(action))
            {
                continue;
            }

            actionId = id;
            targetAction = entityAction;
            tongueEvent = smokerEvent;
            return true;
        }

        actionId = default;
        targetAction = default!;
        tongueEvent = default!;
        return false;
    }

    private bool IsAlive(EntityUid uid)
    {
        return TryComp<MobStateComponent>(uid, out var mob) && mob.CurrentState == MobState.Alive;
    }
}
