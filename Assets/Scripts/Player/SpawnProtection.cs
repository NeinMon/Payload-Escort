using UnityEngine;
using Photon.Pun;

/// <summary>
/// Spawn protection - player bất tử trong vài giây sau khi spawn
/// </summary>
public class SpawnProtection : MonoBehaviourPun
{
    [Header("Settings")]
    public float protectionDuration = 3f;
    public Material protectionMaterial; // Material trong suốt/glowing
    
    private float protectionTimer = 0f;
    private bool isProtected = false;
    private PlayerHealth playerHealth;
    private Renderer[] renderers;
    private Material[] originalMaterials;
    
    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        renderers = GetComponentsInChildren<Renderer>();
        
        if (photonView.IsMine)
        {
            EnableProtection();
        }
    }
    
    void Update()
    {
        if (isProtected)
        {
            protectionTimer -= Time.deltaTime;
            if (protectionTimer <= 0f)
            {
                DisableProtection();
            }
        }
    }
    
    public void EnableProtection()
    {
        isProtected = true;
        protectionTimer = protectionDuration;
        
        // Save original materials
        originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                originalMaterials[i] = renderers[i].material;
        }
        
        // Apply protection visual
        if (protectionMaterial != null)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null)
                    r.material = protectionMaterial;
            }
        }
        
        Debug.Log("Spawn protection enabled for " + protectionDuration + " seconds");
    }
    
    void DisableProtection()
    {
        isProtected = false;
        
        // Restore original materials
        if (originalMaterials != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && originalMaterials[i] != null)
                    renderers[i].material = originalMaterials[i];
            }
        }
        
        Debug.Log("Spawn protection disabled");
    }
    
    public bool IsProtected()
    {
        return isProtected;
    }
}
