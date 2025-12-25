using JetBrains.Annotations;

namespace Content.Shared._Stalker.MoveSpeed;

/// <summary>
/// This handles...
/// </summary>
///
[UsedImplicitly]
public abstract class StalkerMoveSpeedSystemShared : EntitySystem
{

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
    }

    public void SetBonusSpeedWalk(string NameBonus,float ValueBonus,EntityUid InputEntity)
    {
        var ev = new StalkerMSSetBonusWalkEvent(NameBonus,ValueBonus,InputEntity);
        EntityManager.EventBus.RaiseEvent(EventSource.Local, ev);
    }

    public void SetBonusSpeedSprint(string NameBonus,float ValueBonus,EntityUid InputEntity)
    {
        var ev = new StalkerMSSetBonusSprintEvent(NameBonus,ValueBonus,InputEntity);
        EntityManager.EventBus.RaiseEvent(EventSource.Local, ev);
    }

}


public sealed class StalkerMSSetBonusWalkEvent : EntityEventArgs
{
    public string NameBonus="";
    public float ValueBonus=0f;
    public EntityUid Entity;

    public StalkerMSSetBonusWalkEvent(string nameBonus, float valueBonus, EntityUid entity)
    {
        NameBonus = nameBonus;
        ValueBonus = valueBonus;
        Entity = entity;
    }
}

public sealed class StalkerMSSetBonusSprintEvent : EntityEventArgs
{
    public string NameBonus="";
    public float ValueBonus=0f;
    public EntityUid Entity;

    public StalkerMSSetBonusSprintEvent(string nameBonus, float valueBonus, EntityUid entity)
    {
        NameBonus = nameBonus;
        ValueBonus = valueBonus;
        Entity = entity;
    }
}
