using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private bool updateInEditor;
    [SerializeField] private Transform startPoint, endPoint;
    public Camera cam;



    private void SetLine()
    {
        if (lineRenderer == null || startPoint == null || endPoint == null) return;
        if (updateInEditor)
        {
            lineRenderer.SetPosition(0, startPoint.localPosition);
            lineRenderer.SetPosition(1, endPoint.localPosition);
        }
    }

    void Start()
    {
        SetLine();
        GenerateMeshCollider();
    }


    public void GenerateMeshCollider()
    {
        if (lineRenderer == null || startPoint == null || endPoint == null) return;
        MeshCollider collider = GetComponent<MeshCollider>();

        if (collider == null)
        {
            collider = gameObject.AddComponent<MeshCollider>();
        }


        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, cam, true);

        int[] meshIndices = mesh.GetIndices(0);
        int[] newIndices = new int[meshIndices.Length * 2];

        int j = meshIndices.Length - 1;
        for (int i = 0; i < meshIndices.Length; i++)
        {
            newIndices[i] = meshIndices[i];
            newIndices[meshIndices.Length + i] = meshIndices[j];
        }
        mesh.SetIndices(newIndices, MeshTopology.Triangles, 0);

        collider.sharedMesh = mesh;
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
