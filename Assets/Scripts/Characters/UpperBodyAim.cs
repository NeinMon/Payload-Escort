using UnityEngine;
using Photon.Pun;

public class UpperBodyAim : MonoBehaviourPun
{
    [Header("Bones")]
    public Transform spine;
    public Transform chest;
    public Transform head;

    [Header("Target")]
    public Camera targetCamera;
    public FPSController fpsController;
    public PlayerControllerNetwork playerController;
    public PlayerControllerNetwork playerControllerNetwork;

    [Header("Limits")]
    public float maxPitchUp = 45f;
    public float maxPitchDown = 30f;

    [Header("Weights")]
    public float spineWeight = 0.4f;
    public float chestWeight = 0.4f;
    public float headWeight = 0.6f;

    [Header("Smoothing")]
    public float rotationSpeed = 8f;
    public bool syncNetwork = true;
    public float networkThreshold = 0.5f;

    private Quaternion spineBaseRot;
    private Quaternion chestBaseRot;
    private Quaternion headBaseRot;
    private float currentPitch;
    private float lastSentPitch;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = GetComponentInChildren<Camera>();

        if (fpsController == null)
            fpsController = GetComponent<FPSController>();

        if (playerController == null)
            playerController = GetComponent<PlayerControllerNetwork>();

        if (playerControllerNetwork == null)
            playerControllerNetwork = GetComponent<PlayerControllerNetwork>();

        if (spine != null) spineBaseRot = spine.localRotation;
        if (chest != null) chestBaseRot = chest.localRotation;
        if (head != null) headBaseRot = head.localRotation;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        float targetPitch = currentPitch;
        if (!syncNetwork || photonView.IsMine)
        {
            targetPitch = GetPitchSource();
            if (syncNetwork && photonView.IsMine && Mathf.Abs(targetPitch - lastSentPitch) > networkThreshold)
            {
                lastSentPitch = targetPitch;
                photonView.RPC("RPC_SetPitch", RpcTarget.Others, targetPitch);
            }
        }

        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * rotationSpeed);
        ApplyPitch(currentPitch);
    }

    float GetPitchSource()
    {
        if (fpsController != null)
        {
            float pitchValue = fpsController.GetPitch();
            return Mathf.Clamp(pitchValue, -maxPitchUp, maxPitchDown);
        }

        if (playerController != null)
        {
            float pitchValue = playerController.GetPitch();
            return Mathf.Clamp(pitchValue, -maxPitchUp, maxPitchDown);
        }

        if (playerControllerNetwork != null)
        {
            float pitchValue = playerControllerNetwork.GetPitch();
            return Mathf.Clamp(pitchValue, -maxPitchUp, maxPitchDown);
        }

        if (targetCamera == null) return 0f;

        float pitch = targetCamera.transform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        return Mathf.Clamp(pitch, -maxPitchUp, maxPitchDown);
    }

    void ApplyPitch(float pitch)
    {
        float spinePitch = pitch * spineWeight;
        float chestPitch = pitch * chestWeight;
        float headPitch = pitch * headWeight;

        if (spine != null)
            spine.localRotation = spineBaseRot * Quaternion.Euler(spinePitch, 0f, 0f);
        if (chest != null)
            chest.localRotation = chestBaseRot * Quaternion.Euler(chestPitch, 0f, 0f);
        if (head != null)
            head.localRotation = headBaseRot * Quaternion.Euler(headPitch, 0f, 0f);
    }

    [PunRPC]
    void RPC_SetPitch(float pitch)
    {
        currentPitch = Mathf.Clamp(pitch, -maxPitchUp, maxPitchDown);
    }
}
