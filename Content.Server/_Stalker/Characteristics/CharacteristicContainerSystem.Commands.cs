using System.Threading.Tasks;
using Content.Server.Administration;
using Content.Shared._Stalker.Characteristics;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Stalker.Characteristics;

public sealed partial class CharacteristicContainerSystem
{
    [Dependency] private readonly IConsoleHost _consoleHost = default!;

    private void InitializeCommands()
    {
        _consoleHost.RegisterCommand("characteristic_set_levels",
            Loc.GetString("characteristic-set-levels-name"),
            "characteristic_set_levels <uid> <type> <level>",
            SetLevelCommand,
            GetLevelOperationCompletion);

        _consoleHost.RegisterCommand("characteristic_get_levels",
            Loc.GetString("characteristic-get-levels-name"),
            "characteristic_get_levels <uid> <type>",
            GetLevelCommand,
            GetLevelOperationCompletion);
    }

    [AdminCommand(AdminFlags.Host)]
    private void SetLevelCommand(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 3)
        {
            shell.WriteError(Loc.GetString("shell-argument-count-must-be", ("value", 3)));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var uidNet) || !TryGetEntity(uidNet, out var entityUid) || entityUid is not { } uid)
        {
            shell.WriteError(Loc.GetString("shell-could-not-find-entity", ("entity", args[0])));
            return;
        }

        if (!TryComp<CharacteristicContainerComponent>(uid, out var characteristicContainer))
        {
            shell.WriteError(Loc.GetString("shell-entity-with-uid-lacks-component", ("uid", uid), ("componentName", nameof(CharacteristicContainerComponent))));
            return;
        }

        if (!Enum.TryParse<CharacteristicType>(args[1], out var type))
        {
            shell.WriteError(Loc.GetString("shell-invalid-enum-item", ("value", args[1])));
            return;
        }

        if (!int.TryParse(args[2], out var level))
        {
            shell.WriteError(Loc.GetString("shell-invalid-int", ("value", args[2])));
            return;
        }

        if (!TrySetCharacteristic((uid, characteristicContainer), type, level))
        {
            shell.WriteError($"Cannot set {type}");
            return;
        }

        shell.WriteLine($"Value of {type}({uid}) set to {level}");
    }

    [AdminCommand(AdminFlags.Host)]
    private void GetLevelCommand(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("shell-argument-count-must-be", ("value", 2)));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var uidNet) || !TryGetEntity(uidNet, out var entityUid) || entityUid is not { } uid)
        {
            shell.WriteError(Loc.GetString("shell-could-not-find-entity", ("entity", args[0])));
            return;
        }

        if (!TryComp<CharacteristicContainerComponent>(uid, out var characteristicContainer))
        {
            shell.WriteError(Loc.GetString("shell-entity-with-uid-lacks-component", ("uid", uid), ("componentName", nameof(CharacteristicContainerComponent))));
            return;
        }

        if (!Enum.TryParse<CharacteristicType>(args[1], out var type))
        {
            shell.WriteError(Loc.GetString("shell-invalid-enum-item", ("value", args[1])));
            return;
        }

        if (!TryGetCharacteristic((uid, characteristicContainer), type, out var level))
        {
            shell.WriteError($"Cannot get {type}");
            return;
        }

        shell.WriteLine($"Value of {type}({uid}) is {level.Value.Level}");
    }

    private CompletionResult GetLevelOperationCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.Components<CharacteristicContainerComponent>(args[0]), "<uid>"),
            2 => CompletionResult.FromOptions(Enum.GetNames<CharacteristicType>()),
            _ => CompletionResult.Empty,
        };
    }
}
