using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public enum PayloadState
{
    Stopped = 0,
    Moving = 1,
    Contested = 2,
    Disabled = 3
}

[RequireComponent(typeof(PhotonView))]
public class PayloadController : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Path")]
    public Transform[] pathPoints;

    [Header("Movement")]
    public float baseSpeed = 1.5f;
    public bool useDynamicSpeed = false;
    public float speedPerAttacker = 0.5f;
    public float maxSpeed = 4f;

    [Header("Sabotage & Repair")]
    public bool enableSabotage = true;
    public float sabotageHoldTime = 8f;
    public float sabotageDisableDuration = 5f;
    public float sabotagePushDistance = 2f;
    public float repairDuration = 10f;
    public bool autoRecoverWhenDisabled = false;

    [Header("Checkpoint")]
    public int lastCheckpointIndex = -1;

    [Header("Debug")]
    public PayloadState CurrentState = PayloadState.Stopped;

    private float currentDistance;
    private float totalLength;
    private float[] segmentLengths;
    private bool reachedDestination;
    private float lastCheckpointDistance;
    private bool isDisabled;
    private float disabledUntilTime;
    private float sabotageProgress;
    private float repairProgress;

    public float Progress => totalLength > 0f ? Mathf.Clamp01(currentDistance / totalLength) : 0f;
    public bool IsDisabled => isDisabled;
    public float SabotageProgress => Mathf.Clamp01(sabotageProgress);
    public float RepairProgress => Mathf.Clamp01(repairProgress);
    public bool LocalPlayerInZone { get; private set; }

    private readonly HashSet<int> attackersInZone = new HashSet<int>();
    private readonly HashSet<int> defendersInZone = new HashSet<int>();
    private readonly HashSet<int> repairingAttackers = new HashSet<int>();
    private readonly HashSet<int> sabotagingDefenders = new HashSet<int>();

    private const string PAYLOAD_STATE_KEY = "PayloadState";
    private const string PAYLOAD_PROGRESS_KEY = "PayloadProgress";

    void Start()
    {
        CachePathData();
        EnsurePhysicsLocked();
        InitializeDistanceFromPosition();
    }

    void EnsurePhysicsLocked()
    {
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rb in bodies)
        {
            if (rb == null) continue;
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) return;
        if (pathPoints == null || pathPoints.Length < 2) return;

        if (PhotonNetwork.IsMasterClient)
        {
            PruneZoneLists();
            UpdateDisableAndRepair();
            UpdateState();
            MovePayload();
            SyncRoomProperties();
        }
    }

    void PruneZoneLists()
    {
        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        HashSet<int> aliveActors = new HashSet<int>();
        for (int i = 0; i < players.Length; i++)
        {
            PlayerHealth health = players[i];
            if (health == null || health.IsDead) continue;
            PhotonView view = health.GetComponent<PhotonView>();
            if (view == null || view.Owner == null) continue;
            aliveActors.Add(view.OwnerActorNr);
        }

        RemoveMissing(attackersInZone, aliveActors);
        RemoveMissing(defendersInZone, aliveActors);
        RemoveMissing(repairingAttackers, aliveActors);
        RemoveMissing(sabotagingDefenders, aliveActors);
    }

    void RemoveMissing(HashSet<int> set, HashSet<int> aliveActors)
    {
        if (set.Count == 0) return;
        set.RemoveWhere(actor => !aliveActors.Contains(actor));
    }

    void CachePathData()
    {
        if (pathPoints == null || pathPoints.Length < 2)
        {
            totalLength = 0f;
            segmentLengths = new float[0];
            return;
        }

        segmentLengths = new float[pathPoints.Length - 1];
        totalLength = 0f;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            float length = Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
            segmentLengths[i] = length;
            totalLength += length;
        }

        lastCheckpointDistance = 0f;
    }

    void InitializeDistanceFromPosition()
    {
        if (pathPoints == null || pathPoints.Length < 2 || segmentLengths == null || segmentLengths.Length == 0)
            return;

        float bestDistance = 0f;
        float bestSqr = float.MaxValue;
        float accumulated = 0f;
        Vector3 currentPos = transform.position;

        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            Vector3 a = pathPoints[i].position;
            Vector3 b = pathPoints[i + 1].position;
            Vector3 ab = b - a;
            float abSqr = ab.sqrMagnitude;
            float t = 0f;
            if (abSqr > 0f)
                t = Mathf.Clamp01(Vector3.Dot(currentPos - a, ab) / abSqr);

            Vector3 closest = a + ab * t;
            float sqr = (currentPos - closest).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                bestDistance = accumulated + segmentLengths[i] * t;
            }

            accumulated += segmentLengths[i];
        }

        currentDistance = Mathf.Clamp(bestDistance, 0f, totalLength);
        transform.position = EvaluatePosition(currentDistance);
    }

    void UpdateState()
    {
        if (isDisabled)
        {
            CurrentState = PayloadState.Disabled;
            return;
        }

        int attackers = attackersInZone.Count;
        int defenders = defendersInZone.Count;

        PayloadState newState;
        if (attackers > 0 && defenders == 0)
            newState = PayloadState.Moving;
        else if (attackers > 0 && defenders > 0)
            newState = PayloadState.Contested;
        else
            newState = PayloadState.Stopped;

        CurrentState = newState;
    }

    void MovePayload()
    {
        if (CurrentState != PayloadState.Moving || reachedDestination || totalLength <= 0f) return;

        float speed = baseSpeed;
        if (useDynamicSpeed)
        {
            float attackersWeight = GetAttackersPushWeight();
            speed = baseSpeed + Mathf.Max(0f, attackersWeight - 1f) * speedPerAttacker;
            speed = Mathf.Min(speed, maxSpeed);
        }

        currentDistance += speed * Time.deltaTime;
        if (currentDistance < lastCheckpointDistance)
            currentDistance = lastCheckpointDistance;
        if (currentDistance >= totalLength)
        {
            currentDistance = totalLength;
            reachedDestination = true;
            PayloadEscortMatchManager.Instance?.NotifyPayloadReachedDestination();
        }

        transform.position = EvaluatePosition(currentDistance);
    }

    Vector3 EvaluatePosition(float distance)
    {
        if (segmentLengths == null || segmentLengths.Length == 0) return transform.position;

        float remaining = distance;
        for (int i = 0; i < segmentLengths.Length; i++)
        {
            float segmentLength = segmentLengths[i];
            if (remaining <= segmentLength)
            {
                float t = segmentLength > 0f ? remaining / segmentLength : 0f;
                return Vector3.Lerp(pathPoints[i].position, pathPoints[i + 1].position, t);
            }
            remaining -= segmentLength;
        }

        return pathPoints[pathPoints.Length - 1].position;
    }

    void SyncRoomProperties()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (totalLength <= 0f) return;

        float progress = Mathf.Clamp01(currentDistance / totalLength);
        Hashtable props = new Hashtable
        {
            { PAYLOAD_STATE_KEY, (int)CurrentState },
            { PAYLOAD_PROGRESS_KEY, progress }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public void NotifyCheckpointReached(int checkpointIndex)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (checkpointIndex <= lastCheckpointIndex) return;
        lastCheckpointIndex = checkpointIndex;
        lastCheckpointDistance = Mathf.Clamp(GetDistanceToCheckpoint(checkpointIndex), 0f, totalLength);

        if (PayloadEscortMatchManager.Instance != null)
        {
            PayloadEscortMatchManager.Instance.AddTeamScore(PayloadTeam.Attackers, PayloadEscortMatchManager.Instance.checkpointScoreReward);
        }
    }

    float GetDistanceToCheckpoint(int checkpointIndex)
    {
        if (segmentLengths == null || segmentLengths.Length == 0) return 0f;
        int clampedIndex = Mathf.Clamp(checkpointIndex, 0, pathPoints.Length - 1);
        float distance = 0f;
        for (int i = 0; i < clampedIndex; i++)
        {
            if (i < segmentLengths.Length)
                distance += segmentLengths[i];
        }
        return distance;
    }

    public void ReportPlayerZoneStatus(int actorNumber, bool isInside)
    {
        photonView.RPC("RPC_UpdateZone", RpcTarget.MasterClient, actorNumber, isInside);
    }

    public void SetLocalPlayerInZone(bool isInside)
    {
        LocalPlayerInZone = isInside;
    }

    public void ReportRepairStatus(int actorNumber, bool isRepairing)
    {
        photonView.RPC("RPC_UpdateRepair", RpcTarget.MasterClient, actorNumber, isRepairing);
    }

    public void ReportSabotageStatus(int actorNumber, bool isSabotaging)
    {
        photonView.RPC("RPC_UpdateSabotage", RpcTarget.MasterClient, actorNumber, isSabotaging);
    }

    [PunRPC]
    void RPC_UpdateZone(int actorNumber, bool isInside)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!PayloadTeamUtils.TryGetPlayerTeam(actorNumber, out PayloadTeam team)) return;

        HashSet<int> set = team == PayloadTeam.Attackers ? attackersInZone : defendersInZone;
        if (isInside)
            set.Add(actorNumber);
        else
            set.Remove(actorNumber);
    }

    [PunRPC]
    void RPC_UpdateRepair(int actorNumber, bool isRepairing)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!PayloadTeamUtils.TryGetPlayerTeam(actorNumber, out PayloadTeam team)) return;
        if (team != PayloadTeam.Attackers) return;

        if (isRepairing)
            repairingAttackers.Add(actorNumber);
        else
            repairingAttackers.Remove(actorNumber);
    }

    [PunRPC]
    void RPC_UpdateSabotage(int actorNumber, bool isSabotaging)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!PayloadTeamUtils.TryGetPlayerTeam(actorNumber, out PayloadTeam team)) return;
        if (team != PayloadTeam.Defenders) return;

        if (isSabotaging)
            sabotagingDefenders.Add(actorNumber);
        else
            sabotagingDefenders.Remove(actorNumber);
    }

    void UpdateDisableAndRepair()
    {
        if (!enableSabotage) return;

        if (isDisabled)
        {
            if (repairingAttackers.Count > 0)
            {
                float repairWeight = GetRepairWeight();
                float rate = repairDuration > 0f ? (Time.deltaTime * repairWeight) / repairDuration : 1f;
                repairProgress = Mathf.Clamp01(repairProgress + rate);
                if (repairProgress >= 1f)
                {
                    isDisabled = false;
                    repairProgress = 0f;
                    sabotageProgress = 0f;
                }
            }
            else
            {
                repairProgress = 0f;
            }

            if (autoRecoverWhenDisabled && Time.time >= disabledUntilTime)
            {
                isDisabled = false;
                repairProgress = 0f;
                sabotageProgress = 0f;
            }

            return;
        }

        if (sabotagingDefenders.Count > 0 && attackersInZone.Count == 0)
        {
            float sabotageWeight = GetSabotageWeight();
            float rate = sabotageHoldTime > 0f ? (Time.deltaTime * sabotageWeight) / sabotageHoldTime : 1f;
            sabotageProgress = Mathf.Clamp01(sabotageProgress + rate);
            if (sabotageProgress >= 1f)
            {
                isDisabled = true;
                disabledUntilTime = autoRecoverWhenDisabled ? Time.time + sabotageDisableDuration : float.MaxValue;
                sabotageProgress = 0f;
                repairProgress = 0f;
                currentDistance = Mathf.Max(lastCheckpointDistance, currentDistance - sabotagePushDistance);
                transform.position = EvaluatePosition(currentDistance);
            }
        }
        else
        {
            sabotageProgress = 0f;
        }
    }

    float GetAttackersPushWeight()
    {
        float weight = 0f;
        foreach (int actorNumber in attackersInZone)
        {
            if (PlayerRoleUtils.TryGetPlayerRole(actorNumber, out PlayerRole role))
                weight += PlayerRoleUtils.GetPushWeight(role);
            else
                weight += 1f;
        }
        return weight;
    }

    float GetRepairWeight()
    {
        float weight = 0f;
        foreach (int actorNumber in repairingAttackers)
        {
            if (PlayerRoleUtils.TryGetPlayerRole(actorNumber, out PlayerRole role))
                weight += PlayerRoleUtils.GetRepairWeight(role);
            else
                weight += 1f;
        }
        return weight;
    }

    float GetSabotageWeight()
    {
        float weight = 0f;
        foreach (int actorNumber in sabotagingDefenders)
        {
            if (PlayerRoleUtils.TryGetPlayerRole(actorNumber, out PlayerRole role))
                weight += PlayerRoleUtils.GetSabotageWeight(role);
            else
                weight += 1f;
        }
        return weight;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient || otherPlayer == null) return;
        attackersInZone.Remove(otherPlayer.ActorNumber);
        defendersInZone.Remove(otherPlayer.ActorNumber);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(currentDistance);
            stream.SendNext((int)CurrentState);
            stream.SendNext(isDisabled);
            stream.SendNext(sabotageProgress);
            stream.SendNext(repairProgress);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            currentDistance = (float)stream.ReceiveNext();
            CurrentState = (PayloadState)(int)stream.ReceiveNext();
            isDisabled = (bool)stream.ReceiveNext();
            sabotageProgress = (float)stream.ReceiveNext();
            repairProgress = (float)stream.ReceiveNext();
        }
    }
}
