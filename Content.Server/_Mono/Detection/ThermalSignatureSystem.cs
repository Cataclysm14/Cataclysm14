using Content.Server.Power.Components;
using Content.Server.Shuttles.Components;
using Content.Shared._Mono.Detection;
using Robust.Shared.Map.Components;
using System;

namespace Content.Server._Mono.Detection;

/// <summary>
///     Handles the logic for thermal signatures.
/// </summary>
public sealed class ThermalSignatureSystem : EntitySystem
{
    private TimeSpan _updateInterval = TimeSpan.FromSeconds(0.5);
    private TimeSpan _updateAccumulator = TimeSpan.FromSeconds(0);
    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<ThermalSignatureComponent> _sigQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerSupplierComponent, GetThermalSignatureEvent>(OnPowerGetSignature);
        SubscribeLocalEvent<ThrusterComponent, GetThermalSignatureEvent>(OnThrusterGetSignature);

        _gridQuery = GetEntityQuery<MapGridComponent>();
        _sigQuery = GetEntityQuery<ThermalSignatureComponent>();
    }

    private void OnPowerGetSignature(Entity<PowerSupplierComponent> ent, ref GetThermalSignatureEvent args)
    {
        args.Signature += ent.Comp.CurrentSupply * ent.Comp.HeatSignatureRatio;
    }

    private void OnThrusterGetSignature(Entity<ThrusterComponent> ent, ref GetThermalSignatureEvent args)
    {
        if (ent.Comp.Firing)
            args.Signature += ent.Comp.Thrust * ent.Comp.HeatSignatureRatio;
    }

    public override void Update(float frameTime)
    {
        _updateAccumulator += TimeSpan.FromSeconds(frameTime);
        if (_updateAccumulator < _updateInterval)
            return;
        _updateAccumulator -= _updateInterval;

        var interval = (float)_updateInterval.TotalSeconds;

        var gridQuery = EntityQueryEnumerator<MapGridComponent>();
        while (gridQuery.MoveNext(out var uid, out _))
        {
            var sigComp = EnsureComp<ThermalSignatureComponent>(uid);
            sigComp.TotalHeat = 0f;
        }

        var query = EntityQueryEnumerator<ThermalSignatureComponent>();
        while (query.MoveNext(out var uid, out var sigComp))
        {
            var ev = new GetThermalSignatureEvent(interval);
            RaiseLocalEvent(uid, ref ev);
            sigComp.StoredHeat += ev.Signature * interval;
            sigComp.StoredHeat *= MathF.Pow(sigComp.HeatDissipation, interval);
            if (_gridQuery.HasComp(uid))
            {
                sigComp.TotalHeat += sigComp.StoredHeat;
            }
            else
            {
                var xform = Transform(uid);
                sigComp.TotalHeat = sigComp.StoredHeat;
                if (xform.GridUid != null && _sigQuery.TryComp(xform.GridUid, out var gridSig))
                    gridSig.TotalHeat += sigComp.StoredHeat;
            }
        }

        var gridQuery2 = EntityQueryEnumerator<MapGridComponent, ThermalSignatureComponent>();
        while (gridQuery2.MoveNext(out var uid, out _, out var sigComp))
        {
            Dirty(uid, sigComp); // sync to client
        }
    }
}
