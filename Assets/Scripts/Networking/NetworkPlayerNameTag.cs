using UnityEngine;
using TMPro;
using Photon.Pun;

/// <summary>
/// Hiển thị tên player trên đầu (3D WorldSpace Canvas)
/// Visible cho tất cả players
/// </summary>
public class NetworkPlayerNameTag : MonoBehaviourPun
{
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2.5f, 0);
    public float maxDistance = 50f;
    
    [Header("UI")]
    public TextMeshProUGUI nameText;
    public Canvas canvas;
    
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
        
        // Set player name
        if (nameText != null && photonView != null && photonView.Owner != null)
        {
            nameText.text = photonView.Owner.NickName;
            
            // Color based on if local player
            if (photonView.IsMine)
            {
                nameText.color = Color.green;
                // Optionally hide own name tag
                // canvas.enabled = false;
            }
            else
            {
                nameText.color = Color.white;
            }
        }
        
        // Position
        transform.localPosition = offset;
    }
    
    void LateUpdate()
    {
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
            canvas.enabled = distance <= maxDistance;
        }
    }
}
