using UnityEngine;
using Photon.Pun;

public class SciFiWarriorAnimator : MonoBehaviourPun
{
    public Animator animator;
    public Animator viewModelAnimator;
    public bool useNetworkSync = true;
    public bool useInputForMovement = true;
    public bool syncViewModelAnimator = true;
    public bool autoFindViewModelAnimator = true;
    public string viewModelRootName = "HPCharacter 1";

    public enum FireMode
    {
        Single,
        Auto
    }

    [Header("Combat")]
    public FireMode fireMode = FireMode.Single;
    public bool playAutoAnimationOnHold = true;

    [Header("Clip State Names")]
    public string idleShoot = "Idle_Shoot_Ar";
    public string idleGunMiddle = "Idle_gunMiddle_AR";
    public string idleDucking = "Idle_Ducking_AR";
    public string walkForward = "WalkFront_Shoot_AR";
    public string walkBack = "WalkBack_Shoot_AR";
    public string walkLeft = "WalkLeft_Shoot_AR";
    public string walkRight = "WalkRight_Shoot_AR";
    public string runGuard = "Run_guard_AR";
    public string runGunMiddle = "Run_gunMiddle_AR";
    public string jump = "Jump";
    public string shootSingle = "Shoot_SingleShot_AR";
    public string shootAuto = "Shoot_Autoshot_AR";
    public string reload = "Reload";
    public string die = "Die";

    [Header("Tuning")]
    public float runThreshold = 3.5f;
    public float walkThreshold = 0.2f;
    public float velocityDeadZone = 0.15f;
    public float crossFadeDuration = 0.08f;
    public float shootDuration = 0.15f;
    public float autoFireInterval = 0.25f;
    public float reloadDuration = 1.0f;
    public float jumpDuration = 0.3f;
    public float inputDeadZone = 0.1f;
    public float airborneVelocityThreshold = 0.1f;
    public float groundedGraceTime = 0.1f;
    public float groundCheckDistance = 0.15f;
    public LayerMask groundMask = ~0;
    public bool forceIdleWhenNoInput = true;
    public bool disableLegacyAnimations = true;
    public bool useCrouchInput = false;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public bool blockShootWhenOutOfAmmo = true;

    private CharacterController controller;
    private WeaponManager weaponManager;
    private string currentState;
    private float actionLockUntil;
    private float lastGroundedTime;
    private float nextAutoFireTime;
    private bool wasReloading;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        weaponManager = GetComponentInChildren<WeaponManager>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        Animator[] allAnimators = GetComponentsInChildren<Animator>(true);
        if (animator == null || !HasState(animator, idleGunMiddle))
        {
            for (int i = 0; i < allAnimators.Length; i++)
            {
                if (HasState(allAnimators[i], idleGunMiddle))
                {
                    animator = allAnimators[i];
                    break;
                }
            }
        }

        if (animator != null)
        {
            idleShoot = ResolveStateName(animator, idleShoot, "Idle_Shoot_Ar", "Idle_Shoot_AR");
            shootAuto = ResolveStateName(animator, shootAuto, "Shoot_Autoshot_AR", "Shoot_AutoShot_AR");
        }

        if (syncViewModelAnimator && autoFindViewModelAnimator && viewModelAnimator == null)
        {
            Transform viewModelRoot = FindInHierarchy(transform, viewModelRootName);
            if (viewModelRoot != null)
                viewModelAnimator = viewModelRoot.GetComponentInChildren<Animator>(true);

            if (viewModelAnimator == null)
            {
                for (int i = 0; i < allAnimators.Length; i++)
                {
                    if (allAnimators[i] != animator && HasState(allAnimators[i], idleGunMiddle))
                    {
                        viewModelAnimator = allAnimators[i];
                        break;
                    }
                }
            }
        }

