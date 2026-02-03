using UnityEngine;

[CreateAssetMenu(fileName = "HeroUIIconSet", menuName = "FPS Game/UI/Hero UI Icon Set")]
public class HeroUIIconSet : ScriptableObject
{
    [System.Serializable]
    public class RoleIcons
    {
        public PlayerRole role;
        public Sprite heroIcon;
        public Sprite skillQ;
        public Sprite skillE;
        public Sprite skillR;
    }

    public Sprite healthBarFill;
    public Sprite healthBarBackground;
    public RoleIcons[] roles;

    public bool TryGetRoleIcons(PlayerRole role, out RoleIcons icons)
    {
        if (roles != null)
        {
            for (int i = 0; i < roles.Length; i++)
            {
                if (roles[i] != null && roles[i].role == role)
                {
                    icons = roles[i];
                    return true;
                }
            }
        }

        icons = null;
        return false;
    }
}
