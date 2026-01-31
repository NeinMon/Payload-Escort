using UnityEngine;
using Photon.Pun;

public class WeaponCameraAligner : MonoBehaviourPun
{
    public Camera playerCamera;
    public WeaponManager weaponManager;
    public string cameraAnchorName = "CameraAnchor";
    public Transform anchorOverride;
    public bool useFirePointIfNoAnchor = true;
    public bool searchPlayerHierarchy = true;
    public Vector3 positionOffset;
    public bool alignRotation = false;
    public Vector3 rotationOffset;

    private Transform cachedAnchor;
    private WeaponBase cachedWeapon;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (weaponManager == null)
            weaponManager = GetComponentInChildren<WeaponManager>();
    }

    void LateUpdate()
    {
        if (!photonView.IsMine) return;
        if (playerCamera == null || weaponManager == null) return;

        WeaponBase weapon = weaponManager.GetCurrentWeapon();
        Transform anchor = GetAnchor(weapon);
        if (anchor == null) return;

        playerCamera.transform.position = anchor.position + anchor.TransformVector(positionOffset);

        if (alignRotation)
            playerCamera.transform.rotation = anchor.rotation * Quaternion.Euler(rotationOffset);
    }

    Transform GetAnchor(WeaponBase weapon)
    {
        if (anchorOverride != null)
            return anchorOverride;

        if (weapon != cachedWeapon)
        {
            cachedWeapon = weapon;
            cachedAnchor = null;
        }

        if (cachedAnchor != null && cachedAnchor.gameObject.activeInHierarchy)
            return cachedAnchor;

        cachedAnchor = ResolveAnchor(weapon);
        return cachedAnchor;
    }

    Transform ResolveAnchor(WeaponBase weapon)
    {
        if (!string.IsNullOrEmpty(cameraAnchorName))
        {
            if (weapon != null)
            {
                Transform weaponAnchor = weapon.transform.Find(cameraAnchorName);
                if (weaponAnchor != null)
                    return weaponAnchor;
            }

            if (searchPlayerHierarchy)
            {
                Transform ownerAnchor = FindInHierarchy(transform, cameraAnchorName);
                if (ownerAnchor != null)
                    return ownerAnchor;
            }
        }

        if (useFirePointIfNoAnchor && weapon != null && string.IsNullOrEmpty(cameraAnchorName))
            return weapon.firePoint;

        return null;
    }

    Transform FindInHierarchy(Transform parent, string targetName)
    {
        if (parent == null || string.IsNullOrEmpty(targetName))
            return null;

        if (parent.name == targetName)
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            Transform result = FindInHierarchy(child, targetName);
            if (result != null)
                return result;
        }

        return null;
    }
}
