using UnityEngine;
using Photon.Pun;

public class LocalOnlyRendererDisabler : MonoBehaviourPun
{
    [Header("Targets")]
    public Transform root;
    public bool disableAllRenderersUnderRoot = true;
    public Renderer[] specificRenderers;
    public bool includeInactive = true;
    public bool ignoreWeaponLayer = true;

    void Start()
    {
        if (!photonView.IsMine) return;

        if (disableAllRenderersUnderRoot)
        {
            Transform targetRoot = root != null ? root : null;
            Renderer[] renderers = targetRoot != null
                ? targetRoot.GetComponentsInChildren<Renderer>(includeInactive)
                : null;

            if (renderers == null || renderers.Length == 0)
            {
                Animator animator = GetComponentInChildren<Animator>();
                if (animator != null)
                    targetRoot = animator.transform;

                if (targetRoot == null)
                    targetRoot = photonView != null ? photonView.transform : transform;

                renderers = targetRoot.GetComponentsInChildren<Renderer>(includeInactive);
            }

            int weaponLayer = ignoreWeaponLayer ? LayerMask.NameToLayer("Weapon") : -1;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (ignoreWeaponLayer && weaponLayer >= 0 && renderers[i].gameObject.layer == weaponLayer)
                    continue;

                renderers[i].enabled = false;
            }
            return;
        }

        if (specificRenderers == null) return;
        int weaponLayerSpecific = ignoreWeaponLayer ? LayerMask.NameToLayer("Weapon") : -1;
        for (int i = 0; i < specificRenderers.Length; i++)
        {
            if (specificRenderers[i] == null) continue;
            if (ignoreWeaponLayer && weaponLayerSpecific >= 0 && specificRenderers[i].gameObject.layer == weaponLayerSpecific)
                continue;

            specificRenderers[i].enabled = false;
        }
    }
}
