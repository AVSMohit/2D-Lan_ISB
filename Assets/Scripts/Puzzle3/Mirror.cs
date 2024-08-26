using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class Mirror : NetworkBehaviour
{
    public float rotationSpeed = 100f;
    public GameObject highlightEffect; // A child object to show the highlight
    public GameObject arrowIndicator; // A child object to show the reflection direction
    private bool playerInRange = false;
    private PlayerController playerController;

    private void Start()
    {
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(false);
        }
        if (arrowIndicator != null)
        {
            arrowIndicator.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerController = collision.GetComponent<PlayerController>();
            if (playerController != null && playerController.IsOwner)
            {
                playerInRange = true;
                playerController.interactText.gameObject.SetActive(true);
                playerController.interactText.text = "Use 'Q' & 'E' to rotate mirror";
                EnableHighlightAndArrowServerRpc(true, NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController controller = collision.GetComponent<PlayerController>();
            if (controller == playerController)
            {
                playerController.interactText.gameObject.SetActive(false);
                playerInRange = false;
                playerController = null;
                EnableHighlightAndArrowServerRpc(false, NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    private void Update()
    {
        if (playerInRange && IsOwner)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                RotateMirrorServerRpc(-rotationSpeed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                RotateMirrorServerRpc(rotationSpeed * Time.deltaTime);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RotateMirrorServerRpc(float rotationAmount)
    {
        transform.Rotate(Vector3.forward, rotationAmount);
        UpdateArrowIndicator();
        RotateMirrorClientRpc(rotationAmount);
    }

    [ClientRpc]
    private void RotateMirrorClientRpc(float rotationAmount)
    {
        if (!IsOwner)
        {
            transform.Rotate(Vector3.forward, rotationAmount);
            UpdateArrowIndicator();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc(ulong clientId)
    {
        NetworkObject.ChangeOwnership(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void EnableHighlightAndArrowServerRpc(bool enable, ulong clientId)
    {
        EnableHighlightAndArrowClientRpc(enable);
        if (enable)
        {
            NetworkObject.ChangeOwnership(clientId);
        }
    }

    [ClientRpc]
    private void EnableHighlightAndArrowClientRpc(bool enable)
    {
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(enable);
        }
        if (arrowIndicator != null)
        {
            arrowIndicator.SetActive(enable);
        }
    }

    private void UpdateArrowIndicator()
    {

        if (arrowIndicator != null)
        {
            Vector2 reflectedDirection = Vector2.Reflect(transform.right, transform.up);
            float angle = Mathf.Atan2(reflectedDirection.y, reflectedDirection.x) * Mathf.Rad2Deg;
            arrowIndicator.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
    }
}
