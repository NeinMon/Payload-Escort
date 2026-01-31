using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;
using System.Collections.Generic;

public class PayloadEscortMatchManager : MonoBehaviourPunCallbacks
{
    public static PayloadEscortMatchManager Instance;

    [Header("Match Settings")]
    public int minPlayers = 2;
    public int maxPlayers = 10;
    public float matchDuration = 600f;

    [Header("Checkpoint Rewards")]
    public int checkpointScoreReward = 10;

    [Header("Spawning")]
    public string playerPrefabName = "Player";
    public Transform[] attackerSpawnPoints;
    public Transform[] defenderSpawnPoints;
    public bool autoSpawnPlayer = true;
    public bool forceLocalTeam = false;
    public PayloadTeam forcedLocalTeam = PayloadTeam.Defenders;

    [Header("Payload")]
    public PayloadController payloadController;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI stateText;
    public TextMeshProUGUI winnerText;
    public GameObject matchEndUI;

    private float timer;
    private bool matchActive;
    private bool localPlayerSpawned;
    private bool roomInitialized;
    private int attackersScore;
    private int defendersScore;
    private float nextSpawnCheckTime;
    private bool loggedMissingPrefab;

    public int AttackersScore => attackersScore;
    public int DefendersScore => defendersScore;

    public float CurrentTimer => timer;
    public bool MatchActive => matchActive;

    private const string TIMER_KEY = "PayloadMatchTimer";
    private const string MATCH_ACTIVE_KEY = "PayloadMatchActive";
    private const string WINNER_TEAM_KEY = "PayloadWinnerTeam";
    private const string ATTACKERS_SCORE_KEY = "PayloadAttackersScore";
    private const string DEFENDERS_SCORE_KEY = "PayloadDefendersScore";

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    void Start()
    {
        SingleAudioListenerEnforcer.EnsureSingleListener();
        InitializeRoomIfNeeded();
    }

    public override void OnJoinedRoom()
    {
        InitializeRoomIfNeeded();
    }

    void InitializeRoomIfNeeded()
    {
        if (roomInitialized) return;
        if (!PhotonNetwork.InRoom) return;

        roomInitialized = true;

        if (matchEndUI != null) matchEndUI.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            AssignTeamsForAllPlayers();
            timer = matchDuration;
            StartMatch();
        }

