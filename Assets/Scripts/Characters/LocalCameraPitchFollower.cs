using UnityEngine;
using Photon.Pun;

public class LocalCameraPitchFollower : MonoBehaviourPun
{
    [Header("Target")]
    public Transform target; // e.g., Arm root or spine/chest
    public Camera targetCamera;

    [Header("Limits")]
    public float maxPitchUp = 80f;
    public float maxPitchDown = 80f;

    [Header("Tuning")]
    [Range(0f, 1f)]
    public float weight = 1f;

    private Quaternion baseLocalRotation;

    void Start()
    {
        if (target == null)
            target = transform;

        if (targetCamera == null)
            targetCamera = GetComponentInChildren<Camera>();

        baseLocalRotation = target.localRotation;
    }

    void LateUpdate()
    {
        if (!photonView.IsMine) return;
        if (target == null || targetCamera == null) return;

        float pitch = targetCamera.transform.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        pitch = Mathf.Clamp(pitch, -maxPitchUp, maxPitchDown);
        float appliedPitch = pitch * weight;

        target.localRotation = baseLocalRotation * Quaternion.Euler(-appliedPitch, 0f, 0f);
    }
}
