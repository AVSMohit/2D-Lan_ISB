using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Collections;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed;
    public float jumpForce = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded = true;
    public LayerMask groundLayer;
    private bool gravityEnabled = false;

    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;

    CameraController cameraController;
    public TMP_Text interactText;
    public float weight = 1f;

    // Sprites for color changes
    SpriteRenderer bodySr;
    SpriteRenderer headSr;

    // Network variable to hold gender ("male" or "female")
    public NetworkVariable<FixedString32Bytes> gender = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        cameraController = FindObjectOfType<CameraController>();

        // Get the body sprite (assume it's on the same GameObject)
        bodySr = GetComponent<SpriteRenderer>();
        // For head, try to get all SpriteRenderers and use one that is not the body.
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        if (srs.Length > 1)
        {
            // Assume the first one is the body and the second one is the head.
            headSr = srs[1];
        }
        else
        {
            // Fallback: if only one is found, use it for both.
            headSr = bodySr;
        }

        // Subscribe to changes so that color updates on all clients.
        gender.OnValueChanged += OnGenderChanged;

        if (IsOwner)
        {
            // Read the chosen gender from PlayerPrefs (set via your GenderSelection UI)
            string chosenGender = PlayerPrefs.GetString("Gender", "male");
            SetGenderServerRpc(chosenGender);
        }

        // Update the color based on the network variable
        OnGenderChanged(default, gender.Value);

        if (cameraController != null)
        {
            cameraController.AddPlayer(transform);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetGenderServerRpc(string genderValue, ServerRpcParams rpcParams = default)
    {
        gender.Value = genderValue;
    }

    private void OnGenderChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        if (bodySr == null)
            return;

        string g = newValue.ToString().ToLower();
        if (g == "female")
        {
            // Set to a pinkish color
            Color32 femaleColor = new Color32(254, 136, 136, 255);
            bodySr.color = femaleColor;
            if (headSr != null)
                headSr.color = femaleColor;
        }
        else
        {
            // Assume male - set to a blue color
            Color32 maleColor = new Color32(0, 122, 254, 255);
            bodySr.color = maleColor;
            if (headSr != null)
                headSr.color = maleColor;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        moveInput.x = Input.GetAxis("Horizontal");

        if (gravityEnabled)
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                Jump();
            }
        }
        else
        {
            moveInput.y = Input.GetAxis("Vertical");
        }
    }

    void FixedUpdate()
    {
        if (IsOwner)
        {
            if (gravityEnabled)
            {
                rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
                isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            }
            else
            {
                rb.velocity = moveInput * moveSpeed;
            }
            rb.gravityScale = gravityEnabled ? 1 : 0;
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isGrounded = false;
    }

    public void SetGravity(bool enabled)
    {
        gravityEnabled = enabled;
    }
}
