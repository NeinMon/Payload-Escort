using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerNetwork : MonoBehaviourPunCallbacks
{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public float sprintMultiplier = 1.5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public GameObject weapon;
    public Camera playerCamera;
    public Transform cameraRoot;
    public Transform fpsCameraRoot;
    public Vector3 weaponLocalPosition = new Vector3(0.2f, -0.2f, 0.5f);
    public Vector3 weaponLocalEuler = Vector3.zero;
    public WeaponManager weaponManager;
    public Transform viewModelRoot;
    public bool followWeaponParentForArms = true;
    public string autoViewModelRootName = "HPCharacter 1";
    public string autoCameraRootName = "Camera";
    public string autoFpsCameraName = "WeaponCamera";
    public bool forceViewModelWeaponLayer = true;
    public string viewModelLayerName = "Weapon";
    public bool hideCharacterModelForLocal = true;
    public string characterModelRootName = "CharacterModel";

    [Header("Recoil")]
    public float recoilPerShot = 0.9f;
    public float maxRecoil = 6.0f;
    public float recoilReturn = 10f;
    public float horizontalSway = 0.4f;
    public float maxHorizontalRecoil = 1.2f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float rotationX;
    private float verticalVelocity;
    private float recoilKick;
    private float horizontalKick;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (!photonView.IsMine)
        {
            Camera[] cameras = GetComponentsInChildren<Camera>(true);
            for (int i = 0; i < cameras.Length; i++)
            {
                Destroy(cameras[i].gameObject);
            }
            return;
        }

        if (weaponManager == null)
        {
            weaponManager = GetComponentInChildren<WeaponManager>();
        }

        if (cameraRoot == null && !string.IsNullOrEmpty(autoCameraRootName))
            cameraRoot = FindInHierarchy(transform, autoCameraRootName);

        if (fpsCameraRoot == null && !string.IsNullOrEmpty(autoFpsCameraName))
            fpsCameraRoot = FindInHierarchy(transform, autoFpsCameraName);

        if (playerCamera == null)
            playerCamera = fpsCameraRoot != null ? fpsCameraRoot.GetComponent<Camera>() : GetComponentInChildren<Camera>();

        if (viewModelRoot == null && !string.IsNullOrEmpty(autoViewModelRootName))
            viewModelRoot = FindInHierarchy(transform, autoViewModelRootName);

        if (forceViewModelWeaponLayer && viewModelRoot != null)
            SetLayerRecursively(viewModelRoot.gameObject, LayerMask.NameToLayer(viewModelLayerName));

        if (hideCharacterModelForLocal && !string.IsNullOrEmpty(characterModelRootName))
        {
            Transform characterModel = FindInHierarchy(transform, characterModelRootName);
            if (characterModel != null)
            {
                Renderer[] renderers = characterModel.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                    renderers[i].enabled = false;
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        UpdateRecoil();
        Move();
        Look();

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Fire();
        }
    }

    void LateUpdate()
    {
        if (!photonView.IsMine) return;
        if (playerCamera == null) return;

        Transform followTarget = playerCamera.transform;

        Transform viewModel = viewModelRoot;
        if (viewModel == null && followWeaponParentForArms && weaponManager != null)
        {
            WeaponBase currentWeapon = weaponManager.GetCurrentWeapon();
            if (currentWeapon != null && currentWeapon.transform.parent != null)
                viewModel = currentWeapon.transform.parent;
        }

        if (viewModel == null && weapon != null)
            viewModel = weapon.transform;

        if (viewModel == null) return;

        if (viewModel.parent != followTarget)
            viewModel.SetParent(followTarget, false);

        viewModel.localPosition = weaponLocalPosition;
        viewModel.localRotation = Quaternion.Euler(weaponLocalEuler);
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

    void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null || layer < 0) return;
        obj.layer = layer;
        for (int i = 0; i < obj.transform.childCount; i++)
            SetLayerRecursively(obj.transform.GetChild(i).gameObject, layer);
    }

    void Move()
    {
        moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        bool jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
        if (jumpPressed && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * currentSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    void Look()
    {
        lookInput.x = Input.GetAxis("Mouse X") * lookSpeed;
        lookInput.y = Input.GetAxis("Mouse Y") * lookSpeed;

        rotationX -= lookInput.y;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        float finalPitch = rotationX - recoilKick;
        playerCamera.transform.localRotation = Quaternion.Euler(finalPitch, 0, 0);
        transform.Rotate(Vector3.up * (lookInput.x + horizontalKick));
    }

    void Fire()
    {
        if (weapon != null)
        {
            photonView.RPC("RPC_Fire", RpcTarget.All);
        }
    }

    public float GetPitch()
    {
        return rotationX;
    }

    void UpdateRecoil()
    {
        if (recoilKick > 0f)
            recoilKick = Mathf.MoveTowards(recoilKick, 0f, recoilReturn * Time.deltaTime);

        if (Mathf.Abs(horizontalKick) > 0f)
            horizontalKick = Mathf.MoveTowards(horizontalKick, 0f, recoilReturn * Time.deltaTime);
    }

    public void AddRecoil()
    {
        recoilKick += recoilPerShot;
        recoilKick = Mathf.Clamp(recoilKick, 0f, maxRecoil);

        float sway = Random.Range(-horizontalSway, horizontalSway);
        horizontalKick = Mathf.Clamp(horizontalKick + sway, -maxHorizontalRecoil, maxHorizontalRecoil);
    }

    [PunRPC]
    void RPC_Fire()
    {
        // Simple muzzle flash or effect
        Debug.Log(name + " fired weapon.");
    }
}