using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
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
    
    [Header("Kill Feed")]
    public TextMeshProUGUI killFeedText;
    public float killFeedDuration = 3f;
    
    [Header("References")]
    public PlayerHealth playerHealth;
    public WeaponBase currentWeapon;
    
    private float killFeedTimer = 0f;
    
    void Update()
    {
        UpdateHealthBar();
        UpdateAmmoDisplay();
        UpdateKillFeed();
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
    
    void UpdateKillFeed()
    {
        if (killFeedTimer > 0f)
        {
            killFeedTimer -= Time.deltaTime;
            if (killFeedTimer <= 0f && killFeedText != null)
            {
                killFeedText.text = "";
            }
        }
    }
    
    public void ShowKillMessage(string killerName, string victimName)
    {
        if (killFeedText == null) return;
        
        killFeedText.text = $"{killerName} eliminated {victimName}";
        killFeedTimer = killFeedDuration;
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
}
