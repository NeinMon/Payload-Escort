using UnityEngine;
using Photon.Pun;

public class SciFiCharacterLoader : MonoBehaviourPun
{
    [Header("Character Prefab")]
    public GameObject characterPrefab;
    public RuntimeAnimatorController animatorController;

    [Header("Transform")]
    public Vector3 localPosition = Vector3.zero;
    public Vector3 localRotation = Vector3.zero;
    public Vector3 localScale = Vector3.one;

    [Header("Visibility")]
    public bool hideModelForLocal = true;
    public bool disableLegacyRenderer = true;
    public bool excludeWeaponRenderers = true;
    public string[] excludeRendererNameContains = new string[] { "Gun", "Weapon" };

    [Header("Animator")]
    public bool addAnimatorDriver = true;

    private GameObject instance;

    void Start()
    {
        if (characterPrefab == null) return;

        if (disableLegacyRenderer)
        {
            MeshRenderer legacyRenderer = GetComponent<MeshRenderer>();
            if (legacyRenderer != null) legacyRenderer.enabled = false;

            MeshFilter legacyFilter = GetComponent<MeshFilter>();
            if (legacyFilter != null) legacyFilter.sharedMesh = null;

            PlayerColorAssigner colorAssigner = GetComponent<PlayerColorAssigner>();
            if (colorAssigner != null) colorAssigner.enabled = false;
        }

        instance = Instantiate(characterPrefab, transform);
        instance.name = "CharacterModel";
        instance.transform.localPosition = localPosition;
        instance.transform.localEulerAngles = localRotation;
        instance.transform.localScale = localScale;

        Animator animator = instance.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            if (animatorController != null)
                animator.runtimeAnimatorController = animatorController;
            animator.applyRootMotion = false;
        }

        if (hideModelForLocal && photonView.IsMine)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (ShouldExcludeRenderer(renderer))
                    continue;

                renderer.enabled = false;
            }
        }

        if (addAnimatorDriver)
        {
            SciFiWarriorAnimator driver = GetComponent<SciFiWarriorAnimator>();
            if (driver == null)
                driver = gameObject.AddComponent<SciFiWarriorAnimator>();

            driver.animator = animator;
        }
    }

    bool ShouldExcludeRenderer(Renderer renderer)
    {
        if (renderer == null) return false;
        if (excludeWeaponRenderers)
        {
            if (renderer.GetComponentInParent<WeaponBase>() != null)
                return true;
        }

        if (excludeRendererNameContains != null)
        {
            string nameLower = renderer.name.ToLowerInvariant();
            foreach (string keyword in excludeRendererNameContains)
            {
                if (!string.IsNullOrEmpty(keyword) && nameLower.Contains(keyword.ToLowerInvariant()))
                    return true;
            }
        }

        return false;
    }
}
