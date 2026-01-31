using UnityEngine;
using Photon.Pun;

public class SingleAudioListenerEnforcer : MonoBehaviour
{
    void Awake()
    {
        EnsureSingleListener();
    }

    public static void EnsureSingleListener()
    {
        AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (listeners == null || listeners.Length <= 1) return;

        AudioListener preferred = GetLocalPlayerListener(listeners);
        if (preferred == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                preferred = mainCamera.GetComponent<AudioListener>();
        }

        if (preferred == null)
        {
            preferred = listeners[0];
        }

        foreach (AudioListener listener in listeners)
        {
            if (listener == null) continue;
            bool shouldEnable = listener == preferred;
            if (listener.enabled != shouldEnable)
                listener.enabled = shouldEnable;
        }
    }

    private static AudioListener GetLocalPlayerListener(AudioListener[] listeners)
    {
        foreach (AudioListener listener in listeners)
        {
            if (listener == null) continue;
            PhotonView view = listener.GetComponentInParent<PhotonView>();
            if (view != null && view.IsMine)
                return listener;
        }
        return null;
    }
}
