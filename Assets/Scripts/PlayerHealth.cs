using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class PlayerHealth : MonoBehaviourPunCallbacks
{
    public float maxHealth = 100f;
    private float currentHealth;
    public float respawnDelay = 5f;
    public Transform[] spawnPoints;

    private FPSController controller;
    private Transform playerCamera;
    private int lastAttackerID = -1; // Track who killed this player

    void Start()
    {
        currentHealth = maxHealth;
        controller = GetComponent<FPSController>();
        playerCamera = controller != null ? controller.playerCamera : null;
    }

    [PunRPC]
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= damage;
        if (currentHealth < 0f) currentHealth = 0f;

        Debug.Log($"{gameObject.name} took {damage} damage, remaining health: {currentHealth}");

        // Sync health to all clients
        photonView.RPC("SyncHealth", RpcTarget.All, currentHealth);

        // Trigger death across all clients
        if (currentHealth <= 0f)
        {
            photonView.RPC("Die", RpcTarget.All);
        }
    }
    
    // New method with attacker tracking
    [PunRPC]
    public void TakeDamageFromPlayer(float damage, int attackerActorNumber)
    {
        if (currentHealth <= 0f) return;
        
        lastAttackerID = attackerActorNumber;
        currentHealth -= damage;
        if (currentHealth < 0f) currentHealth = 0f;
        
        Debug.Log($"{gameObject.name} took {damage} damage from Actor {attackerActorNumber}, remaining health: {currentHealth}");
        
        // Sync health to all clients
        photonView.RPC("SyncHealth", RpcTarget.All, currentHealth);
        
        // Trigger death across all clients
        if (currentHealth <= 0f)
        {
            photonView.RPC("Die", RpcTarget.All);
            
            // Update kill/death stats
            if (GameStats.Instance != null)
            {
                // Add kill to attacker
                Player attacker = PhotonNetwork.CurrentRoom.GetPlayer(attackerActorNumber);
                if (attacker != null)
                {
                    GameStats.Instance.AddKill(attacker);
                }
                
                // Add death to victim
                Player victim = photonView.Owner;
                if (victim != null)
                {
                    GameStats.Instance.AddDeath(victim);
                }
            }
        }
    }

    [PunRPC]
    void Die()
    {
        Debug.Log($"{gameObject.name} died. Owner: {photonView.Owner?.NickName}");

        SciFiWarriorAnimator warriorAnimator = GetComponent<SciFiWarriorAnimator>();
        if (warriorAnimator != null)
        {
            warriorAnimator.PlayDie();
        }

        if (controller != null)
            controller.enabled = false;

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        // Each player handles their own respawn
        if (photonView.IsMine)
        {
            StartCoroutine(RespawnCoroutine());
        }
    }

    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        // Get spawn point from GameManager
        Transform spawnPoint = null;
        if (PayloadEscortMatchManager.Instance != null)
        {
            spawnPoint = PayloadEscortMatchManager.Instance.GetSpawnPointForLocalTeam();
        }
        else if (MultiplayerFPSGameManager.Instance != null)
        {
            spawnPoint = MultiplayerFPSGameManager.Instance.GetRandomSpawnPoint();
        }

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        // Destroy old player object
        PhotonNetwork.Destroy(photonView);

        // Spawn new player
        PhotonNetwork.Instantiate("Player", spawnPos, spawnRot);
        Debug.Log($"Respawned player {PhotonNetwork.LocalPlayer.NickName} at {spawnPos}");
    }

    public void Heal(float amount)
    {
        if (currentHealth <= 0f) return;
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        photonView.RPC("SyncHealth", RpcTarget.All, currentHealth);
    }

    [PunRPC]
    void SyncHealth(float newHealth)
    {
        currentHealth = newHealth;
    }

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float GetHealthRatio() => currentHealth / maxHealth;
}
