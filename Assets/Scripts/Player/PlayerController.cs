using System.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float jumpForce = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded = true;
    public LayerMask groundLayer;
    private bool gravityEnabled = false;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;

    [Header("UI & Visuals")]
    public TMP_Text interactText;
    private CameraController cameraController;
    public float weight = 1f;

    // ✅ Network-synced color (server-authoritative)
    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
        {
            cameraController.AddPlayer(transform);
        }

        // ✅ Apply synced color on every client
        playerColor.OnValueChanged += (oldColor, newColor) =>
        {
            GetComponent<SpriteRenderer>().color = newColor;
        };

        // Apply existing color (in case spawn happens late)
        GetComponent<SpriteRenderer>().color = playerColor.Value;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // ✅ Only server sets spawn position + color
        if (IsServer)
        {
            StartCoroutine(DelayedAssignSpawnAndColor());
        }
    }

    private IEnumerator DelayedAssignSpawnAndColor()
    {
        yield return new WaitForSeconds(0.25f);

        // ✅ Set spawn position
        var spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            var spawn = spawnManager.GetSpawnPointForPlayer(OwnerClientId);
            if (spawn != null)
            {
                transform.position = spawn.position;
                Debug.Log($"✅ [SERVER] Player {OwnerClientId} moved to spawn: {spawn.position}");
            }
            else
            {
                Debug.LogWarning($"❌ No spawn point for player {OwnerClientId}");
            }
        }

        // ✅ Apply gender-based color
        string gender = PlayerPrefs.GetString("PlayerGender", "Male");
        Color newColor = Color.white;

        if (gender == "Male")
            ColorUtility.TryParseHtmlString("#007AFE", out newColor);
        else if (gender == "Female")
            ColorUtility.TryParseHtmlString("#FE88BD", out newColor);

        playerColor.Value = newColor;
        Debug.Log($"🎨 [SERVER] Applied color {newColor} to Player {OwnerClientId}");
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

    private void Update()
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

    private void FixedUpdate()
    {
        if (!IsOwner) return;

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
