using Robust.Shared.GameStates;

namespace Content.Shared._ES.Viewcone;

/// <summary>
/// ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡠⠀⠀⠀⡀⢄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
/// ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⣴⡿⣟⣿⣻⣦⠆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
/// ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣲⣯⢿⡽⣞⣷⣻⡞⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
/// ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡈⢿⣽⣯⢿⡽⣞⣷⡻⢥⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
/// ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
/// ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠰⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
/// ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
/// ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣣⣀⣀⡀⠀⠀⠀⠀⠀⠀⢀⣀⣀⣬⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
/// ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣌⣿⡽⣯⣟⣿⣻⢟⣿⡻⣟⣯⢿⡽⣯⡡⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
/// ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠰⣽⣷⣻⢷⣻⢾⣽⣻⢾⣽⣻⣞⣯⣟⡷⣧⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
/// ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢳⣟⡾⣽⢯⡿⣽⢾⣽⣻⣞⣷⣻⢾⣭⣟⡷⡞⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
/// ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣘⠉⠙⠙⠯⠿⢽⣯⣟⣾⣳⣟⣾⡽⠻⠾⠙⠋⠉⢁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
/// ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠆⠀⠀⠀⠀⠀⠀⠀⠀⠀
/// ⠀⠀⠀⠀⢀⣤⣤⡶⣶⣶⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⣶⣶⢶⣤⣠⡀⠀⠀⠀
/// ⠀⠀⠀⣰⣟⡷⣯⣟⣷⡻⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⢷⣯⣟⡾⣽⣳⣆⠀⠀
/// ⠀⠀⢀⡿⣾⣽⣳⣟⡾⢳⣟⣿⣲⢦⣤⣄⣀⣀⠀⠀⠀⠀⠀⠀⢀⣀⣀⣤⣴⢶⡿⣽⡛⣾⣽⢻⣷⣻⢾⡄⠀
/// ⠀⠀⢸⣟⡷⣯⢷⣯⣏⡿⣽⢾⣽⣻⢾⡽⣯⣟⡿⣻⣟⢿⣻⣟⡿⣯⣟⡷⣯⢿⣽⣳⢿⣹⣞⡿⡾⣽⣻⣄⠀
/// ⠀⠀⣿⢾⡽⣯⣟⡾⣼⡽⣯⣟⡾⣽⢯⣟⡷⣯⣟⡷⣯⣟⡷⣯⣟⡷⣯⢿⣽⣻⢾⡽⣯⢧⢯⡿⣽⣳⣟⣎⠀
/// ⠀⢰⣯⢿⣽⣳⢯⡷⣯⣟⡷⣯⢿⣽⣻⢾⣽⣳⢯⣟⡷⣯⣟⡷⣯⢿⣽⣻⢾⡽⣯⢿⣽⣻⢾⣽⣳⣟⡾⣽⠇
/// ⠀⣼⣞⡿⡾⣽⢯⣟⡷⣯⢿⣽⣻⢾⣽⣻⢾⣽⣻⢾⣽⣳⢯⡿⣽⣻⢾⡽⣯⢿⣽⣻⢾⣽⣻⣞⡷⣯⢿⣽⣣
/// ⢠⡾⣽⣻⡽⣯⣟⡾⣽⢯⣟⡾⣽⣻⢾⣽⣻⢾⣽⣻⢾⡽⣯⣟⡷⣯⢿⡽⣯⣟⡾⣽⣻⣞⡷⣯⢿⣽⣻⣞⣷
/// ⠀⠻⣷⣯⣟⡷⣯⢿⣽⣻⢾⣽⣳⢯⣟⡾⣽⣻⢾⡽⣯⣟⡷⣯⢿⡽⣯⣟⡷⣯⣟⣷⣻⢾⡽⣯⣟⣾⣳⢿⡞
/// ⠀⠀⠈⠓⠛⠙⠋⠛⠚⠙⠛⠚⠋⠛⠚⠛⠓⠛⠋⠛⠓⠋⠛⠙⠋⠛⠓⠋⠛⠓⠛⠚⠙⠋⠛⠓⠛⠚⠛⠉⠀
///           THE CONE MAN APPROACHES
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ESViewconeComponent : Component
{
    [DataField, AutoNetworkedField]
    public float ConeAngle = 190f;

    [DataField, AutoNetworkedField]
    public float ConeFeather = 40f;

    [DataField, AutoNetworkedField]
    public float ConeIgnoreRadius = 1f;

    [DataField, AutoNetworkedField]
    public float ConeIgnoreFeather = 0.35f;

    //Cataclysm14 start
    [DataField, AutoNetworkedField]
    public float GrainMultiplier = 2.5f;

    [DataField, AutoNetworkedField]
    public float GrainBase = 1f;

    [DataField, AutoNetworkedField]
    public float ConeColorFactor = 0.8f;

    [DataField, AutoNetworkedField]
    public float ConeColor = 0.008f;
    //Cataclysm14 end

    // Clientside, used for lerping view angle
    // and keeping it consistent across all overlays
    public Angle ViewAngle;
    public Angle? DesiredViewAngle = null;
    public Angle LastMouseRotationAngle;
    public Angle LastWorldRotationAngle;
}
