using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TurretOwner : MonoBehaviour
{
    [SerializeField] private int ownerActorNumber = -1;
    [SerializeField] private PayloadTeam ownerTeam = PayloadTeam.Attackers;
    [SerializeField] private bool hasOwnerTeam;

    public int OwnerActorNumber => ownerActorNumber;
    public PayloadTeam OwnerTeam => ownerTeam;
    public bool HasOwnerTeam => hasOwnerTeam;

    void Awake()
    {
        if (ownerActorNumber > 0) return;

        PhotonView view = GetComponent<PhotonView>();
        if (view != null && view.OwnerActorNr > 0)
            SetOwner(view.OwnerActorNr);
    }

    public void SetOwner(int actorNumber)
    {
        ownerActorNumber = actorNumber;
        if (PhotonNetwork.InRoom)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            if (player != null && PayloadTeamUtils.TryGetPlayerTeam(player, out PayloadTeam team))
            {
                ownerTeam = team;
                hasOwnerTeam = true;
                return;
            }
        }

        hasOwnerTeam = false;
    }
}
