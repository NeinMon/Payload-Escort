using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class HeroAssigner : MonoBehaviourPunCallbacks
{
    [SerializeField] private HeroRoster heroRoster;
    [SerializeField] private string heroRosterResourcesPath = "HeroRoster";
    [SerializeField] private HeroRuntime heroRuntime;

    void Awake()
    {
        if (heroRuntime == null)
            heroRuntime = GetComponent<HeroRuntime>();

        if (heroRoster == null && !string.IsNullOrEmpty(heroRosterResourcesPath))
            heroRoster = Resources.Load<HeroRoster>(heroRosterResourcesPath);
    }

    void Start()
    {
        ApplyHeroForOwner();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (photonView == null || photonView.Owner == null) return;
        if (targetPlayer == photonView.Owner)
            ApplyHeroForOwner();
    }

    private void ApplyHeroForOwner()
    {
        if (heroRuntime == null || heroRoster == null || photonView == null || photonView.Owner == null)
            return;

        if (PlayerRoleUtils.TryGetPlayerRole(photonView.Owner, out PlayerRole role)
            && heroRoster.TryGetLoadoutByRole(role, out HeroRoster.HeroLoadout loadoutByRole))
        {
            heroRuntime.SetHero(loadoutByRole.heroDefinition, loadoutByRole.skills);
        }
    }
}
