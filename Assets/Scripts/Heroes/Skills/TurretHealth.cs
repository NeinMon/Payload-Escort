using UnityEngine;
using Photon.Pun;

public class TurretHealth : MonoBehaviourPun
{
    public float maxHealth = 250f;
    private float currentHealth;

    public System.Action Destroyed;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    [PunRPC]
    public void TakeDamageFromPlayer(float damage, int attackerActorNumber)
    {
        ApplyDamage(damage);
    }

    [PunRPC]
    public void TakeDamage(float damage)
    {
        ApplyDamage(damage);
    }

    private void ApplyDamage(float damage)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Destroyed?.Invoke();

            if (PhotonNetwork.InRoom)
            {
                if (photonView.IsMine)
                    PhotonNetwork.Destroy(photonView);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
