using Content.Client.Movement.Systems;
using Content.Shared.Zombies;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;

namespace Content.Client._Cataclysm14.Zombies;

/// <summary>
/// Gives the locally controlled zombie fullbright vision while preserving normal FOV
/// if ZombieComponent is attached
/// </summary>
public sealed class ZombieFullbrightSystem : EntitySystem
{
    [Dependency] private readonly ContentEyeSystem _contentEye = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityUid? _lastLocalEntity;
    private bool _lastZombieState;
    private bool _hasAppliedState;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var local = _player.LocalEntity;
        if (local == null)
            return;

        var uid = local.Value;
        var isZombie = HasComp<ZombieComponent>(uid);

        // only touch the eye when control changes or zombie state changes
        if (_hasAppliedState && _lastLocalEntity == uid && _lastZombieState == isZombie)
            return;

        if (!TryComp<EyeComponent>(uid, out var eye))
            return;

        if (!_timing.InPrediction || !_timing.IsFirstTimePredicted)
            return;

        // zombies ignore darkness; non-zombies use normal lighting
        _contentEye.RequestEye(eye.DrawFov, !isZombie);

        _lastLocalEntity = uid;
        _lastZombieState = isZombie;
        _hasAppliedState = true;
    }
}
