using UnityEngine;
using Photon.Pun;

public class EngineerDeployTurretSkill : HeroSkillBehaviour, IHeroHoldSkill
{
    [Header("Turret Prefab")]
    [Tooltip("Prefab name inside Resources (e.g., TurretPrefab)")]
    public string turretResourceName = "Turret";

    [Header("Placement")]
    public Transform placementOrigin;
    public float maxDistance = 8f;
    public LayerMask placementMask = ~0;
    public float minGroundNormal = 0.6f;
    public Vector3 spawnOffset = Vector3.zero;
    public float placementGraceTime = 0.2f;

    [Header("Preview")]
    public GameObject previewPrefab;

    private bool isHolding;
    private bool hasValidPlacement;
    private Vector3 lastPlacement;
    private Quaternion lastRotation;
    private GameObject previewInstance;
    private float lastValidTime;

    public override void Activate(HeroRuntime runtime)
    {
        // Not used: hold-to-place uses BeginHold/EndHold.
    }

    public void BeginHold(HeroRuntime runtime)
    {
        if (runtime == null) return;

        if (runtime.GetCooldownRemaining(HeroSkillSlot.E) > 0f)
            return;

        if (HasExistingTurretForOwner(runtime))
            return;

        EngineerTurretState state = runtime.GetComponent<EngineerTurretState>();
        if (state != null && state.HasActiveTurret)
            return;

        isHolding = true;
        CreatePreview();
    }

    public void UpdateHold(HeroRuntime runtime)
    {
        if (!isHolding || runtime == null) return;

        Transform origin = GetOrigin(runtime);
        if (origin == null) return;

        hasValidPlacement = false;

        if (TryGetPlacementHit(origin, out RaycastHit hit))
        {
            if (hit.normal.y >= minGroundNormal)
            {
                hasValidPlacement = true;
                lastValidTime = Time.time;
                lastPlacement = hit.point + spawnOffset;
                Vector3 forward = Vector3.ProjectOnPlane(origin.forward, Vector3.up).normalized;
                if (forward.sqrMagnitude < 0.01f)
                    forward = Vector3.forward;
                lastRotation = Quaternion.LookRotation(forward, Vector3.up);
            }
        }

        if (!hasValidPlacement && Time.time - lastValidTime <= placementGraceTime)
            hasValidPlacement = true;

        UpdatePreview();
    }

    public void EndHold(HeroRuntime runtime)
    {
        if (!isHolding) return;
        isHolding = false;

        if (runtime == null)
        {
            DestroyPreview();
            return;
        }

        if (HasExistingTurretForOwner(runtime))
        {
            DestroyPreview();
            return;
        }

        if (!hasValidPlacement)
        {
            DestroyPreview();
            return;
        }

        GameObject turretInstance = null;
        if (PhotonNetwork.InRoom)
        {
            turretInstance = PhotonNetwork.Instantiate(turretResourceName, lastPlacement, lastRotation);
        }
        else
        {
            GameObject prefab = Resources.Load<GameObject>(turretResourceName);
            if (prefab != null)
                turretInstance = Object.Instantiate(prefab, lastPlacement, lastRotation);
        }

        if (turretInstance != null)
        {
            EngineerTurretState state = runtime.GetComponent<EngineerTurretState>();
            if (state == null)
                state = runtime.gameObject.AddComponent<EngineerTurretState>();

            state.SetTurret(turretInstance);

            PhotonView ownerView = runtime.GetComponent<PhotonView>();
            int ownerActor = ownerView != null ? ownerView.OwnerActorNr : (PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : -1);
            TurretOwner turretOwner = turretInstance.GetComponent<TurretOwner>();
            if (turretOwner == null)
                turretOwner = turretInstance.AddComponent<TurretOwner>();
            turretOwner.SetOwner(ownerActor);

            TurretAutoAttack autoAttack = turretInstance.GetComponent<TurretAutoAttack>();
            if (autoAttack == null)
                autoAttack = turretInstance.AddComponent<TurretAutoAttack>();

            TurretHealth health = turretInstance.GetComponent<TurretHealth>();
            if (health == null)
                health = turretInstance.AddComponent<TurretHealth>();

            HeroRuntime cachedRuntime = runtime;
            health.Destroyed += () =>
            {
                state.ClearTurret();
                cachedRuntime.StartCooldown(HeroSkillSlot.E);
            };

            runtime.NotifySkillActivated(HeroSkillSlot.E);
        }

        DestroyPreview();
    }

