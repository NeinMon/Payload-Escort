using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;

public class GameStats : MonoBehaviourPun
{
    // Player stats stored in Photon Custom Properties
    private const string KILLS_KEY = "kills";
    private const string DEATHS_KEY = "deaths";
    private const string ASSISTS_KEY = "assists";
    
    public static GameStats Instance;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        // Initialize stats for local player when joining
        if (PhotonNetwork.LocalPlayer != null)
        {
            InitializePlayerStats(PhotonNetwork.LocalPlayer);
        }
    }
    
    public void InitializePlayerStats(Player player)
    {
        Hashtable stats = new Hashtable
        {
            { KILLS_KEY, 0 },
            { DEATHS_KEY, 0 },
            { ASSISTS_KEY, 0 }
        };
        player.SetCustomProperties(stats);
    }
    
    public void AddKill(Player player)
    {
        if (player == null) return;
        
        int currentKills = GetKills(player);
        Hashtable newStats = new Hashtable { { KILLS_KEY, currentKills + 1 } };
        player.SetCustomProperties(newStats);
        
        Debug.Log($"{player.NickName} now has {currentKills + 1} kills");
    }
    
    public void AddDeath(Player player)
    {
        if (player == null) return;
        
        int currentDeaths = GetDeaths(player);
        Hashtable newStats = new Hashtable { { DEATHS_KEY, currentDeaths + 1 } };
        player.SetCustomProperties(newStats);
        
        Debug.Log($"{player.NickName} now has {currentDeaths + 1} deaths");
    }
    
    public void AddAssist(Player player)
    {
        if (player == null) return;
        
        int currentAssists = GetAssists(player);
        Hashtable newStats = new Hashtable { { ASSISTS_KEY, currentAssists + 1 } };
        player.SetCustomProperties(newStats);
    }
    
    // Getters
    public int GetKills(Player player)
    {
        if (player == null || player.CustomProperties == null) return 0;
        return player.CustomProperties.ContainsKey(KILLS_KEY) ? (int)player.CustomProperties[KILLS_KEY] : 0;
    }
    
    public int GetDeaths(Player player)
    {
        if (player == null || player.CustomProperties == null) return 0;
        return player.CustomProperties.ContainsKey(DEATHS_KEY) ? (int)player.CustomProperties[DEATHS_KEY] : 0;
    }
    
    public int GetAssists(Player player)
    {
        if (player == null || player.CustomProperties == null) return 0;
        return player.CustomProperties.ContainsKey(ASSISTS_KEY) ? (int)player.CustomProperties[ASSISTS_KEY] : 0;
    }
    
    public float GetKDRatio(Player player)
    {
        int deaths = GetDeaths(player);
        if (deaths == 0) return GetKills(player);
        return (float)GetKills(player) / deaths;
    }
    
    // Get sorted player list by kills
    public List<Player> GetPlayersSortedByKills()
    {
        List<Player> players = new List<Player>(PhotonNetwork.PlayerList);
        players.Sort((a, b) => GetKills(b).CompareTo(GetKills(a)));
        return players;
    }
    
    // Get winner (player with most kills)
    public Player GetWinner()
    {
        Player winner = null;
        int maxKills = -1;
        
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int kills = GetKills(player);
            if (kills > maxKills)
            {
                maxKills = kills;
                winner = player;
            }
        }
        
        return winner;
    }
}
