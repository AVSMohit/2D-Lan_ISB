using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed;
    private Rigidbody2D rb;
    Vector2 moveInput;

    // Start is called before the first frame update
    void Start()
    {
            rb = GetComponent<Rigidbody2D>();    
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
