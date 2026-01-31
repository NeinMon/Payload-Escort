using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(MeshRenderer))]
public class PlayerColorAssigner : MonoBehaviourPun
{
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        AssignColor();
    }

    void AssignColor()
    {
        if (!photonView.IsMine && photonView.Owner == null)
            return;

        Color assignedColor;
        Player owner = photonView.Owner;

        if (PayloadTeamUtils.TryGetPlayerTeam(owner, out PayloadTeam team))
        {
            assignedColor = team == PayloadTeam.Attackers ? Color.red : Color.blue;
        }
        else
        {
            int actorNumber = owner.ActorNumber;

            switch (actorNumber % 5)
            {
                case 0: assignedColor = Color.red; break;
                case 1: assignedColor = Color.blue; break;
                case 2: assignedColor = Color.green; break;
                case 3: assignedColor = Color.yellow; break;
                default: assignedColor = Color.magenta; break;
            }
        }

        meshRenderer.material.color = assignedColor;
    }
}