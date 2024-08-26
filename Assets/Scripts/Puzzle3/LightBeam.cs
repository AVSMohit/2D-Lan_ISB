using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using static UnityEngine.GraphicsBuffer;

public class LightBeam : NetworkBehaviour
{

    public LineRenderer lineRenderer;
    public LayerMask reflectLayer;
    public LayerMask targetLayer;
    public int maxReflections = 5;
    private Vector2 direction;
    private TargetToHit lastHitTarget = null;

    private void Start()
    {
        direction = transform.right;
    }

    private void Update()
    {
        if (IsServer)
        {
            Vector3[] positions = DrawLightBeam();
            UpdateLightBeamClientRpc(positions, positions.Length);
        }
    }

    private Vector3[] DrawLightBeam()
    {
        List<Vector3> positions = new List<Vector3> { transform.position };

        Vector2 currentDirection = direction;
        Vector3 currentPosition = transform.position;
        bool targetHitThisFrame = false;

        for (int i = 0; i < maxReflections; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, currentDirection, Mathf.Infinity, reflectLayer | targetLayer);
            if (hit.collider != null)
            {
                positions.Add(hit.point);

                // Check if the hit object is a target
                if (hit.collider.CompareTag("Target"))
                {
                    // Call the target's OnHit method
                    TargetToHit target = hit.collider.GetComponent<TargetToHit>();
                    if (target != null)
                    {
                        target.OnHit();
                        lastHitTarget = target;
                        targetHitThisFrame = true;
                    }
                    break; // Stop the beam when it hits a target
                }

                currentDirection = Vector2.Reflect(currentDirection, hit.normal);
                currentPosition = hit.point;
            }
            else
            {
                positions.Add(currentPosition + (Vector3)currentDirection * 100);
                break;
            }
        }

        // If the target was hit in the previous frame but not in this frame, reset the target
        if (lastHitTarget != null && !targetHitThisFrame)
        {
            lastHitTarget.ResetTarget();
            lastHitTarget = null;
        }

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());

        return positions.ToArray();
    }

    [ClientRpc]
    private void UpdateLightBeamClientRpc(Vector3[] positions, int count)
    {
        lineRenderer.positionCount = count;
        lineRenderer.SetPositions(positions);
    }

}
