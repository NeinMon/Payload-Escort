using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PayloadCheckpoint : MonoBehaviour
{
    public PayloadController payloadController;
    public int checkpointIndex = 0;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (payloadController == null) return;
        PayloadController payload = other.GetComponentInParent<PayloadController>();
        if (payload == null || payload != payloadController) return;
        payloadController.NotifyCheckpointReached(checkpointIndex);
    }
}
