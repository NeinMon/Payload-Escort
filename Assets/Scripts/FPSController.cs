using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviourPun
{
    public float speed = 6.0f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public Transform playerCamera;
    public float mouseSensitivity = 100f;
    public float shootRange = 100f;
    public float shootForce = 10f;

    [Header("Recoil")]
    public float recoilPerShot = 0.9f;
    public float maxRecoil = 6.0f;
    public float recoilReturn = 10f;
    public float horizontalSway = 0.4f;
    public float maxHorizontalRecoil = 1.2f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private WeaponManager weaponManager;
    private float recoilKick;
    private float horizontalKick;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        weaponManager = GetComponentInChildren<WeaponManager>();

        if (!photonView.IsMine)
        {
            playerCamera.gameObject.SetActive(false);
            return;
        }

        AudioListener listener = playerCamera != null ? playerCamera.GetComponent<AudioListener>() : null;
        if (listener != null) listener.enabled = true;
        SingleAudioListenerEnforcer.EnsureSingleListener();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        HandleMovement();
        UpdateRecoil();
        HandleMouseLook();
        HandleShooting();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        bool jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
        if (jumpPressed && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        float finalPitch = xRotation - recoilKick;
        playerCamera.localRotation = Quaternion.Euler(finalPitch, 0f, 0f);
        transform.Rotate(Vector3.up * (mouseX + horizontalKick));
    }

    public float GetPitch()
    {
        return xRotation;
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

    void HandleShooting()
    {
        if (weaponManager != null) return;
        if (Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, shootRange))
            {
                Debug.Log($"Hit {hit.transform.name}");

                // Check if hit a player
                PlayerHealth targetHealth = hit.transform.GetComponent<PlayerHealth>();
                if (targetHealth != null)
                {
                    // Use new damage method with attacker tracking
                    PhotonView targetView = hit.transform.GetComponent<PhotonView>();
                    if (targetView != null)
                    {
                        int attackerID = photonView.Owner.ActorNumber;
                        targetView.RPC("TakeDamageFromPlayer", RpcTarget.All, 25f, attackerID);
                        Debug.Log($"{PhotonNetwork.LocalPlayer.NickName} damaged {targetView.Owner.NickName}");
                    }
                }
                else if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * shootForce);
                }
            }
        }
    }
}