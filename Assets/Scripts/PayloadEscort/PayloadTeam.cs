using Photon.Realtime;
using ExitGames.Client.Photon;

public enum PayloadTeam
{
    Attackers = 0,
    Defenders = 1
}

public static class PayloadTeamConstants
{
    public const string TEAM_KEY = "Team";
}

public static class PayloadTeamUtils
{
    public static bool TryGetPlayerTeam(Player player, out PayloadTeam team)
    {
        team = PayloadTeam.Attackers;
        if (player == null || player.CustomProperties == null) return false;
        if (!player.CustomProperties.ContainsKey(PayloadTeamConstants.TEAM_KEY)) return false;

        object value = player.CustomProperties[PayloadTeamConstants.TEAM_KEY];
        if (value is int intValue)
        {
            team = (PayloadTeam)intValue;
            return true;
        }

        return false;
    }

    public static bool TryGetPlayerTeam(int actorNumber, out PayloadTeam team)
    {
        Player player = Photon.Pun.PhotonNetwork.CurrentRoom?.GetPlayer(actorNumber);
        return TryGetPlayerTeam(player, out team);
    }

    public static void SetPlayerTeam(Player player, PayloadTeam team)
    {
        if (player == null) return;
        Hashtable props = new Hashtable { { PayloadTeamConstants.TEAM_KEY, (int)team } };
        player.SetCustomProperties(props);
    }
}
