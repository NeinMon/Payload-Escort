using UnityEngine;

[CreateAssetMenu(fileName = "HeroRoster", menuName = "FPS Game/Heroes/Hero Roster")]
public class HeroRoster : ScriptableObject
{
    [System.Serializable]
    public class HeroLoadout
    {
        public string heroId;
        public PlayerRole role;
        public HeroDefinition heroDefinition;
        public HeroSkillDefinition[] skills;
    }

    public HeroLoadout[] loadouts;

    public bool TryGetLoadoutByRole(PlayerRole role, out HeroLoadout loadout)
    {
        if (loadouts != null)
        {
            for (int i = 0; i < loadouts.Length; i++)
            {
                if (loadouts[i] != null && loadouts[i].role == role)
                {
                    loadout = loadouts[i];
                    return true;
                }
            }
        }

        loadout = null;
        return false;
    }

    public bool TryGetLoadoutById(string heroId, out HeroLoadout loadout)
    {
        if (!string.IsNullOrEmpty(heroId) && loadouts != null)
        {
            for (int i = 0; i < loadouts.Length; i++)
            {
                if (loadouts[i] != null && loadouts[i].heroId == heroId)
                {
                    loadout = loadouts[i];
                    return true;
                }
            }
        }

        loadout = null;
        return false;
    }
}
