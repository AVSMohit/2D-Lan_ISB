using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class PlayerController : NetworkBehaviour
{
    public float moveSpeed;
    private Rigidbody2D rb;
    Vector2 moveInput;

    CameraController cameraController;

    public TMP_Text interactText;

    

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
        if(!IsOwner) return;

        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            rb.velocity = moveInput * moveSpeed;
        }
    }
}
