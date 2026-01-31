using UnityEngine;
using Photon.Pun;

public class PlayerRoleAssigner : MonoBehaviourPun
{
    public PlayerRole defaultRole = PlayerRole.Engineer;

    void Start()
    {
        if (!photonView.IsMine) return;
        if (!PlayerRoleUtils.TryGetPlayerRole(PhotonNetwork.LocalPlayer, out _))
        {
            PlayerRoleUtils.SetPlayerRole(PhotonNetwork.LocalPlayer, defaultRole);
        }
    }
}
