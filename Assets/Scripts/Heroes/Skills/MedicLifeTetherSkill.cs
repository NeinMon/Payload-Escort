using UnityEngine;
using Photon.Pun;

public class MedicLifeTetherSkill : HeroSkillBehaviour, IHeroManualCooldown, IHeroSkillId
{
    private const string SkillIdConst = "medic_life_tether";

    public string SkillId => SkillIdConst;

    [Header("Targeting")]
    public float maxRange = 20f;
    public LayerMask targetMask = ~0;

    [Header("Healing")]
    public float healPerSecond = 40f;
    public float healTickInterval = 0.2f;

    [Header("Medic Slow")]
    [Range(0.5f, 1f)]
    public float slowMultiplier = 0.85f;

    [Header("VFX")]
    [Tooltip("Resources path, e.g. Healing or VFX/Healing")]
    public string healingVfxResource = "Healing";
    public Vector3 targetVfxOffset = new Vector3(0f, -1f, 0f);

    [Header("Tether Line")]
    public Transform aimOrigin;
    public Transform tetherOrigin;
    public Vector3 lineStartOffset = new Vector3(0f, 0.4f, 0f);
    public Vector3 lineEndOffset = new Vector3(0f, 0.4f, 0f);
    public LineRenderer lineRenderer;
    public Color lineColor = new Color(0.2f, 1f, 0.7f, 1f);
    public float lineWidth = 0.04f;

    private PhotonView photonView;
    private PlayerControllerNetwork controller;
    private PlayerHealth selfHealth;
    private HeroRuntime runtimeRef;
    private PlayerHealth targetHealth;
    private GameObject targetVfxInstance;
    private float healTimer;
    private float cachedMoveSpeed;
    private bool tetherActive;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        controller = GetComponent<PlayerControllerNetwork>();
        selfHealth = GetComponent<PlayerHealth>();

        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
        }
    }

    public override void Initialize(HeroRuntime runtime)
    {
        runtimeRef = runtime;
    }

    void Update()
    {
        if (!tetherActive)
            return;

        if (photonView != null && photonView.IsMine)
        {
            if (selfHealth == null || selfHealth.IsDead || targetHealth == null || targetHealth.IsDead)
            {
                StopTether();
                return;
            }

            Transform origin = GetAimOrigin(runtimeRef);
            Vector3 originPos = origin != null ? origin.position : transform.position;
            float dist = Vector3.Distance(originPos, targetHealth.transform.position);
            if (dist > maxRange)
            {
                StopTether();
                return;
            }

            healTimer += Time.deltaTime;
            if (healTimer >= healTickInterval)
            {
                float amount = healPerSecond * healTimer;
                targetHealth.Heal(amount);
                healTimer = 0f;
            }
        }

        UpdateLine();
    }

    public override void Activate(HeroRuntime runtime)
    {
        if (runtime == null)
            return;

        if (!IsCorrectSkill(runtime))
            return;

        if (tetherActive)
        {
            StopTether();
            return;
        }

        if (photonView != null && !photonView.IsMine)
            return;

        PlayerHealth target = FindTarget(runtime);
        if (target == null)
            return;

        StartTetherLocal(target);

        if (PhotonNetwork.InRoom && photonView != null)
            photonView.RPC(nameof(RPC_StartTether), RpcTarget.All, target.photonView.OwnerActorNr);
    }

    private void StartTetherLocal(PlayerHealth target)
    {
        targetHealth = target;
        healTimer = 0f;
        tetherActive = true;

        if (controller != null && photonView != null && photonView.IsMine)
        {
            cachedMoveSpeed = controller.moveSpeed;
            controller.moveSpeed = cachedMoveSpeed * slowMultiplier;
        }

        SpawnTargetVfx();
        if (lineRenderer != null)
            lineRenderer.enabled = true;
    }

    private void StopTether()
    {
        if (!tetherActive)
            return;

        tetherActive = false;
        healTimer = 0f;

        if (controller != null && photonView != null && photonView.IsMine)
            controller.moveSpeed = cachedMoveSpeed;

        if (PhotonNetwork.InRoom && photonView != null && photonView.IsMine)
            photonView.RPC(nameof(RPC_StopTether), RpcTarget.All);

        if (runtimeRef != null && (photonView == null || photonView.IsMine))
            runtimeRef.StartCooldown(HeroSkillSlot.E);

        CleanupVfx();
        if (lineRenderer != null)
            lineRenderer.enabled = false;

        targetHealth = null;
    }

    [PunRPC]
    private void RPC_StartTether(int targetActorNumber)
    {
        if (photonView != null && photonView.IsMine)
            return;

        PlayerHealth target = FindPlayerByActor(targetActorNumber);
        if (target == null)
            return;

        targetHealth = target;
        tetherActive = true;
        SpawnTargetVfx();
        if (lineRenderer != null)
            lineRenderer.enabled = true;
    }

    [PunRPC]
    private void RPC_StopTether()
    {
        if (photonView != null && photonView.IsMine)
            return;

        tetherActive = false;
        CleanupVfx();
        if (lineRenderer != null)
            lineRenderer.enabled = false;
        targetHealth = null;
    }

    private void UpdateLine()
    {
        if (lineRenderer == null || !lineRenderer.enabled || targetHealth == null)
            return;

        Transform origin = tetherOrigin != null ? tetherOrigin : transform;
        Vector3 start = origin.position + lineStartOffset;
        Vector3 end = targetHealth.transform.position + lineEndOffset;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    private void SpawnTargetVfx()
    {
        if (targetHealth == null || targetVfxInstance != null)
            return;

        GameObject prefab = Resources.Load<GameObject>(healingVfxResource);
        if (prefab == null)
            return;

        targetVfxInstance = Instantiate(prefab, targetHealth.transform);
        targetVfxInstance.transform.localPosition = targetVfxOffset;
        targetVfxInstance.transform.localRotation = Quaternion.identity;
    }

    private void CleanupVfx()
    {
        if (targetVfxInstance != null)
        {
            Destroy(targetVfxInstance);
            targetVfxInstance = null;
        }
    }

    private PlayerHealth FindTarget(HeroRuntime runtime)
    {
        Transform origin = GetAimOrigin(runtime);
        if (origin == null)
            return null;

        Ray ray = new Ray(origin.position, origin.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRange, targetMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
            return null;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        PlayerHealth target = null;
        for (int i = 0; i < hits.Length; i++)
        {
            target = hits[i].transform.GetComponentInParent<PlayerHealth>();
            if (target != null && target != selfHealth)
                break;
        }

        if (target == null)
            return null;

        if (PhotonNetwork.InRoom)
        {
            if (!IsSameTeam(target))
                return null;
        }

        return target;
    }

    private Transform GetAimOrigin(HeroRuntime runtime)
    {
        if (aimOrigin != null)
            return aimOrigin;

        if (controller != null && controller.playerCamera != null)
            return controller.playerCamera.transform;

        if (Camera.main != null)
            return Camera.main.transform;

        return runtime != null ? runtime.transform : transform;
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
        HeroSkillDefinition def = runtime.GetSkill(HeroSkillSlot.E);
        return def != null && def.skillId == SkillIdConst;
    }
}
