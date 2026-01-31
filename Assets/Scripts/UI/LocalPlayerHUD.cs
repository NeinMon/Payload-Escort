using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

/// <summary>
/// Local Player HUD - Mỗi player có UI riêng (Canvas trong Player Prefab)
/// Chỉ hiển thị cho local player
/// </summary>
public class LocalPlayerHUD : MonoBehaviourPun
{
    [Header("Health UI")]
    public Image healthBar;
    public TextMeshProUGUI healthText;
    
    [Header("Ammo UI")]
    public TextMeshProUGUI ammoText;
    
    [Header("Crosshair")]
    public Image crosshairImage;
    public Color normalColor = Color.white;
    public Color hitColor = Color.red;
    
    [Header("Info")]
    public TextMeshProUGUI playerNameText;
    
    private PlayerHealth playerHealth;
    private WeaponBase currentWeapon;
    private Canvas canvas;
    
    void Start()
    {
        canvas = GetComponent<Canvas>();
        
        // Only show HUD for local player
        if (!photonView.IsMine)
        {
            if (canvas != null)
                canvas.enabled = false;
            gameObject.SetActive(false);
            return;
        }
        
        // Setup canvas for local player
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.enabled = true;
        }
        
        // Get references
        playerHealth = GetComponentInParent<PlayerHealth>();
        
        // Find weapon
        WeaponManager weaponManager = GetComponentInParent<WeaponManager>();
        if (weaponManager != null)
        {
            currentWeapon = weaponManager.GetCurrentWeapon();
        }
        else
        {
            currentWeapon = GetComponentInParent<WeaponBase>();
        }
        
        if (playerNameText != null)
            playerNameText.text = PhotonNetwork.LocalPlayer.NickName;
    }
    
    void Update()
    {
        if (!photonView.IsMine) return;
        
        UpdateHealthBar();
        UpdateAmmoDisplay();
    }
    
    void UpdateHealthBar()
    {
        if (playerHealth == null || healthBar == null) return;
        
        float healthRatio = playerHealth.CurrentHealth / playerHealth.MaxHealth;
        healthBar.fillAmount = healthRatio;
        
        // Color gradient: green -> yellow -> red
        if (healthRatio > 0.5f)
            healthBar.color = Color.Lerp(Color.yellow, Color.green, (healthRatio - 0.5f) * 2f);
        else
            healthBar.color = Color.Lerp(Color.red, Color.yellow, healthRatio * 2f);
        
        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(playerHealth.CurrentHealth)} / {playerHealth.MaxHealth}";
    }
    
    void UpdateAmmoDisplay()
    {
        if (currentWeapon == null || ammoText == null) return;
        
        ammoText.text = $"{currentWeapon.currentAmmo} / {currentWeapon.maxAmmo}";
        
        // Change color if low ammo
        if (currentWeapon.currentAmmo <= currentWeapon.maxAmmo * 0.2f)
            ammoText.color = Color.red;
        else
            ammoText.color = Color.white;
    }
    
    public void ShowHitmarker()
    {
        if (crosshairImage == null) return;
        
        StopAllCoroutines();
        StartCoroutine(HitmarkerEffect());
    }
    
    System.Collections.IEnumerator HitmarkerEffect()
    {
        crosshairImage.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        crosshairImage.color = normalColor;
    }
    
    public void SetWeapon(WeaponBase weapon)
    {
        currentWeapon = weapon;
    }
    
    public void UpdateWeaponDisplay()
    {
        WeaponManager weaponManager = GetComponentInParent<WeaponManager>();
        if (weaponManager != null)
        {
            currentWeapon = weaponManager.GetCurrentWeapon();
        }
    }
}