        if (disableLegacyAnimations)
        {
            DisableLegacyAnimations();
        }
    }

    void Update()
    {
        if (animator == null) return;
        if (useNetworkSync && !photonView.IsMine) return;

        float xInput = Input.GetAxisRaw("Horizontal");
        float zInput = Input.GetAxisRaw("Vertical");
        bool hasMoveInput = Mathf.Abs(xInput) > inputDeadZone || Mathf.Abs(zInput) > inputDeadZone;
        bool hasFireHeld = Input.GetButton("Fire1");
        bool hasFireDown = Input.GetButtonDown("Fire1");
        bool hasReload = Input.GetKeyDown(KeyCode.R);
        bool hasJump = Input.GetButtonDown("Jump");
        bool hasSprint = Input.GetKey(KeyCode.LeftShift);
        bool hasCrouch = useCrouchInput && Input.GetKey(crouchKey);

        WeaponBase currentWeapon = weaponManager != null ? weaponManager.GetCurrentWeapon() : null;
        bool isReloading = currentWeapon != null && currentWeapon.IsReloading;
        bool outOfAmmo = currentWeapon != null && currentWeapon.currentAmmo <= 0;
        float currentReloadDuration = currentWeapon != null ? currentWeapon.reloadTime : reloadDuration;

        if (isReloading)
        {
            if (!wasReloading)
            {
                PlayAction(reload, currentReloadDuration);
                wasReloading = true;
            }
            return;
        }
        if (wasReloading)
        {
            wasReloading = false;
        }

        if (forceIdleWhenNoInput && !hasMoveInput && !hasFireHeld && !hasFireDown && !hasReload && !hasJump && !hasSprint && !hasCrouch)
        {
            if (Time.time >= actionLockUntil)
                PlayState(idleGunMiddle, true);
            return;
        }

        bool isGrounded = IsGrounded();
        if (isGrounded && hasJump)
        {
            TryPlayAction(jump, jumpDuration);
            return;
        }

        if (hasReload && currentWeapon != null && currentWeapon.currentAmmo < currentWeapon.maxAmmo)
        {
            TryPlayAction(reload, currentReloadDuration);
            return;
        }

        if (!hasSprint && hasFireHeld && (fireMode == FireMode.Auto || playAutoAnimationOnHold))
        {
            if ((!blockShootWhenOutOfAmmo || !outOfAmmo) && Time.time >= nextAutoFireTime)
            {
                if (TryPlayAction(shootAuto, shootDuration))
                {
                    nextAutoFireTime = Time.time + Mathf.Max(0.05f, autoFireInterval);
                }
            }
            return;
        }

        if (!hasSprint && hasFireDown && (!blockShootWhenOutOfAmmo || !outOfAmmo))
        {
            TryPlayAction(GetShootClip(), shootDuration);
            return;
        }

        if (Time.time < actionLockUntil) return;

        Vector3 velocity = controller ? new Vector3(controller.velocity.x, 0f, controller.velocity.z) : Vector3.zero;
        if (velocity.magnitude < velocityDeadZone)
            velocity = Vector3.zero;
        float speed = velocity.magnitude;

        if (useInputForMovement)
        {
            Vector3 input = new Vector3(xInput, 0f, zInput);
            float inputMag = input.magnitude;

            if (inputMag <= inputDeadZone)
            {
                if (hasCrouch && !string.IsNullOrEmpty(idleDucking))
                    PlayState(idleDucking);
                else if (hasFireHeld && !string.IsNullOrEmpty(idleShoot))
                    PlayState(idleShoot);
                else
                    PlayState(idleGunMiddle);
                return;
            }

            if (hasSprint)
            {
                if (!string.IsNullOrEmpty(runGuard))
                    PlayState(runGuard);
                else
                    PlayState(runGunMiddle);
                return;
            }

            if (Mathf.Abs(zInput) >= Mathf.Abs(xInput))
                PlayState(zInput >= 0 ? walkForward : walkBack);
            else
                PlayState(xInput >= 0 ? walkRight : walkLeft);

            return;
        }

        if (speed < walkThreshold)
        {
            PlayState(idleGunMiddle);
            return;
        }

        if (speed >= runThreshold)
        {
            PlayState(runGunMiddle);
            return;
        }

        Vector3 localVel = transform.InverseTransformDirection(velocity);
        if (Mathf.Abs(localVel.z) >= Mathf.Abs(localVel.x))
        {
            PlayState(localVel.z >= 0 ? walkForward : walkBack);
        }
        else
        {
            PlayState(localVel.x >= 0 ? walkRight : walkLeft);
        }
    }

    bool IsGrounded()
    {
        if (controller == null)
            return true;

        bool grounded = controller.isGrounded;
        if (!grounded)
        {
            float radius = Mathf.Max(0.05f, controller.radius * 0.9f);
            float rayDist = (controller.height * 0.5f) - controller.radius + groundCheckDistance;
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            grounded = Physics.SphereCast(origin, radius, Vector3.down, out _, rayDist, groundMask, QueryTriggerInteraction.Ignore);
        }

        if (grounded)
            lastGroundedTime = Time.time;

        return grounded || (Time.time - lastGroundedTime) <= groundedGraceTime;
    }

    void DisableLegacyAnimations()
    {
        Animation[] legacyAnimations = GetComponentsInChildren<Animation>(true);
        foreach (Animation legacy in legacyAnimations)
        {
            if (legacy == null) continue;
            legacy.Stop();
            legacy.enabled = false;
        }
    }

    public void PlayDie()
    {
        PlayState(die, true);
    }

    void PlayAction(string stateName, float duration)
    {
        PlayState(stateName, true);
        actionLockUntil = Time.time + Mathf.Max(0.05f, duration);
    }

    bool TryPlayAction(string stateName, float duration)
    {
        if (Time.time < actionLockUntil) return false;
        PlayAction(stateName, duration);
        return true;
    }

    void PlayState(string stateName, bool force = false)
    {
        if (string.IsNullOrEmpty(stateName) || animator == null) return;
        if (!force && currentState == stateName) return;

        currentState = stateName;
        animator.CrossFade(stateName, crossFadeDuration);

        if (syncViewModelAnimator && viewModelAnimator != null)
        {
            viewModelAnimator.CrossFade(stateName, crossFadeDuration);
        }

        if (useNetworkSync && photonView.IsMine)
        {
            photonView.RPC("RPC_PlayState", RpcTarget.Others, stateName);
        }
    }

    [PunRPC]
    void RPC_PlayState(string stateName)
    {
        if (animator == null) return;
        currentState = stateName;
        animator.CrossFade(stateName, crossFadeDuration);
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

    bool HasState(Animator targetAnimator, string stateName)
    {
        if (targetAnimator == null || string.IsNullOrEmpty(stateName)) return false;
        int stateHash = Animator.StringToHash(stateName);
        return targetAnimator.HasState(0, stateHash);
    }

    string ResolveStateName(Animator targetAnimator, string currentName, string primary, string fallback)
    {
        if (HasState(targetAnimator, currentName)) return currentName;
        if (HasState(targetAnimator, primary)) return primary;
        if (HasState(targetAnimator, fallback)) return fallback;
        return currentName;
    }

    string GetShootClip()
    {
        switch (fireMode)
        {
            case FireMode.Auto:
                return shootAuto;
            default:
                return shootSingle;
        }
    }
}
