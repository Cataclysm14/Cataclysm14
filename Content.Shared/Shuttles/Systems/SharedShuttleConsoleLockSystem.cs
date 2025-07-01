using Content.Shared.Access.Components;
using Content.Shared.Interaction;
using Content.Shared.Shuttles.Components;
using Content.Shared._NF.Shipyard.Components;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Content.Shared.Access;
using Content.Shared.Examine;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.Shuttles.Systems;

/// <summary>
/// System that handles locking and unlocking shuttle consoles based on shuttle deeds.
/// </summary>
public abstract class SharedShuttleConsoleLockSystem : EntitySystem
{
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShuttleConsoleLockComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<ShuttleConsoleLockComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, ShuttleConsoleLockComponent component, ComponentStartup args)
    {
        UpdateAppearance(uid, component);
    }

    private void OnExamined(EntityUid uid, ShuttleConsoleLockComponent component, ExaminedEvent args)
    {
        var effectiveLocked = GetEffectiveLockState(uid, component);
        args.PushMarkup(effectiveLocked ? Loc.GetString("shuttle-console-locked-examine") : Loc.GetString("shuttle-console-unlocked-examine"));
    }

    /// <summary>
    /// Gets the effective lock state for a console, considering grid-level locks when available
    /// </summary>
    public bool GetEffectiveLockState(EntityUid console, ShuttleConsoleLockComponent component)
    {
        // Get the grid this console is on
        var transform = Transform(console);
        if (transform.GridUid == null)
            return component.Locked || component.EmergencyLocked;

        var gridUid = transform.GridUid.Value;

        // If the grid has a deed and grid lock component, use grid lock state ONLY
        if (TryComp<ShuttleDeedComponent>(gridUid, out _) &&
            TryComp<ShipGridLockComponent>(gridUid, out var gridLock))
        {
            // Grid lock state takes complete precedence over individual console state
            return gridLock.Locked || gridLock.EmergencyLocked;
        }

        // No grid lock, use individual console lock state
        return component.Locked || component.EmergencyLocked;
    }

    protected void UpdateAppearance(EntityUid uid, ShuttleConsoleLockComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        var effectiveLocked = GetEffectiveLockState(uid, component);
        Appearance.SetData(uid, ShuttleConsoleLockVisuals.Locked, effectiveLocked, appearance);
    }

    /// <summary>
    /// Sets the lock state for a ship grid
    /// </summary>
    protected void SetGridLockState(EntityUid gridUid, bool locked, string? shuttleId = null)
    {
        if (!TryComp<ShipGridLockComponent>(gridUid, out var gridLock))
        {
            // Create the component if it doesn't exist
            gridLock = AddComp<ShipGridLockComponent>(gridUid);
        }

        gridLock.Locked = locked;
        if (shuttleId != null)
            gridLock.ShuttleId = shuttleId;

        Dirty(gridUid, gridLock);
    }

    /// <summary>
    /// Sets the emergency lock state for a ship grid
    /// </summary>
    protected void SetGridEmergencyLockState(EntityUid gridUid, bool emergencyLocked)
    {
        if (!TryComp<ShipGridLockComponent>(gridUid, out var gridLock))
        {
            // Create the component if it doesn't exist
            gridLock = AddComp<ShipGridLockComponent>(gridUid);
        }

        gridLock.EmergencyLocked = emergencyLocked;
        Dirty(gridUid, gridLock);
    }

    /// <summary>
    /// Ensures a grid has a ShipGridLockComponent if it has a deed
    /// </summary>
    protected void EnsureGridLockComponent(EntityUid gridUid, string? shuttleId = null)
    {
        // Only add to grids that have deeds
        if (!TryComp<ShuttleDeedComponent>(gridUid, out var deed))
            return;

        // Add the component if it doesn't exist
        if (!TryComp<ShipGridLockComponent>(gridUid, out var gridLock))
        {
            gridLock = AddComp<ShipGridLockComponent>(gridUid);
            gridLock.Locked = true; // Ships start locked by default
            gridLock.ShuttleId = shuttleId ?? deed.ShuttleUid?.ToString();
            Dirty(gridUid, gridLock);
        }
    }

    /// <summary>
    /// Attempts to unlock a console with the given ID card
    /// </summary>
    public virtual bool TryUnlock(EntityUid console, EntityUid idCard, ShuttleConsoleLockComponent? lockComp = null, IdCardComponent? idComp = null, EntityUid? user = null)
    {
        // Implemented in client and server separately
        return false;
    }
}
