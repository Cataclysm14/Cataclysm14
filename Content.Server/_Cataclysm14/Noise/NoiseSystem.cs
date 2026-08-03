using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.NPC.Components;
using Content.Shared._Cataclysm14.Noise;
using Content.Shared.FixedPoint;
using Content.Shared.Weapons.Ranged.Systems;
using Microsoft.Extensions.ObjectPool;
using Robust.Shared.Threading;
using Robust.Shared.Utility;

namespace Content.Server._Cataclysm14.Noise;

public sealed partial class NoiseSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;

    private ObjectPool<HashSet<Entity<NoiseListenerComponent>>> _entPool = new DefaultObjectPool<HashSet<Entity<NoiseListenerComponent>>>(new SetPolicy<Entity<NoiseListenerComponent>>());

    private List<NoiseListenerComponent> _heardNoise = new(32);

    private UpdateNoiseJob _job;
    private bool _cleanJob = false;

    public override void Initialize()
    {
        _job = new UpdateNoiseJob(_heardNoise, this);
        SubscribeLocalEvent<NoiseEvent>(OnNoiseEvent);
        SubscribeLocalEvent<EntitySpokeEvent>(OnEntitySpokeEvent);
        SubscribeLocalEvent<GunShotEvent>(OnGunShotEvent);
        SubscribeLocalEvent<NoiseListenerComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnNoiseEvent(ref NoiseEvent ev) => ProcessNoise(ev);

    private void OnEntitySpokeEvent(EntitySpokeEvent ev)
    {
        if (ev.IsWhisper)
            return;

        ProcessNoise(new(Transform(ev.Source).Coordinates, ev.Message.EndsWith('!') ? 13 : 6));
    }

    private void OnGunShotEvent(ref GunShotEvent ev) => ProcessNoise(new(Transform(ev.User).Coordinates, 30));

    private void ProcessNoise(in NoiseEvent ev)
    {
        var set = _entPool.Get();
        _lookup.GetEntitiesInRange(ev.Source, ev.Strength, set);
        foreach (var ent in set)
        {
            if (HasComp<NPCMeleeCombatComponent>(ent.Owner)) // skip if zombie in combat mode
                continue;

            var comp = ent.Comp;
            if (!_heardNoise.Contains(comp))
                _heardNoise.Add(comp);
            comp.Target = ev.Source;
            comp.Timer = FixedPoint2.Zero;
        }
        _entPool.Return(set);
    }

    private void OnComponentShutdown(EntityUid uid, NoiseListenerComponent component, ComponentShutdown args)
    {
        if (_heardNoise.Contains(component))
            _heardNoise.Remove(component);
    }

    public override void Update(float frameTime)
    {
        _job.FrameTime = frameTime;
        _parallel.ProcessNow(_job, _heardNoise.Count);
        if (_cleanJob)
        {
            _cleanJob = false;
            _heardNoise.RemoveAll(RemoveAllEmpty);
        }
    }

    private bool RemoveAllEmpty(NoiseListenerComponent obj)
    {
        return obj.Target == null;
    }

    private record struct UpdateNoiseJob(List<NoiseListenerComponent> HeardNoise, NoiseSystem System) : IParallelRobustJob
    {
        public int BatchSize => 16;

        public FixedPoint2 FrameTime = FixedPoint2.Zero;

        public void Execute(int index)
        {
            var heardNoise = HeardNoise[index];
            heardNoise.Timer += FrameTime;
            if (heardNoise.Timer >= heardNoise.MaxTimer)
            {
                heardNoise.Timer = FixedPoint2.Zero;
                heardNoise.Target = null;
                System._cleanJob = true;
            }
        }
    }
}
