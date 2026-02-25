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

        Vector3 start = startPoint.position;
        Vector3 end = endPoint.position;

        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        float radius = .5f; // laser thickness

        // Blocker check
        if (Physics.CapsuleCast(
            start,
            start + direction * 0.01f, // tiny height capsule
            radius,
            direction,
            out RaycastHit hitBlocker,
            distance,
            blockerLayer,
            QueryTriggerInteraction.Ignore))
        {
            Vector3 targetLocalPos =
                lineRenderer.transform.InverseTransformPoint(hitBlocker.point);
            Vector3 Pos = endPoint.localPosition;
            Pos.z = targetLocalPos.z;

            SetLine(Pos);
        }
        else
        {
            SetLine(endPoint.localPosition);
        }
        // Player / clone hit check
        if (Physics.CapsuleCast(
            start,
            start + direction * 0.01f,
            radius,
            direction,
            out RaycastHit hitPlayer,
            distance,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            HandleHit(hitPlayer.collider);
        }
    }


    private void HandleHit(Collider other)
    {
        if (other.TryGetComponent(out ThirdPersonController player))
        {
            if (player.IsPlayerControlled)
            {
                print("Player hit laser");
                player.RespawnAtCheckpoint();
            }
            else
            {
                if (player.TryGetComponent(out CloneController clone))
                {
                    clone.activeSpawner.StopRecording();
                    print("Clone hit laser");
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (startPoint == null || endPoint == null) return;

        float radius = .5f;

        Vector3 start = startPoint.position;
        Vector3 end = endPoint.position;
        Vector3 dir = (end - start).normalized;
        float dist = Vector3.Distance(start, end);

        Gizmos.color = Color.red;

        // Draw center line
        Gizmos.DrawLine(start, end);

        // Draw capsule ends
        Gizmos.DrawWireSphere(start, radius);
        Gizmos.DrawWireSphere(end, radius);

        // Draw capsule sides (approx)
        Vector3 right = Vector3.Cross(dir, Vector3.up).normalized * radius;
        Vector3 up = Vector3.Cross(dir, right).normalized * radius;

        Gizmos.DrawLine(start + right, end + right);
        Gizmos.DrawLine(start - right, end - right);
        Gizmos.DrawLine(start + up, end + up);
        Gizmos.DrawLine(start - up, end - up);
    }

}