    private Transform GetOrigin(HeroRuntime runtime)
    {
        Transform origin = placementOrigin;

        if (origin == null)
        {
            PlayerControllerNetwork controller = runtime.GetComponent<PlayerControllerNetwork>();
            if (controller != null && controller.playerCamera != null)
                origin = controller.playerCamera.transform;
        }

        if (origin == null && Camera.main != null)
            origin = Camera.main.transform;

        return origin;
    }

    private void CreatePreview()
    {
        if (previewInstance != null) return;

        if (previewPrefab != null)
        {
            previewInstance = Object.Instantiate(previewPrefab);
        }
        else
        {
            previewInstance = BuildDefaultPreview();
        }
    }

    private void UpdatePreview()
    {
        if (previewInstance == null) return;
        previewInstance.SetActive(hasValidPlacement);
        if (!hasValidPlacement) return;

        previewInstance.transform.position = lastPlacement;
        previewInstance.transform.rotation = lastRotation;
    }

    private void DestroyPreview()
    {
        if (previewInstance == null) return;
        Object.Destroy(previewInstance);
        previewInstance = null;
    }

    private bool TryGetPlacementHit(Transform origin, out RaycastHit hit)
    {
        hit = default;
        if (origin == null) return false;

        Ray ray = new Ray(origin.position, origin.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, placementMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        Transform selfRoot = origin.root;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform == null) continue;
            if (hits[i].transform.root == selfRoot) continue;
            hit = hits[i];
            return true;
        }

        return false;
    }

    private GameObject BuildDefaultPreview()
    {
        GameObject root = new GameObject("TurretPlacementPreview");

        Material previewMat = CreatePreviewMaterial(new Color(0.2f, 0.9f, 1f, 0.35f));

        GameObject disk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disk.name = "PreviewDisk";
        disk.transform.SetParent(root.transform, false);
        disk.transform.localScale = new Vector3(0.8f, 0.02f, 0.8f);
        if (previewMat != null)
            disk.GetComponent<Renderer>().material = previewMat;
        Collider diskCol = disk.GetComponent<Collider>();
        if (diskCol != null) diskCol.enabled = false;

        GameObject ring = new GameObject("PreviewRing");
        ring.transform.SetParent(root.transform, false);
        LineRenderer line = ring.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 48;
        line.widthMultiplier = 0.03f;
        line.material = CreatePreviewMaterial(new Color(0.2f, 0.9f, 1f, 0.9f));

        float radius = 0.85f;
        for (int i = 0; i < line.positionCount; i++)
        {
            float t = (float)i / (line.positionCount - 1) * Mathf.PI * 2f;
            line.SetPosition(i, new Vector3(Mathf.Cos(t) * radius, 0.05f, Mathf.Sin(t) * radius));
        }

        return root;
    }

    private Material CreatePreviewMaterial(Color color)
    {
        Shader shader = Shader.Find("Unlit/Color");
        if (shader == null)
            shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");
        if (shader == null) return null;

        Material mat = new Material(shader);
        mat.color = color;
        return mat;
    }

    private bool HasExistingTurretForOwner(HeroRuntime runtime)
    {
        PhotonView ownerView = runtime.GetComponent<PhotonView>();
        int ownerActor = ownerView != null ? ownerView.OwnerActorNr : (PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : -1);
        if (ownerActor <= 0) return false;

        TurretOwner[] turrets = FindObjectsByType<TurretOwner>(FindObjectsSortMode.None);
        for (int i = 0; i < turrets.Length; i++)
        {
            TurretOwner turret = turrets[i];
            if (turret == null) continue;
            if (turret.OwnerActorNumber == ownerActor)
                return true;
        }

        return false;
    }
}
