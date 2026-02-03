using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class TurretAutoAttack : MonoBehaviourPun
{
    [Header("Stats")]
    public float damage = 15f;
    public float shotsPerSecond = 2f;
    public float range = 20f;
    public int targetsPerShot = 1;

    [Header("Targeting")]
    public LayerMask targetMask = ~0;
    public LayerMask lineOfSightMask = ~0;
    public bool requireLineOfSight = true;
    public Transform muzzle;
    public Transform yawPivot;
    public Transform pitchPivot;
    public float turnSpeed = 360f;
    public float targetLockDuration = 0.25f;

    [Header("Attack Indicator")]
    public GameObject attackIndicator;
    public float indicatorDuration = 0.08f;
    public ParticleSystem muzzleFlash;

    [Header("Visibility")]
    public LayerMask visibilityMask = ~0;

    [Header("Network Sync")]
    public float aimSendInterval = 0.1f;

    private TurretOwner owner;
    private float nextShotTime;
    private Coroutine indicatorRoutine;
    private readonly List<PlayerHealth> cachedTargets = new List<PlayerHealth>();
    private float lastAimSendTime;
    private PlayerHealth lockedTarget;
    private float lockedUntilTime;
    private Quaternion remoteYawTarget;
    private Quaternion remotePitchTarget;
    private bool fxActive;

    private bool overdriveActive;
    private float overdriveDamageMultiplier = 1f;
    private float overdriveRangeMultiplier = 1f;
    private int overdriveExtraTargets;
    private Coroutine overdriveRoutine;

    void Awake()
    {
        owner = GetComponent<TurretOwner>();
        ConfigureMuzzleFlash();
    }

    void Update()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            UpdateRemoteAim();
            return;
        }

        PlayerHealth target = GetLockedOrBestTarget();
        UpdateAim(target);

        if (target != null && PhotonNetwork.InRoom && Time.time - lastAimSendTime >= aimSendInterval)
        {
            lastAimSendTime = Time.time;
            SendAimSync();
        }

        if (target == null)
        {
            StopAttackFx();
            return;
        }

        if (Time.time >= nextShotTime)
        {
            nextShotTime = Time.time + (shotsPerSecond > 0f ? 1f / shotsPerSecond : 0.5f);
            FireAtTargets(target);
        }
    }

    public void ApplyOverdrive(float duration, float damageMultiplier, float rangeMultiplier, int extraTargets)
    {
        if (overdriveRoutine != null)
            StopCoroutine(overdriveRoutine);

        overdriveRoutine = StartCoroutine(OverdriveRoutine(duration, damageMultiplier, rangeMultiplier, extraTargets));
    }

    private IEnumerator OverdriveRoutine(float duration, float damageMultiplier, float rangeMultiplier, int extraTargets)
    {
        overdriveActive = true;
        overdriveDamageMultiplier = Mathf.Max(1f, damageMultiplier);
        overdriveRangeMultiplier = Mathf.Max(1f, rangeMultiplier);
        overdriveExtraTargets = Mathf.Max(0, extraTargets);

        yield return new WaitForSeconds(duration);

        overdriveActive = false;
        overdriveDamageMultiplier = 1f;
        overdriveRangeMultiplier = 1f;
        overdriveExtraTargets = 0;
        overdriveRoutine = null;
    }

    private float CurrentRange => range * (overdriveActive ? overdriveRangeMultiplier : 1f);
    private float CurrentDamage => damage * (overdriveActive ? overdriveDamageMultiplier : 1f);
    private int CurrentTargetsPerShot => targetsPerShot + (overdriveActive ? overdriveExtraTargets : 0);

    private PlayerHealth FindBestTarget()
    {
        cachedTargets.Clear();
        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        float bestDistance = float.MaxValue;
        PlayerHealth bestTarget = null;
        float currentRange = CurrentRange;
        Vector3 origin = muzzle != null ? muzzle.position : transform.position;

        for (int i = 0; i < players.Length; i++)
        {
            PlayerHealth health = players[i];
            if (health == null || health.IsDead) continue;

            PhotonView targetView = health.GetComponent<PhotonView>();
            if (targetView == null) continue;

            if (targetView.IsMine)
                continue;

            if (owner != null)
            {
                if (targetView.Owner != null && targetView.Owner.ActorNumber == owner.OwnerActorNumber)
                    continue;

                if (owner.HasOwnerTeam && PayloadTeamUtils.TryGetPlayerTeam(targetView.Owner, out PayloadTeam targetTeam))
                {
                    if (targetTeam == owner.OwnerTeam) continue;
                }
            }

            Vector3 targetPos = health.transform.position + Vector3.up * 1.2f;
            float dist = Vector3.Distance(origin, targetPos);
            if (dist > currentRange) continue;

            if (requireLineOfSight && !HasLineOfSight(origin, targetPos, health.transform))
                continue;

            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestTarget = health;
            }
        }

        return bestTarget;
    }

    private PlayerHealth GetLockedOrBestTarget()
    {
        if (lockedTarget != null)
        {
            if (!lockedTarget.IsDead && Time.time <= lockedUntilTime)
            {
                float dist = Vector3.Distance(muzzle != null ? muzzle.position : transform.position, lockedTarget.transform.position);
                if (dist <= CurrentRange)
                    return lockedTarget;
            }

            lockedTarget = null;
        }

        PlayerHealth best = FindBestTarget();
        if (best != null)
        {
            lockedTarget = best;
            lockedUntilTime = Time.time + targetLockDuration;
        }

        return best;
    }

    private bool HasLineOfSight(Vector3 origin, Vector3 targetPos, Transform targetTransform)
    {
        if (Physics.Linecast(origin, targetPos, out RaycastHit hit, lineOfSightMask))
        {
            if (hit.transform == targetTransform || hit.transform.IsChildOf(targetTransform))
                return true;
            return false;
        }
        return true;
    }

    private void UpdateAim(PlayerHealth target)
    {
        if (target == null) return;
        Vector3 aimPoint = target.transform.position + Vector3.up * 1.2f;
        Vector3 direction = (aimPoint - transform.position).normalized;

        if (yawPivot != null)
        {
            Vector3 flatDir = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (flatDir.sqrMagnitude > 0.0001f)
            {
                Quaternion desired = Quaternion.LookRotation(flatDir, Vector3.up);
                yawPivot.rotation = Quaternion.RotateTowards(yawPivot.rotation, desired, turnSpeed * Time.deltaTime);
            }
        }

        if (pitchPivot != null)
        {
            Vector3 localDir = (aimPoint - pitchPivot.position).normalized;
            Quaternion desired = Quaternion.LookRotation(localDir, Vector3.up);
            pitchPivot.rotation = Quaternion.RotateTowards(pitchPivot.rotation, desired, turnSpeed * Time.deltaTime);
        }
    }

    private void UpdateRemoteAim()
    {
        if (yawPivot != null)
            yawPivot.rotation = Quaternion.Slerp(yawPivot.rotation, remoteYawTarget, Time.deltaTime * 12f);

        if (pitchPivot != null)
            pitchPivot.rotation = Quaternion.Slerp(pitchPivot.rotation, remotePitchTarget, Time.deltaTime * 12f);
    }

    private void SendAimSync()
    {
        if (!PhotonNetwork.InRoom || !photonView.IsMine) return;

        Vector3 yawEuler = yawPivot != null ? yawPivot.rotation.eulerAngles : transform.rotation.eulerAngles;
        Vector3 pitchEuler = pitchPivot != null ? pitchPivot.rotation.eulerAngles : Vector3.zero;
        photonView.RPC("RPC_SyncAim", RpcTarget.Others, yawEuler, pitchEuler);
    }

    private void FireAtTargets(PlayerHealth primaryTarget)
    {
        int shots = Mathf.Max(1, CurrentTargetsPerShot);
        List<PlayerHealth> targets = GetNearestTargets(primaryTarget, shots);
        for (int i = 0; i < targets.Count; i++)
        {
            ApplyDamage(targets[i]);
        }

        PlayAttackIndicator();
        if (PhotonNetwork.InRoom && photonView.IsMine)
            photonView.RPC("RPC_PlayAttackFx", RpcTarget.Others);
    }

    private List<PlayerHealth> GetNearestTargets(PlayerHealth primaryTarget, int maxTargets)
    {
        List<PlayerHealth> targets = new List<PlayerHealth>();
        if (primaryTarget != null) targets.Add(primaryTarget);

        if (maxTargets <= 1) return targets;

        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        float currentRange = CurrentRange;
        Vector3 origin = muzzle != null ? muzzle.position : transform.position;

        for (int i = 0; i < players.Length; i++)
        {
            PlayerHealth health = players[i];
            if (health == null || health.IsDead || health == primaryTarget) continue;

            PhotonView targetView = health.GetComponent<PhotonView>();
            if (targetView == null) continue;

            if (targetView.IsMine)
                continue;

            if (owner != null && owner.HasOwnerTeam && PayloadTeamUtils.TryGetPlayerTeam(targetView.Owner, out PayloadTeam targetTeam))
            {
                if (targetTeam == owner.OwnerTeam) continue;
            }

            Vector3 targetPos = health.transform.position + Vector3.up * 1.2f;
            float dist = Vector3.Distance(origin, targetPos);
            if (dist > currentRange) continue;

            if (requireLineOfSight && !HasLineOfSight(origin, targetPos, health.transform))
                continue;

            targets.Add(health);
            if (targets.Count >= maxTargets) break;
        }

        return targets;
    }

    private void ApplyDamage(PlayerHealth target)
    {
        if (target == null) return;
        PhotonView targetView = target.GetComponent<PhotonView>();
        if (targetView == null) return;

        int attackerActor = owner != null ? owner.OwnerActorNumber : (PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : -1);
        float dmg = CurrentDamage;

        if (PhotonNetwork.InRoom)
            targetView.RPC("TakeDamageFromPlayer", RpcTarget.All, dmg, attackerActor);
        else
            target.TakeDamageFromPlayer(dmg, attackerActor);
    }

    private void PlayAttackIndicator()
    {
        if (!IsMuzzleVisible())
            return;

        fxActive = true;
        if (muzzleFlash != null)
        {
            if (!muzzleFlash.gameObject.activeSelf)
                muzzleFlash.gameObject.SetActive(true);

            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play();
        }

        if (attackIndicator == null) return;

        if (muzzleFlash != null && IsSameFxRoot(attackIndicator, muzzleFlash.transform))
            return;

        if (indicatorRoutine != null)
            StopCoroutine(indicatorRoutine);
        indicatorRoutine = StartCoroutine(IndicatorRoutine());
    }

    private void StopAttackFx()
    {
        if (!fxActive) return;
        fxActive = false;

        if (muzzleFlash != null)
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (attackIndicator != null)
            attackIndicator.SetActive(false);

        if (PhotonNetwork.InRoom && photonView.IsMine)
            photonView.RPC("RPC_StopAttackFx", RpcTarget.Others);
    }

    private IEnumerator IndicatorRoutine()
    {
        attackIndicator.SetActive(true);
        yield return new WaitForSeconds(indicatorDuration);
        attackIndicator.SetActive(false);
        indicatorRoutine = null;
    }

    private bool IsSameFxRoot(GameObject indicatorObject, Transform muzzleTransform)
    {
        if (indicatorObject == null || muzzleTransform == null) return false;

        Transform indicatorTransform = indicatorObject.transform;
        return indicatorTransform == muzzleTransform ||
               indicatorTransform.IsChildOf(muzzleTransform) ||
               muzzleTransform.IsChildOf(indicatorTransform);
    }

    [PunRPC]
    private void RPC_SyncAim(Vector3 yawEuler, Vector3 pitchEuler)
    {
        if (photonView.IsMine) return;

        remoteYawTarget = Quaternion.Euler(yawEuler);
        remotePitchTarget = Quaternion.Euler(pitchEuler);
    }

    [PunRPC]
    private void RPC_PlayAttackFx()
    {
        if (photonView.IsMine) return;
        PlayAttackIndicator();
    }

    [PunRPC]
    private void RPC_StopAttackFx()
    {
        if (photonView.IsMine) return;

        fxActive = false;
        if (muzzleFlash != null)
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (attackIndicator != null)
            attackIndicator.SetActive(false);
    }

    private void ConfigureMuzzleFlash()
    {
        if (muzzleFlash == null) return;

        ParticleSystem.MainModule main = muzzleFlash.main;
        main.playOnAwake = false;

        muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        SetLayerRecursively(muzzleFlash.gameObject, gameObject.layer);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;
        obj.layer = layer;
        Transform t = obj.transform;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i).gameObject, layer);
    }

    private bool IsMuzzleVisible()
    {
        Camera cam = Camera.main;
        if (cam == null) return true;

        Vector3 origin = cam.transform.position;
        Vector3 target = muzzle != null ? muzzle.position : transform.position;
        Vector3 dir = target - origin;
        float dist = dir.magnitude;
        if (dist <= 0.01f) return true;
        dir /= dist;

        RaycastHit[] hits = Physics.RaycastAll(origin, dir, dist, visibilityMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return true;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        Transform selfRoot = transform.root;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform == null) continue;
            if (hits[i].transform.root == selfRoot) continue;
            return false;
        }

        return true;
    }
}
