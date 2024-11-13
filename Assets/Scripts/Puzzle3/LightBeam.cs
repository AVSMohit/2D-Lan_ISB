using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LightBeam : MonoBehaviourPun, IPunObservable
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
        if (PhotonNetwork.IsMasterClient)
        {
            Vector3[] positions = DrawLightBeam();
            photonView.RPC("UpdateLightBeamRPC", RpcTarget.All, positions, positions.Length);
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

                if (hit.collider.CompareTag("Target"))
                {
                    TargetToHit target = hit.collider.GetComponent<TargetToHit>();
                    if (target != null && PhotonNetwork.IsMasterClient)
                    {
                        target.OnHit();
                        lastHitTarget = target;
                        targetHitThisFrame = true;
                    }
                    break;
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

        if (lastHitTarget != null && !targetHitThisFrame)
        {
            lastHitTarget.ResetTarget();
            lastHitTarget = null;
        }

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());

        return positions.ToArray();
    }

    [PunRPC]
    private void UpdateLightBeamRPC(Vector3[] positions, int count)
    {
        lineRenderer.positionCount = count;
        lineRenderer.SetPositions(positions);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(lineRenderer.positionCount);
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                stream.SendNext(lineRenderer.GetPosition(i));
            }
        }
        else
        {
            lineRenderer.positionCount = (int)stream.ReceiveNext();
            Vector3[] positions = new Vector3[lineRenderer.positionCount];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = (Vector3)stream.ReceiveNext();
            }
            lineRenderer.SetPositions(positions);
        }
    }
}
