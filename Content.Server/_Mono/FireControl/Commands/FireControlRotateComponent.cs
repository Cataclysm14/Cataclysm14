// Copyright Rane (elijahrane@gmail.com) 2025
// All rights reserved. Relicensed under AGPL with permission

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Mono.FireControl;

[RegisterComponent]
public sealed partial class FireControlRotateComponent : Component;
// component that should prevent a grid from being able to use spaceartillery
// intended for fullcloak ships
// i ape hullrot at every turn...
