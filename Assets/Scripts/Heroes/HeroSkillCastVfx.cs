using System;
using UnityEngine;
using Photon.Pun;

public class HeroSkillCastVfx : MonoBehaviourPun
{
    [Serializable]
    public class SkillVfx
    {
        public PlayerRole role = PlayerRole.None;
        public HeroSkillSlot slot = HeroSkillSlot.Q;
        public ParticleSystem prefab;
        public Transform attachPoint;
        public Vector3 localOffset;
        public bool followOwner = false;
        public float destroyAfter = 3f;
    }

    [SerializeField] private HeroRuntime heroRuntime;
    [SerializeField] private PlayerRole defaultRole = PlayerRole.Engineer;
    [SerializeField] private SkillVfx[] vfx;

    void Awake()
    {
        if (heroRuntime == null)
            heroRuntime = GetComponent<HeroRuntime>();
    }

    void OnEnable()
    {
        if (heroRuntime != null)
            heroRuntime.SkillActivated += OnSkillActivated;
    }

    void OnDisable()
    {
        if (heroRuntime != null)
            heroRuntime.SkillActivated -= OnSkillActivated;
    }

    private void OnSkillActivated(HeroSkillDefinition def)
    {
        if (def == null) return;

        if (PhotonNetwork.InRoom && photonView != null)
        {
            if (photonView.IsMine)
                photonView.RPC(nameof(PlayVfxRpc), RpcTarget.All, (int)def.slot);
            return;
        }

        PlayVfx(def.slot);
    }

    [PunRPC]
    private void PlayVfxRpc(int slotValue)
    {
        PlayVfx((HeroSkillSlot)slotValue);
    }

    private void PlayVfx(HeroSkillSlot slot)
    {
        if (vfx == null || vfx.Length == 0) return;

        PlayerRole ownerRole = GetOwnerRole();
        for (int i = 0; i < vfx.Length; i++)
        {
            SkillVfx entry = vfx[i];
            if (entry == null || entry.prefab == null) continue;
            if (entry.slot != slot) continue;
            if (entry.role != PlayerRole.None && entry.role != ownerRole) continue;

            Transform anchor = entry.attachPoint != null ? entry.attachPoint : transform;
            ParticleSystem instance;
            if (entry.followOwner)
            {
                instance = Instantiate(entry.prefab, anchor);
                instance.transform.localPosition = entry.localOffset;
                instance.transform.localRotation = Quaternion.identity;
            }
            else
            {
                Vector3 worldPos = anchor.TransformPoint(entry.localOffset);
                instance = Instantiate(entry.prefab, worldPos, anchor.rotation);
            }

            if (entry.destroyAfter > 0f)
                Destroy(instance.gameObject, entry.destroyAfter);

            instance.Play();
            break;
        }
    }

    private PlayerRole GetOwnerRole()
    {
        if (PhotonNetwork.InRoom && photonView != null && photonView.Owner != null)
        {
            if (PlayerRoleUtils.TryGetPlayerRole(photonView.Owner, out PlayerRole role))
                return role;
        }

        return defaultRole;
    }
}
