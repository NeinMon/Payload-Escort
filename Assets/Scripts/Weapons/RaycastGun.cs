using UnityEngine;
using Photon.Pun;

public class RaycastGun : WeaponBase
{
    [Header("Raycast Settings")]
    public float shootRange = 100f;
    public LayerMask hitLayers;
    public GameObject hitEffectPrefab;
    public Camera fpsCamera;
    public bool useCameraCenterRay = true;

    [Header("Spread")]
    public float minSpread = 0.002f;
    public float maxSpread = 0.05f;
    public float spreadIncreasePerShot = 0.005f;
    public float spreadRecoverPerSecond = 0.025f;
    public float currentSpread = 0.002f;

    [Header("Recoil")]
    public bool applyCameraRecoil = true;
    
    [Header("Visual")]
    public LineRenderer bulletTrail;
    public float trailDuration = 0.05f;
    public bool spawnHitEffectLocally = true;
    public bool spawnHitEffectOnNetwork = false;
    public float hitEffectLifetime = 5f;

    private PlayerControllerNetwork playerController;
    private FPSController fpsController;

    void Awake()
    {
        if (firePoint == null)
        {
            Transform found = transform.Find("FirePoint");
            if (found != null)
                firePoint = found;
        }

        if (fpsCamera == null)
        {
            fpsCamera = GetComponentInParent<Camera>();
            if (fpsCamera == null)
            {
                Camera[] cams = GetComponentsInParent<Camera>(true);
                if (cams != null && cams.Length > 0)
                    fpsCamera = cams[0];
            }
            if (fpsCamera == null)
                fpsCamera = Camera.main;
        }
    }

    protected override void Start()
    {
        base.Start();
        currentSpread = Mathf.Clamp(currentSpread, minSpread, maxSpread);
        playerController = GetComponentInParent<PlayerControllerNetwork>();
        fpsController = GetComponentInParent<FPSController>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (currentSpread > minSpread)
        {
            currentSpread = Mathf.MoveTowards(currentSpread, minSpread, spreadRecoverPerSecond * Time.deltaTime);
        }
    }
    
