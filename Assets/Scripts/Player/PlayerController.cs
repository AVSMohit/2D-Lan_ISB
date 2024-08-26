using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Security.Cryptography;
public class PlayerController : NetworkBehaviour
{
    public float moveSpeed;
    public float jumpForce = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded = true;
    public LayerMask groundLayer;
    private bool gravityEnabled = true;

    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;

    CameraController cameraController;

    public TMP_Text interactText;

    public float weight = 1f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
        {
            cameraController.AddPlayer(transform);
        }
    }

    private void OnEnable()
    {
        gameObject.tag = "Player";
    }

    private void OnDisable()
    {
        if (cameraController != null)
        {
            cameraController.RemovePlayer(transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        moveInput.x = Input.GetAxis("Horizontal");

        if (gravityEnabled)
        {
            // Handle jumping only when gravity is enabled
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                Jump();
            }
        }
        else
        {
            // Allow vertical movement when gravity is disabled
            moveInput.y = Input.GetAxis("Vertical");
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            if (gravityEnabled)
            {
                // Apply gravity and horizontal movement when gravity is enabled
                rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);

                // Check if grounded
                isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            }
            else
            {
                // Allow free movement when gravity is disabled
                rb.velocity = moveInput * moveSpeed;
            }

            // Apply gravity scale
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
