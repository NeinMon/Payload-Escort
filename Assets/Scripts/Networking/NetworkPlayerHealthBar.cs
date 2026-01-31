using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

/// <summary>
/// Health bar 3D trên đầu player (visible cho tất cả)
/// </summary>
public class NetworkPlayerHealthBar : MonoBehaviourPun
{
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2.2f, 0);
    public float maxDistance = 30f;
    
    [Header("UI")]
    public Image healthBarFill;
    public Canvas canvas;
    
    private PlayerHealth playerHealth;
    private Camera mainCamera;
    private Transform cameraTransform;
    
    void Start()
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();
        
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }
        
        playerHealth = GetComponentInParent<PlayerHealth>();
        
        // Hide for local player (they see their own UI)
        if (photonView.IsMine && canvas != null)
        {
            canvas.enabled = false;
        }
        
        transform.localPosition = offset;
    }
    
    void Update()
    {
        if (photonView.IsMine) return; // Don't update for local player
        
        UpdateHealthBar();
    }
    
    void LateUpdate()
    {
        if (photonView.IsMine) return;
        
        // Find camera if not found
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
                cameraTransform = mainCamera.transform;
        }
        
        if (cameraTransform == null) return;
        
        // Always face camera
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
        
        // Hide if too far
        if (canvas != null)
        {
            float distance = Vector3.Distance(transform.position, cameraTransform.position);
            canvas.enabled = distance <= maxDistance && playerHealth != null && playerHealth.CurrentHealth > 0;
        }
    }
    
    void UpdateHealthBar()
    {
        if (playerHealth == null || healthBarFill == null) return;
        
        float healthRatio = playerHealth.GetHealthRatio();
        healthBarFill.fillAmount = healthRatio;
        
        // Color based on health
        if (healthRatio > 0.6f)
            healthBarFill.color = Color.green;
        else if (healthRatio > 0.3f)
            healthBarFill.color = Color.yellow;
        else
            healthBarFill.color = Color.red;
    }
}
