using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Mirror : MonoBehaviourPun
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
            if (playerController != null && photonView.IsMine)
            {
                playerInRange = true;
                playerController.interactText.gameObject.SetActive(true);
                playerController.interactText.text = "Use 'Q' & 'E' to rotate mirror";
                photonView.RPC("EnableHighlightAndArrowRPC", RpcTarget.AllBuffered, true);
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
                photonView.RPC("EnableHighlightAndArrowRPC", RpcTarget.AllBuffered, false);
            }
        }
    }

    private void Update()
    {
        if (playerInRange && photonView.IsMine)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                RotateMirror(-rotationSpeed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                RotateMirror(rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void RotateMirror(float rotationAmount)
    {
        transform.Rotate(Vector3.forward, rotationAmount);
        UpdateArrowIndicator();
        photonView.RPC("RotateMirrorRPC", RpcTarget.Others, rotationAmount);
    }

    [PunRPC]
    private void RotateMirrorRPC(float rotationAmount)
    {
        transform.Rotate(Vector3.forward, rotationAmount);
        UpdateArrowIndicator();
    }

    [PunRPC]
    private void EnableHighlightAndArrowRPC(bool enable)
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
