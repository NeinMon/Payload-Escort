using UnityEngine;
using Photon.Pun;

/// <summary>
/// Weapon Manager - Quản lý chuyển đổi giữa nhiều loại súng
/// </summary>
public class WeaponManager : MonoBehaviourPun
{
    [Header("Weapons")]
    public WeaponBase[] weapons; // Danh sách các vũ khí

    [Header("Firing")]
    public bool disableFireWhileSprinting = true;
    public float autoFireInterval = 0.22f;
    
    private int currentWeaponIndex = 0;
    private WeaponBase currentWeapon;
    private float nextAutoFireTime = 0f;
    
    void Start()
    {
        if (!photonView.IsMine) return;

        // Ẩn tất cả vũ khí trừ vũ khí đầu tiên
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].gameObject.SetActive(i == 0);
            }
        }

        if (weapons.Length > 0)
        {
            currentWeapon = weapons[0];
            currentWeaponIndex = 0;
        }
    }
    
    void Update()
    {
        if (!photonView.IsMine) return;
        
        // Chuyển vũ khí bằng số (1, 2, 3...)
        for (int i = 0; i < weapons.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchWeapon(i);
            }
        }
        
        // Chuyển vũ khí bằng scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SwitchWeapon((currentWeaponIndex + 1) % weapons.Length);
        }
        else if (scroll < 0f)
        {
            int newIndex = currentWeaponIndex - 1;
            if (newIndex < 0) newIndex = weapons.Length - 1;
            SwitchWeapon(newIndex);
        }
        
        // Bắn
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        bool canFire = currentWeapon != null && !currentWeapon.IsReloading && currentWeapon.currentAmmo > 0;
        if (canFire && (!disableFireWhileSprinting || !isSprinting))
        {
            if (Input.GetButtonDown("Fire1"))
            {
                currentWeapon.Fire();
                nextAutoFireTime = Time.time + Mathf.Max(autoFireInterval, currentWeapon.fireRate);
            }
            else if (Input.GetButton("Fire1"))
            {
                if (Time.time >= nextAutoFireTime)
                {
                    currentWeapon.Fire();
                    nextAutoFireTime = Time.time + Mathf.Max(autoFireInterval, currentWeapon.fireRate);
                }
            }
        }

        // Reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentWeapon != null)
                currentWeapon.Reload();
        }

        if (currentWeapon != null && !currentWeapon.IsReloading && currentWeapon.currentAmmo <= 0)
        {
            currentWeapon.Reload();
        }
    }
    
    void SwitchWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;
        if (index == currentWeaponIndex) return;
        
        // Ẩn vũ khí hiện tại
        if (currentWeapon != null)
            currentWeapon.gameObject.SetActive(false);
        
        // Hiện vũ khí mới
        currentWeaponIndex = index;
        currentWeapon = weapons[index];
        
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);
            Debug.Log("Switched to " + currentWeapon.weaponName);
        }
        
        // Update HUD
        PlayerHUD hud = FindAnyObjectByType<PlayerHUD>();
        if (hud != null)
        {
            hud.SetWeapon(currentWeapon);
        }
    }
    
    public WeaponBase GetCurrentWeapon()
    {
        return currentWeapon;
    }
}
