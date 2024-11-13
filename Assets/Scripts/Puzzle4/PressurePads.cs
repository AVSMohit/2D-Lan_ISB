using TMPro;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PressurePads : MonoBehaviourPun
{
    public Color defaultColor = Color.red;
    public Color activatedColor = Color.green;
    public Color wrongColor = Color.yellow;
    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;
    public int padNumber;
    public int assignedPlayerId;
    public bool isPartOfSequence;
    public int sequenceIndex = -1;
    public static PressurePadManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional, if you want it to persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = defaultColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && photonView.IsMine)
        {
            // Check assigned player ID for pad interaction
        }
    }

    public void Interact()
    {
        if (isPartOfSequence && PressurePadManager.Instance.CheckPadSequence(padNumber))
        {
            photonView.RPC("ActivatePad", RpcTarget.AllBuffered);
        }
        else
        {
            photonView.RPC("ShowWrongInteraction", RpcTarget.AllBuffered);
            PressurePadManager.Instance.photonView.RPC("ResetPads", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void ActivatePad()
    {
        spriteRenderer.color = activatedColor;
        isActivated = true;
    }

    [PunRPC]
    private void ShowWrongInteraction()
    {
        StartCoroutine(BlinkWrongPad());
    }

    private IEnumerator BlinkWrongPad()
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
    }
}
