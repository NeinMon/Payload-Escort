using UnityEngine;
using Photon.Pun;

public abstract class WeaponBase : MonoBehaviourPun
{
    [Header("Weapon Stats")]
    public string weaponName = "Weapon";
    public float damage = 25f;
    public float fireRate = 0.15f; // Time between shots
    public int maxAmmo = 30;
    public int currentAmmo;
    public float reloadTime = 2f;
    
    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;
    public bool autoFindMuzzleFlash = true;
    
    [Header("References")]
    public Transform firePoint;
    
    protected float nextFireTime = 0f;
    protected bool isReloading = false;
    protected AudioSource audioSource;
    
    protected virtual void Start()
    {
        currentAmmo = maxAmmo;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (autoFindMuzzleFlash && muzzleFlash == null)
        {
            ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].name.ToLower().Contains("muzzle"))
                {
                    muzzleFlash = particles[i];
                    break;
                }
            }

            if (muzzleFlash == null && particles.Length > 0)
                muzzleFlash = particles[0];
        }
    }
    
    public virtual bool CanFire()
    {
        return !isReloading && Time.time >= nextFireTime && currentAmmo > 0;
    }
    
    public virtual void Fire()
    {
        if (!CanFire())
        {
            if (currentAmmo <= 0)
            {
                PlayEmptySound();
            }
            return;
        }
        
        currentAmmo--;
        nextFireTime = Time.time + fireRate;
        
        // Visual effects
        if (muzzleFlash != null)
        {
            if (!muzzleFlash.gameObject.activeInHierarchy)
                muzzleFlash.gameObject.SetActive(true);

            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play(true);
        }
        
        // Audio
        if (fireSound != null && audioSource != null)
            audioSource.PlayOneShot(fireSound);
        
        // Override in child class for actual shooting logic
        OnFire();
    }
    
    protected abstract void OnFire();
    
    public virtual void Reload()
    {
        if (isReloading || currentAmmo == maxAmmo) return;
        
        StartCoroutine(ReloadCoroutine());
    }

    public bool IsReloading => isReloading;
    
    protected System.Collections.IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        
        if (reloadSound != null && audioSource != null)
            audioSource.PlayOneShot(reloadSound);
        
        yield return new WaitForSeconds(reloadTime);
        
        currentAmmo = maxAmmo;
        isReloading = false;
    }
    
    public void PlayEmptySound()
    {
        if (emptySound != null && audioSource != null)
            audioSource.PlayOneShot(emptySound);
    }
}
