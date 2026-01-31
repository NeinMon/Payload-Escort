using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using ExitGames.Client.Photon;

public class MultiplayerFPSGameManager : MonoBehaviourPunCallbacks
{
    public static MultiplayerFPSGameManager Instance;

    [Header("Match Settings")]
    public int minPlayers = 2;
    public int maxPlayers = 8;
    public float matchDuration = 300f; // seconds
    public int killsToWin = 20; // Alternative win condition

    [Header("References")]
    public Transform[] spawnPoints;
    public GameObject playerPrefab;
    
    [Header("UI")]
    public GameObject matchStartUI;
    public GameObject matchEndUI;
    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI winnerText;

    private float timer;
    private bool matchActive = false;
    
    private const string TIMER_KEY = "MatchTimer";
    private const string MATCH_ACTIVE_KEY = "MatchActive";

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            // Initialize GameStats
            if (GameStats.Instance != null)
            {
                GameStats.Instance.InitializePlayerStats(PhotonNetwork.LocalPlayer);
            }
            
            if (PhotonNetwork.IsMasterClient)
            {
                timer = matchDuration;
                StartMatch();
            }
            
            SpawnPlayer();
        }
        
        if (matchEndUI != null) matchEndUI.SetActive(false);
    }

    void Update()
    {
        if (!matchActive) return;

        if (PhotonNetwork.IsMasterClient)
        {
            timer -= Time.deltaTime;
            
            // Sync timer to all clients
            Hashtable props = new Hashtable { { TIMER_KEY, timer } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            
            if (timer <= 0)
            {
                EndMatch("Time's Up!");
            }
            
            // Check kill limit
            if (GameStats.Instance != null)
            {
                Player leader = GameStats.Instance.GetWinner();
                if (leader != null && GameStats.Instance.GetKills(leader) >= killsToWin)
                {
                    EndMatch($"{leader.NickName} reached {killsToWin} kills!");
                }
            }
        }
        else
        {
            // Clients get timer from room properties
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(TIMER_KEY))
            {
                timer = (float)PhotonNetwork.CurrentRoom.CustomProperties[TIMER_KEY];
            }
        }
        
        UpdateTimerUI();
    }
    
    void StartMatch()
    {
        matchActive = true;
        
        Hashtable props = new Hashtable 
        { 
            { MATCH_ACTIVE_KEY, true },
            { TIMER_KEY, timer }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        
        photonView.RPC("RPC_MatchStarted", RpcTarget.All);
    }
    
    [PunRPC]
    void RPC_MatchStarted()
    {
        matchActive = true;
        if (matchStartUI != null)
        {
            matchStartUI.SetActive(true);
            Invoke(nameof(HideStartUI), 3f);
        }
    }
    
    void HideStartUI()
    {
        if (matchStartUI != null)
            matchStartUI.SetActive(false);
    }
    
    void UpdateTimerUI()
    {
        if (timerText == null) return;
        
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null || spawnPoints.Length == 0) return;
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        PhotonNetwork.Instantiate(playerPrefab.name, spawn.position, spawn.rotation);
    }

    void EndMatch(string reason)
    {
        if (!matchActive) return;
        
        matchActive = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        photonView.RPC("RPC_MatchEnded", RpcTarget.All, reason);
    }
    
    [PunRPC]
    void RPC_MatchEnded(string reason)
    {
        matchActive = false;
        
        if (matchEndUI != null)
        {
            matchEndUI.SetActive(true);
            
            // Display winner
            if (GameStats.Instance != null && winnerText != null)
            {
                Player winner = GameStats.Instance.GetWinner();
                if (winner != null)
                {
                    int kills = GameStats.Instance.GetKills(winner);
                    winnerText.text = $"Winner: {winner.NickName}\n{kills} Kills\n\n{reason}";
                }
            }
        }
        
        // Return to lobby after delay
        Invoke(nameof(ReturnToLobby), 10f);
    }
    
    void ReturnToLobby()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("LobbyScene"); // Change to your lobby scene name
        }
    }
    
    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints.Length == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
    
    public bool IsMatchActive()
    {
        return matchActive;
    }
}