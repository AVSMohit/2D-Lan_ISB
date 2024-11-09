using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerController : MonoBehaviourPun
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

    private CameraController cameraController;

    public TMP_Text interactText;

    public float weight = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        if (photonView.IsMine)
        {
            cameraController = FindObjectOfType<CameraController>();
            if (cameraController != null)
            {
                cameraController.AddPlayer(transform);
            }
        }
    }

    private void OnEnable()
    {
        if (photonView.IsMine)
        {
            gameObject.tag = "Player";
        }
    }

    private void OnDisable()
    {
        if (photonView.IsMine && cameraController != null)
        {
            cameraController.RemovePlayer(transform);
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

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

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            if (gravityEnabled)
            {
                rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);

                // Check if grounded
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
