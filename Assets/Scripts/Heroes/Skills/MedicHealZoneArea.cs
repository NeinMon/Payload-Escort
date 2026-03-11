using UnityEngine;
using Photon.Pun;

public class MedicHealZoneArea : MonoBehaviour
{
    public float radius = 4f;
    public float healPerSecond = 25f;
    public float tickInterval = 0.2f;
    public float duration = 10f;
    public int ownerActorNumber = -1;
    public PayloadTeam ownerTeam = PayloadTeam.Attackers;

    private float elapsed;
    private float tickTimer;
    private bool canHeal;

    public void Initialize(bool isAuthority)
    {
        canHeal = isAuthority;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= duration)
        {
            Destroy(gameObject);
            return;
        }

        if (!canHeal)
            return;

        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval)
            return;

        float amount = healPerSecond * tickTimer;
        tickTimer = 0f;

        Collider[] hits = Physics.OverlapSphere(transform.position, radius, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            PlayerHealth health = hits[i].GetComponentInParent<PlayerHealth>();
            if (health == null || health.IsDead)
                continue;

            if (PhotonNetwork.InRoom)
            {
                PhotonView view = health.GetComponent<PhotonView>();
                if (view == null || view.Owner == null)
                    continue;

                if (!PayloadTeamUtils.TryGetPlayerTeam(view.Owner, out PayloadTeam team))
                    continue;

                if (team != ownerTeam)
                    continue;
            }

            health.Heal(amount);
        }
    }
}
