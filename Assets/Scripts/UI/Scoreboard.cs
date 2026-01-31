using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class Scoreboard : MonoBehaviourPun
{
    [Header("UI References")]
    public GameObject scoreboardPanel;
    public Transform playerListContainer;
    public GameObject playerRowPrefab;
    
    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.Tab;
    public float updateInterval = 1f;
    
    private Dictionary<int, GameObject> playerRows = new Dictionary<int, GameObject>();
    private float nextUpdateTime = 0f;
    
    void Update()
    {
        // Toggle scoreboard visibility
        if (Input.GetKeyDown(toggleKey))
        {
            scoreboardPanel.SetActive(true);
            UpdateScoreboard();
        }
        
        if (Input.GetKeyUp(toggleKey))
        {
            scoreboardPanel.SetActive(false);
        }
        
        // Auto update when visible
        if (scoreboardPanel.activeSelf && Time.time >= nextUpdateTime)
        {
            UpdateScoreboard();
            nextUpdateTime = Time.time + updateInterval;
        }
    }
    
    void UpdateScoreboard()
    {
        if (GameStats.Instance == null) return;
        
        // Get sorted players
        List<Player> sortedPlayers = GameStats.Instance.GetPlayersSortedByKills();
        
        // Remove rows for players who left
        List<int> toRemove = new List<int>();
        foreach (var kvp in playerRows)
        {
            bool stillInRoom = false;
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.ActorNumber == kvp.Key)
                {
                    stillInRoom = true;
                    break;
                }
            }
            
            if (!stillInRoom)
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        
        foreach (int id in toRemove)
            playerRows.Remove(id);
        
        // Update or create rows for each player
        int rank = 1;
        foreach (Player player in sortedPlayers)
        {
            if (!playerRows.ContainsKey(player.ActorNumber))
            {
                // Create new row
                GameObject newRow = Instantiate(playerRowPrefab, playerListContainer);
                playerRows[player.ActorNumber] = newRow;
            }
            
            // Update row data
            GameObject row = playerRows[player.ActorNumber];
            UpdatePlayerRow(row, player, rank);
            
            // Set row order
            row.transform.SetSiblingIndex(rank - 1);
            
            rank++;
        }
    }
    
    void UpdatePlayerRow(GameObject row, Player player, int rank)
    {
        // Assume row has: Rank, PlayerName, Kills, Deaths, KD
        TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();
        
        if (texts.Length >= 5)
        {
            texts[0].text = rank.ToString();
            texts[1].text = player.NickName;
            texts[2].text = GameStats.Instance.GetKills(player).ToString();
            texts[3].text = GameStats.Instance.GetDeaths(player).ToString();
            texts[4].text = GameStats.Instance.GetKDRatio(player).ToString("F2");
            
            // Highlight local player
            if (player.IsLocal)
            {
                texts[1].color = Color.yellow;
            }
            else
            {
                texts[1].color = Color.white;
            }
        }
    }
    
    public void ShowScoreboard()
    {
        scoreboardPanel.SetActive(true);
        UpdateScoreboard();
    }
    
    public void HideScoreboard()
    {
        scoreboardPanel.SetActive(false);
    }
}
