using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MovingPlatform : NetworkBehaviour
{
    public float requiredWeight = 2f;
    public float moveSpeed = 2f;
    public Transform targetPosition; // The target position the platform should move to
    public float delayBeforeMovingDown = 3f; // Delay before moving down
    private float currentWeight = 0f;
    private bool isMoving = false;
    private Vector3 startPosition;
    private List<PlayerController> playersOnPlatform = new List<PlayerController>();
    private Vector3 lastPosition;

    private void Start()
    {
        startPosition = transform.position;
        lastPosition = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                currentWeight += playerController.weight;
                playersOnPlatform.Add(playerController);
                CheckWeight();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                currentWeight -= playerController.weight;
                playersOnPlatform.Remove(playerController);
                CheckWeight();
            }
        }
    }

    private void CheckWeight()
    {
        if (!isMoving && currentWeight >= requiredWeight)
        {
            isMoving = true;
            StartCoroutine(MovePlatform(targetPosition.position));
        }
        else if (isMoving && currentWeight < requiredWeight)
        {
            StopAllCoroutines();
            StartCoroutine(WaitAndMovePlatformDown());
        }
    }

    private IEnumerator MovePlatform(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            Vector3 movement = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime) - transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            UpdatePlayerPositions(movement);
            yield return null;
        }
    }

    private IEnumerator WaitAndMovePlatformDown()
    {
        yield return new WaitForSeconds(delayBeforeMovingDown);
        StartCoroutine(MovePlatform(startPosition));
        isMoving = false;
    }

    private void UpdatePlayerPositions(Vector3 movement)
    {
        foreach (var player in playersOnPlatform)
        {
            player.transform.position += movement;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivatePlatformMovementServerRpc(ServerRpcParams rpcParams = default)
    {
        CheckWeight();
    }

    [ClientRpc]
    private void UpdatePlatformPositionClientRpc(Vector3 position)
    {
        Vector3 movement = position - transform.position;
        transform.position = position;
        UpdatePlayerPositions(movement);
    }

    private void Update()
    {
        if (IsServer)
        {
            UpdatePlatformPositionClientRpc(transform.position);
        }
    }
}
