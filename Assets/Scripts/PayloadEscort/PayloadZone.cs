using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class PayloadZone : MonoBehaviour
{
    public PayloadController payloadController;
    public KeyCode repairKey = KeyCode.E;
    public KeyCode sabotageKey = KeyCode.F;

    private bool localInside;
    private PhotonView localView;
    private bool lastRepairing;
    private bool lastSabotaging;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (payloadController == null) return;
        PhotonView view = other.GetComponentInParent<PhotonView>();
        if (view == null || !view.IsMine) return;
        localView = view;
        localInside = true;
        payloadController.ReportPlayerZoneStatus(view.OwnerActorNr, true);
    }

    void OnTriggerExit(Collider other)
    {
        if (payloadController == null) return;
        PhotonView view = other.GetComponentInParent<PhotonView>();
        if (view == null || !view.IsMine) return;
        localInside = false;
        if (localView != null)
        {
            payloadController.ReportRepairStatus(localView.OwnerActorNr, false);
            payloadController.ReportSabotageStatus(localView.OwnerActorNr, false);
        }
        lastRepairing = false;
        lastSabotaging = false;
        payloadController.ReportPlayerZoneStatus(view.OwnerActorNr, false);
    }

    void Update()
    {
        if (!localInside || payloadController == null || localView == null) return;

        if (!PayloadTeamUtils.TryGetPlayerTeam(localView.Owner, out PayloadTeam team)) return;

        if (team == PayloadTeam.Attackers)
        {
            bool repairing = Input.GetKey(repairKey);
            if (repairing != lastRepairing)
            {
                payloadController.ReportRepairStatus(localView.OwnerActorNr, repairing);
                lastRepairing = repairing;
            }
        }
        else
        {
            bool sabotaging = Input.GetKey(sabotageKey);
            if (sabotaging != lastSabotaging)
            {
                payloadController.ReportSabotageStatus(localView.OwnerActorNr, sabotaging);
                lastSabotaging = sabotaging;
            }
        }
    }
}
