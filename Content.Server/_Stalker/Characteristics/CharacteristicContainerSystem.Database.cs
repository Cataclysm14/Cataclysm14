// using System.Threading.Tasks;
// using Content.Server.Database;
// using Content.Shared._Stalker.Characteristics;
// using Robust.Shared.Asynchronous;
// using Robust.Shared.Player;

// namespace Content.Server._Stalker.Characteristics;

// public sealed partial class CharacteristicContainerSystem : SharedCharacteristicContainerSystem
// {
//     [Dependency] private readonly IServerDbManager _dbManager = default!;
//     [Dependency] private readonly ITaskManager _taskManager = default!;

//     private readonly Dictionary<(string, CharacteristicType), DateTime?> _lastTrainedByCharacteristic = [];

//     private void InitializeDatabase()
//     {
//         SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerBeforeSpawn);
//     }

//     // private async void OnPlayerBeforeSpawn(PlayerAttachedEvent args)
//     // {
//     //     await LoadCharacteristicsAsync(args.Player);
//     // }

//     public async Task<bool> IsTrainTimeConditionMet(Entity<CharacteristicContainerComponent> entity, CharacteristicType type)
//     {
//         var login = GetLogin(entity);
//         if (login is null)
//             return false;

//         var characteristic = entity.Comp.Characteristics[type];

//         var whenLastTrained = DateTime.MinValue.ToUniversalTime();

//         if (_lastTrainedByCharacteristic.TryGetValue((login, type), out var lastTrained) && lastTrained.HasValue)
//         {
//             whenLastTrained = lastTrained.Value;
//         }

//         var date = DateOnly.FromDateTime(whenLastTrained);
//         var today = DateOnly.FromDateTime(DateTime.UtcNow);
//         return today > date;
//     }

//     public string? GetLogin(Entity<CharacteristicContainerComponent> entity)
//     {
//         if (!TryComp<ActorComponent>(entity.Comp.Owner, out var actor))
//             return null;

//         return actor.PlayerSession.Name;
//     }

//     /// <summary>
//     /// Clears any in-memory caches related to stalker characteristics/stats.
//     /// Called when a global reset of stalker data is performed.
//     /// </summary>
// //     public void ClearAllStatsCache()
// //     {
// //         _lastTrainedByCharacteristic.Clear();
// //     }
// // }
