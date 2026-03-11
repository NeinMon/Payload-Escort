using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MedicReviveSkill : HeroSkillBehaviour, IHeroSkillId
{
    private const string SkillIdConst = "medic_revive";

    public string SkillId => SkillIdConst;

    [Header("Revive")]
    public float castTime = 0f;
    public float reviveRange = 10f;
    public int maxRevives = 2;
    [Range(0.1f, 1f)]
    public float reviveHealthRatio = 0.5f;

    [Header("VFX")]
    [Tooltip("Resources path, e.g. Magic Shield or VFX/Magic Shield")]
    public string reviveVfxResource = "Magic shield pink";
    public Vector3 reviveVfxOffset = new Vector3(0f, 1f, 0f);

    private PhotonView photonView;
    private PlayerHealth selfHealth;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        selfHealth = GetComponent<PlayerHealth>();
    }

    public override void Activate(HeroRuntime runtime)
    {
        if (runtime == null)
            return;

        if (!IsCorrectSkill(runtime))
            return;

        if (photonView != null && PhotonNetwork.InRoom && !photonView.IsMine)
            return;
        ExecuteRevive();
    }

    [PunRPC]
    private void RPC_SpawnReviveVfx(int actorNumber)
    {
        PlayerHealth target = FindPlayerByActor(actorNumber);
        if (target == null)
            return;

        SpawnReviveVfx(target.transform.position);
    }

    private void ExecuteRevive()
    {
        if (selfHealth != null && selfHealth.IsDead)
            return;

        List<PlayerHealth> targets = FindReviveTargets();
        for (int i = 0; i < targets.Count; i++)
        {
            PlayerHealth target = targets[i];
            if (target == null || target.photonView == null)
                continue;

            target.photonView.RPC("Revive", RpcTarget.All, reviveHealthRatio);

            if (PhotonNetwork.InRoom && photonView != null)
                photonView.RPC(nameof(RPC_SpawnReviveVfx), RpcTarget.All, target.photonView.OwnerActorNr);
            else
                SpawnReviveVfx(target.transform.position);
        }
    }

    private void SpawnReviveVfx(Vector3 position)
    {
        GameObject prefab = Resources.Load<GameObject>(reviveVfxResource);
        if (prefab == null)
            return;

        GameObject instance = Instantiate(prefab, position + reviveVfxOffset, Quaternion.identity);
        Destroy(instance, 3f);
    }

    private List<PlayerHealth> FindReviveTargets()
    {
        List<PlayerHealth> results = new List<PlayerHealth>();
        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

        for (int i = 0; i < players.Length; i++)
        {
            PlayerHealth health = players[i];
            if (health == null || !health.IsDead)
                continue;

            if (health == selfHealth)
                continue;

            float dist = Vector3.Distance(transform.position, health.transform.position);
            if (dist > reviveRange)
                continue;

            if (PhotonNetwork.InRoom)
            {
                if (!IsSameTeam(health))
                    continue;
            }

            results.Add(health);
        }

        results.Sort((a, b) => Vector3.Distance(transform.position, a.transform.position)
            .CompareTo(Vector3.Distance(transform.position, b.transform.position)));

        if (results.Count > maxRevives)
            results.RemoveRange(maxRevives, results.Count - maxRevives);

        return results;
    }

    private bool IsSameTeam(PlayerHealth target)
    {
        if (photonView == null || photonView.Owner == null)
            return false;

        if (target == null || target.photonView == null || target.photonView.Owner == null)
            return false;

        if (!PayloadTeamUtils.TryGetPlayerTeam(photonView.Owner, out PayloadTeam myTeam))
            return false;

        if (!PayloadTeamUtils.TryGetPlayerTeam(target.photonView.Owner, out PayloadTeam targetTeam))
            return false;

        return myTeam == targetTeam;
    }

    private PlayerHealth FindPlayerByActor(int actorNumber)
    {
        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
        {
            PhotonView view = players[i].GetComponent<PhotonView>();
            if (view != null && view.OwnerActorNr == actorNumber)
                return players[i];
        }

        return null;
    }

    private bool IsCorrectSkill(HeroRuntime runtime)
    {
        HeroSkillDefinition def = runtime.GetSkill(HeroSkillSlot.R);
        return def != null && def.skillId == SkillIdConst;
    }
}
