using Photon.Realtime;
using ExitGames.Client.Photon;

public enum PlayerRole
{
    None = 0,
    Engineer = 1,
    Heavy = 2,
    Medic = 3,
    Saboteur = 4
}

public static class PlayerRoleConstants
{
    public const string ROLE_KEY = "Role";
}

public static class PlayerRoleUtils
{
    public static bool TryGetPlayerRole(Player player, out PlayerRole role)
    {
        role = PlayerRole.None;
        if (player == null || player.CustomProperties == null) return false;
        if (!player.CustomProperties.ContainsKey(PlayerRoleConstants.ROLE_KEY)) return false;

        object value = player.CustomProperties[PlayerRoleConstants.ROLE_KEY];
        if (value is int intValue)
        {
            role = (PlayerRole)intValue;
            return true;
        }

        return false;
    }

    public static bool TryGetPlayerRole(int actorNumber, out PlayerRole role)
    {
        Player player = Photon.Pun.PhotonNetwork.CurrentRoom?.GetPlayer(actorNumber);
        return TryGetPlayerRole(player, out role);
    }

    public static void SetPlayerRole(Player player, PlayerRole role)
    {
        if (player == null) return;
        Hashtable props = new Hashtable { { PlayerRoleConstants.ROLE_KEY, (int)role } };
        player.SetCustomProperties(props);
    }

    public static float GetPushWeight(PlayerRole role)
    {
        switch (role)
        {
            case PlayerRole.Heavy:
                return 1.5f;
            case PlayerRole.Saboteur:
                return 0.75f;
            default:
                return 1f;
        }
    }

    public static float GetRepairWeight(PlayerRole role)
    {
        switch (role)
        {
            case PlayerRole.Engineer:
                return 1.5f;
            case PlayerRole.Medic:
                return 1.2f;
            case PlayerRole.Heavy:
                return 0.8f;
            case PlayerRole.Saboteur:
                return 0.7f;
            default:
                return 1f;
        }
    }

    public static float GetSabotageWeight(PlayerRole role)
    {
        switch (role)
        {
            case PlayerRole.Saboteur:
                return 1.5f;
            case PlayerRole.Engineer:
            case PlayerRole.Medic:
                return 0.8f;
            default:
                return 1f;
        }
    }
}
