using Photon.Realtime;
using ExitGames.Client.Photon;

public static class HeroSelectionConstants
{
    public const string HERO_ID_KEY = "HeroId";
}

public static class HeroSelectionUtils
{
    public static void SetPlayerHeroId(Player player, string heroId)
    {
        if (player == null) return;
        Hashtable props = new Hashtable { { HeroSelectionConstants.HERO_ID_KEY, heroId } };
        player.SetCustomProperties(props);
    }

    public static bool TryGetPlayerHeroId(Player player, out string heroId)
    {
        heroId = null;
        if (player == null || player.CustomProperties == null) return false;
        if (!player.CustomProperties.ContainsKey(HeroSelectionConstants.HERO_ID_KEY)) return false;

        heroId = player.CustomProperties[HeroSelectionConstants.HERO_ID_KEY] as string;
        return !string.IsNullOrEmpty(heroId);
    }
}
