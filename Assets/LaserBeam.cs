using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform startPoint, endPoint;

    [SerializeField] private LayerMask blockerLayer;


    private void SetLine(Vector3 targetPos)
    {
        if (lineRenderer == null || startPoint == null || endPoint == null) return;


        lineRenderer.SetPosition(0, startPoint.localPosition);
        lineRenderer.SetPosition(1, targetPos);
    }

    private void Update()
    {
        if (startPoint == null || endPoint == null) return;

        Vector3 direction = (endPoint.position - startPoint.position).normalized;
        float distance = Vector3.Distance(startPoint.position, endPoint.position);

        // Check if something is blocking the laser
        if (Physics.Raycast(startPoint.position, direction, out RaycastHit hitBlocker, distance, blockerLayer) && !hitBlocker.collider.isTrigger)
        {
            // Laser is blocked, cut it at the blocker
            Vector3 targetLocalPos = lineRenderer.transform.InverseTransformPoint(hitBlocker.point);

            SetLine(targetLocalPos);
            // Player/clones behind this point are safe
        }
        else
        {
            // No blocker, full length
            SetLine(endPoint.localPosition);

            // Raycast for player/clone hits
            if (Physics.Raycast(startPoint.position, direction, out RaycastHit hitPlayer, distance))
            {
                HandleHit(hitPlayer.collider);
            }
        }
    }

    private void HandleHit(Collider other)
    {
        if (other.TryGetComponent(out ThirdPersonController player))
        {
            if (player.IsPlayerControlled)
            {
                print("Player hit laser");
                // player.RespawnAtCheckpoint();
            }
            else
            {
                if (player.TryGetComponent(out CloneController clone))
                {
                    clone.StopPlayback();
                    print("Clone hit laser");
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (lineRenderer == null || startPoint == null || endPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(startPoint.position, endPoint.position);

        Gizmos.DrawSphere(startPoint.position, 0.3f);
        Gizmos.DrawSphere(endPoint.position, 0.3f);
    }
}