        TrySpawnLocalPlayerIfReady();
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) return;

        if (!localPlayerSpawned && Time.time >= nextSpawnCheckTime)
        {
            nextSpawnCheckTime = Time.time + 1f;
            TrySpawnLocalPlayerIfReady();
        }

        if (PhotonNetwork.IsMasterClient && matchActive)
        {
            timer -= Time.deltaTime;
            if (timer < 0f) timer = 0f;

            Hashtable props = new Hashtable { { TIMER_KEY, timer } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);

            if (timer <= 0f)
            {
                EndMatch(PayloadTeam.Defenders, "Time's Up");
            }
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MATCH_ACTIVE_KEY))
            {
                matchActive = (bool)PhotonNetwork.CurrentRoom.CustomProperties[MATCH_ACTIVE_KEY];
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(TIMER_KEY))
            {
                timer = (float)PhotonNetwork.CurrentRoom.CustomProperties[TIMER_KEY];
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ATTACKERS_SCORE_KEY))
            {
                attackersScore = (int)PhotonNetwork.CurrentRoom.CustomProperties[ATTACKERS_SCORE_KEY];
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(DEFENDERS_SCORE_KEY))
            {
                defendersScore = (int)PhotonNetwork.CurrentRoom.CustomProperties[DEFENDERS_SCORE_KEY];
            }
        }

        UpdateTimerUI();
        UpdateStateUI();
    }

    void StartMatch()
    {
        matchActive = true;
        Hashtable props = new Hashtable
        {
            { MATCH_ACTIVE_KEY, true },
            { TIMER_KEY, timer },
            { ATTACKERS_SCORE_KEY, attackersScore },
            { DEFENDERS_SCORE_KEY, defendersScore }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void UpdateStateUI()
    {
        if (stateText == null || payloadController == null) return;
        stateText.text = payloadController.CurrentState.ToString();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        AssignTeamForPlayer(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        AssignTeamsForAllPlayers();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer != PhotonNetwork.LocalPlayer) return;
        if (changedProps.ContainsKey(PayloadTeamConstants.TEAM_KEY))
        {
            TrySpawnLocalPlayerIfReady();
        }
    }

    void AssignTeamsForAllPlayers()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int attackers = 0;
        int defenders = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (PayloadTeamUtils.TryGetPlayerTeam(player, out PayloadTeam team))
            {
                if (team == PayloadTeam.Attackers) attackers++; else defenders++;
            }
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (PayloadTeamUtils.TryGetPlayerTeam(player, out _)) continue;
            PayloadTeam assignTeam = attackers <= defenders ? PayloadTeam.Attackers : PayloadTeam.Defenders;
            PayloadTeamUtils.SetPlayerTeam(player, assignTeam);
            if (assignTeam == PayloadTeam.Attackers) attackers++; else defenders++;
        }
    }

    void AssignTeamForPlayer(Player player)
    {
        int attackers = GetTeamCount(PayloadTeam.Attackers);
        int defenders = GetTeamCount(PayloadTeam.Defenders);
        PayloadTeam assignTeam = attackers <= defenders ? PayloadTeam.Attackers : PayloadTeam.Defenders;
        PayloadTeamUtils.SetPlayerTeam(player, assignTeam);
    }

    int GetTeamCount(PayloadTeam team)
    {
        int count = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (PayloadTeamUtils.TryGetPlayerTeam(player, out PayloadTeam playerTeam) && playerTeam == team)
                count++;
        }
        return count;
    }

    void TrySpawnLocalPlayerIfReady()
    {
        if (!autoSpawnPlayer || localPlayerSpawned) return;
        if (!PhotonNetwork.InRoom) return;

        if (!loggedMissingPrefab)
        {
            if (Resources.Load<GameObject>(playerPrefabName) == null)
            {
                Debug.LogError($"Player prefab not found in Resources: {playerPrefabName}");
                loggedMissingPrefab = true;
                return;
            }
        }

        if (forceLocalTeam)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PayloadTeamUtils.SetPlayerTeam(PhotonNetwork.LocalPlayer, forcedLocalTeam);
            }
            else
            {
                photonView.RPC("RPC_RequestTeam", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, (int)forcedLocalTeam);
                return;
            }
        }

        if (!PayloadTeamUtils.TryGetPlayerTeam(PhotonNetwork.LocalPlayer, out _))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                AssignTeamForPlayer(PhotonNetwork.LocalPlayer);
            }
            else
            {
                photonView.RPC("RPC_RequestTeam", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, -1);
            }
            return;
        }

        if (HasLocalPlayerInstance())
        {
            localPlayerSpawned = true;
            return;
        }

        Transform spawnPoint = GetSpawnPointForLocalTeam();
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        PhotonNetwork.Instantiate(playerPrefabName, spawnPos, spawnRot);
        localPlayerSpawned = true;
    }

    [PunRPC]
    void RPC_RequestTeam(int actorNumber, int teamValue)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        Player player = PhotonNetwork.CurrentRoom?.GetPlayer(actorNumber);
        if (player == null) return;
        if (teamValue < 0)
        {
            AssignTeamForPlayer(player);
        }
        else
        {
            PayloadTeamUtils.SetPlayerTeam(player, (PayloadTeam)teamValue);
        }
    }

    bool HasLocalPlayerInstance()
    {
        PhotonView[] views = FindObjectsByType<PhotonView>(FindObjectsSortMode.None);
        foreach (PhotonView view in views)
        {
            if (view.IsMine && view.Owner == PhotonNetwork.LocalPlayer)
            {
                if (view.GetComponent<PlayerHealth>() != null ||
                    view.GetComponent<FPSController>() != null ||
                    view.GetComponent<PlayerControllerNetwork>() != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public Transform GetSpawnPointForLocalTeam()
    {
        PayloadTeam team;
        if (forceLocalTeam)
        {
            team = forcedLocalTeam;
        }
        else
        {
            if (!PayloadTeamUtils.TryGetPlayerTeam(PhotonNetwork.LocalPlayer, out team))
                return null;
        }

        Transform[] points = team == PayloadTeam.Attackers ? attackerSpawnPoints : defenderSpawnPoints;
        if (points == null || points.Length == 0) return null;
        return points[Random.Range(0, points.Length)];
    }

    public void NotifyPayloadReachedDestination()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        EndMatch(PayloadTeam.Attackers, "Payload reached destination");
    }

    public void AddTeamScore(PayloadTeam team, int amount)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (amount == 0) return;

        if (team == PayloadTeam.Attackers)
            attackersScore += amount;
        else
            defendersScore += amount;

        Hashtable props = new Hashtable
        {
            { ATTACKERS_SCORE_KEY, attackersScore },
            { DEFENDERS_SCORE_KEY, defendersScore }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    void EndMatch(PayloadTeam winnerTeam, string reason)
    {
        if (!matchActive) return;
        matchActive = false;

        Hashtable props = new Hashtable
        {
            { MATCH_ACTIVE_KEY, false },
            { WINNER_TEAM_KEY, (int)winnerTeam }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        photonView.RPC("RPC_MatchEnded", RpcTarget.All, (int)winnerTeam, reason);
    }

    [PunRPC]
    void RPC_MatchEnded(int winnerTeamValue, string reason)
    {
        matchActive = false;

        if (matchEndUI != null) matchEndUI.SetActive(true);
        if (winnerText != null)
        {
            PayloadTeam winnerTeam = (PayloadTeam)winnerTeamValue;
            winnerText.text = $"Winner: {winnerTeam}\n{reason}";
        }
    }
}