    protected override void OnFire()
    {
        if (PhotonNetwork.IsConnected && photonView != null && !photonView.IsMine) return;

        currentSpread = Mathf.Clamp(currentSpread + spreadIncreasePerShot, minSpread, maxSpread);
        TryApplyRecoil();

        int mask = hitLayers.value == 0 ? Physics.DefaultRaycastLayers : hitLayers;

        Vector3 cameraOrigin = fpsCamera != null ? fpsCamera.transform.position : transform.position;
        Vector3 cameraDir = GetShootDirection();

        Vector3 aimPoint = cameraOrigin + cameraDir * shootRange;
        if (TryGetValidHit(cameraOrigin, cameraDir, shootRange, mask, out RaycastHit cameraHit))
        {
            aimPoint = cameraHit.point;
        }

        Vector3 fireOrigin = firePoint != null ? firePoint.position : transform.position;
        Vector3 fireDir = (aimPoint - fireOrigin).normalized;
        Ray ray = new Ray(fireOrigin, fireDir);
        RaycastHit hit;
        
        if (TryGetValidHit(ray.origin, ray.direction, shootRange, mask, out hit))
        {
            Debug.Log($"[RaycastGun] Hit: {hit.transform.name}");
            
            // Visual hit effect
            if (hitEffectPrefab != null)
            {
                SpawnHitEffect(hit.point, Quaternion.LookRotation(hit.normal));
            }
            
            // Check if hit a player
            PlayerHealth targetHealth = hit.transform.GetComponent<PlayerHealth>();
            PhotonView targetPhotonView = hit.transform.GetComponent<PhotonView>();
            
            if (targetHealth != null && targetPhotonView != null)
            {
                // Send damage over network with attacker ID
                int attackerID = photonView.Owner.ActorNumber;
                targetPhotonView.RPC("TakeDamageFromPlayer", RpcTarget.All, damage, attackerID);
                
                // Show hitmarker for local player
                LocalPlayerHUD hud = GetComponentInParent<LocalPlayerHUD>();
                if (hud != null)
                {
                    hud.ShowHitmarker();
                }
                
                Debug.Log($"[RaycastGun] Damaged {targetPhotonView.Owner.NickName} for {damage} damage");
            }

            TurretHealth turretHealth = hit.transform.GetComponentInParent<TurretHealth>();
            if (turretHealth != null)
            {
                bool allowDamage = true;
                TurretOwner turretOwner = turretHealth.GetComponent<TurretOwner>();

                if (turretOwner != null)
                {
                    if (turretOwner.OwnerActorNumber == photonView.Owner.ActorNumber)
                        allowDamage = false;

                    if (allowDamage && turretOwner.HasOwnerTeam && PayloadTeamUtils.TryGetPlayerTeam(photonView.Owner, out PayloadTeam shooterTeam))
                    {
                        if (shooterTeam == turretOwner.OwnerTeam)
                            allowDamage = false;
                    }
                }

                if (allowDamage)
                {
                    PhotonView turretView = turretHealth.GetComponent<PhotonView>();
                    int attackerID = photonView.Owner.ActorNumber;
                    if (turretView != null && PhotonNetwork.InRoom)
                        turretView.RPC("TakeDamageFromPlayer", RpcTarget.All, damage, attackerID);
                    else
                        turretHealth.TakeDamageFromPlayer(damage, attackerID);

                    LocalPlayerHUD hud = GetComponentInParent<LocalPlayerHUD>();
                    if (hud != null)
                        hud.ShowHitmarker();
                }
            }
            
            // Show bullet trail
            if (bulletTrail != null)
            {
                Vector3 trailStart = fireOrigin;
                StartCoroutine(ShowBulletTrail(trailStart, hit.point));
            }
        }
        else
        {
            // Missed shot - still show trail
            if (bulletTrail != null)
            {
                Vector3 trailStart = fireOrigin;
                Vector3 trailEnd = ray.origin + ray.direction * shootRange;
                StartCoroutine(ShowBulletTrail(trailStart, trailEnd));
            }
        }
    }

    bool TryGetValidHit(Vector3 origin, Vector3 direction, float distance, int mask, out RaycastHit hit)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, mask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
        {
            hit = default;
            return false;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        Transform selfRoot = transform.root;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform == null) continue;
            if (hits[i].transform.root == selfRoot) continue;
            hit = hits[i];
            return true;
        }

        hit = default;
        return false;
    }
    
    System.Collections.IEnumerator ShowBulletTrail(Vector3 start, Vector3 end)
    {
        bulletTrail.SetPosition(0, start);
        bulletTrail.SetPosition(1, end);
        bulletTrail.enabled = true;
        
        yield return new WaitForSeconds(trailDuration);
        
        bulletTrail.enabled = false;
    }

    void SpawnHitEffect(Vector3 position, Quaternion rotation)
    {
        if (spawnHitEffectLocally)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, rotation);
            Destroy(effect, Mathf.Max(0.1f, hitEffectLifetime));
        }

        if (spawnHitEffectOnNetwork && PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            PhotonNetwork.Instantiate(hitEffectPrefab.name, position, rotation);
        }
    }

    Vector3 GetShootDirection()
    {
        if (fpsCamera == null)
            return transform.forward;

        Vector3 dir = fpsCamera.transform.forward;
        if (!useCameraCenterRay)
            return dir.normalized;

        dir += fpsCamera.transform.right * Random.Range(-currentSpread, currentSpread);
        dir += fpsCamera.transform.up * Random.Range(-currentSpread, currentSpread);
        return dir.normalized;
    }

    void TryApplyRecoil()
    {
        if (!applyCameraRecoil) return;

        if (playerController != null)
        {
            playerController.AddRecoil();
            return;
        }

        if (fpsController != null)
        {
            fpsController.AddRecoil();
        }
    }
}
