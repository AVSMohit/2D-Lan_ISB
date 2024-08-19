using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PressurePads : NetworkBehaviour
{
    public Color defaultColor = Color.red;
    public Color activatedColor = Color.green;
    public Color wrongColor = Color.yellow; // Color for incorrect interaction
    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;
    public int padNumber;
    public ulong assignedPlayerId;  // Player ID assigned to this pad
    public bool isPartOfSequence;
    public int sequenceIndex = -1;  // The position of this pad in the correct sequence

    private PlayerController playerController;  // Reference to the player entering the pad's trigger
    private TMP_Text interactText;  // Reference to the interaction text

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = defaultColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                interactText = playerController.interactText;
                ShowInteractText(true);  // Show interaction text when player enters the pad
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
                ShowInteractText(false);  // Hide interaction text when player leaves the pad
                playerController = null;
            }
        }
    }

    private void Update()
    {
        if (playerController != null && Input.GetKeyDown(KeyCode.E) && !isActivated)
        {
            // When the player presses "E", check the sequence and interaction
            InteractWithPadServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractWithPadServerRpc(ulong playerId)
    {
        if (isPartOfSequence && PressurePadManager.Instance.CheckPadSequence(padNumber))
        {
            // Correct interaction
            isActivated = true;
            ActivatePadClientRpc();
            PressurePadManager.Instance.OnPadActivated(padNumber);  // Notify the manager of correct interaction
        }
        else
        {
            // Incorrect interaction, reset the game
            ShowWrongInteractionClientRpc();  // Show wrong interaction feedback
            PressurePadManager.Instance.ResetPadsClientRpc();  // Reset the game
        }
    }

    [ClientRpc]
    private void ActivatePadClientRpc()
    {
        spriteRenderer.color = activatedColor;  // Turn the pad green
        ShowInteractText(false);  // Hide interaction text when pad is activated
    }

    [ClientRpc]
    private void ShowWrongInteractionClientRpc()
    {
        StartCoroutine(BlinkWrongPad());  // Blink the pad to indicate wrong interaction
    }

    private System.Collections.IEnumerator BlinkWrongPad()
    {
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = wrongColor;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = defaultColor;
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void ResetPad()
    {
        isActivated = false;
        spriteRenderer.color = defaultColor;
        if (playerController != null)
        {
            ShowInteractText(false);  // Ensure interaction text is hidden when resetting the pad
        }
    }

    private void ShowInteractText(bool show)
    {
        if (interactText != null)
        {
            interactText.gameObject.SetActive(show);  // Show or hide the interaction text
        }
    }
}
