using UnityEngine;
using Photon.Pun;

public class MedicHealZoneSkill : HeroSkillBehaviour, IHeroSkillId
{
    private const string SkillIdConst = "medic_heal_zone";

    public string SkillId => SkillIdConst;

    [Header("Placement")]
    public float maxRange = 20f;
    public LayerMask placementMask = ~0;
    public float minGroundNormal = 0.6f;

    [Header("Healing")]
    public float radius = 4f;
    public float healPerSecond = 25f;
    public float tickInterval = 0.2f;

    [Header("VFX")]
    [Tooltip("Resources path, e.g. Healing Circle or VFX/Healing Circle")]
    public string healZoneVfxResource = "Healing circle";

    private PhotonView photonView;
    private PlayerControllerNetwork controller;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        controller = GetComponent<PlayerControllerNetwork>();
    }

    public override void Activate(HeroRuntime runtime)
    {
        if (runtime == null)
            return;

        if (!IsCorrectSkill(runtime))
            return;

        if (photonView != null && PhotonNetwork.InRoom && !photonView.IsMine)
            return;

        if (!TryGetPlacement(runtime, out Vector3 pos))
            return;

        float duration = GetDuration(runtime);
        SpawnLocalHealZone(pos, duration);

        SpawnVfxLocal(pos, duration);

        if (PhotonNetwork.InRoom && photonView != null)
            photonView.RPC(nameof(RPC_SpawnHealZoneVfx), RpcTarget.All, pos, duration);
    }

    [PunRPC]
    private void RPC_SpawnHealZoneVfx(Vector3 position, float duration)
    {
        if (photonView != null && photonView.IsMine)
            return;

        SpawnVfxLocal(position, duration);
    }

    private void SpawnVfxLocal(Vector3 position, float duration)
    {
        GameObject prefab = Resources.Load<GameObject>(healZoneVfxResource);
        if (prefab == null)
            return;

        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        if (duration > 0f)
            Destroy(instance, duration);
    }

    private void SpawnLocalHealZone(Vector3 position, float duration)
    {
        GameObject areaObject = new GameObject("MedicHealZoneRuntime");
        areaObject.transform.position = position;

        MedicHealZoneArea area = areaObject.AddComponent<MedicHealZoneArea>();
        area.radius = radius;
        area.healPerSecond = healPerSecond;
        area.tickInterval = tickInterval;
        area.duration = duration;

        if (PhotonNetwork.InRoom && photonView != null)
        {
            area.ownerActorNumber = photonView.OwnerActorNr;
            if (PayloadTeamUtils.TryGetPlayerTeam(photonView.Owner, out PayloadTeam team))
                area.ownerTeam = team;
        }

        area.Initialize(true);
    }

    private bool TryGetPlacement(HeroRuntime runtime, out Vector3 position)
    {
        position = Vector3.zero;
        Transform origin = GetAimOrigin(runtime);
        if (origin == null)
            return false;

        Ray ray = new Ray(origin.position, origin.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRange, placementMask, QueryTriggerInteraction.Ignore))
            return false;

        if (hit.normal.y < minGroundNormal)
            return false;

        position = hit.point;
        return true;
    }

    private Transform GetAimOrigin(HeroRuntime runtime)
    {
        if (controller != null && controller.playerCamera != null)
            return controller.playerCamera.transform;

        if (Camera.main != null)
            return Camera.main.transform;

        return runtime != null ? runtime.transform : transform;
    }

    private float GetDuration(HeroRuntime runtime)
    {
        HeroSkillDefinition def = runtime.GetSkill(HeroSkillSlot.Q);
        if (def != null && def.durationSeconds > 0f)
            return def.durationSeconds;

        return 10f;
    }

    private bool IsCorrectSkill(HeroRuntime runtime)
    {
        HeroSkillDefinition def = runtime.GetSkill(HeroSkillSlot.Q);
        return def != null && def.skillId == SkillIdConst;
    }
}
