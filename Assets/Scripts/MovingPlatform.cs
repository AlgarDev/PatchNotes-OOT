using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool loop = true;
    [SerializeField] private float waitTimeAtPoint = 0f;
    [SerializeField] private bool canMove = false;
    [SerializeField] private Transform ghostTransform;

    [Header("Detection")]
    [SerializeField] private Vector3 checkSize = new Vector3(2f, 0.5f, 2f);

    private int currentIndex = 0;
    private float waitTimer;
    private Vector3 lastPosition;

    private HashSet<GameObject> occupants = new HashSet<GameObject>();
    public List<GameObject> exposed = new List<GameObject>();


    private void Start()
    {
        if (waypoints.Count == 0)
        {
            print("No waypoints");
            return;
        }
        transform.position = waypoints[0].position;
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (waypoints.Count < 2)
            return;
        if (canMove)
        {
            MovePlatform();
        }
            HandleOccupants();
    }

    private void MovePlatform()
    {

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            lastPosition = transform.position;
            return;
        }

        Vector3 target = waypoints[currentIndex].position;
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            currentIndex++;

            if (currentIndex >= waypoints.Count)
            {
                if (loop)
                    currentIndex = 0;
                else
                    currentIndex = waypoints.Count - 1;
            }

            waitTimer = waitTimeAtPoint;
        }

        lastPosition = transform.position;

    }
    private void HandleOccupants()
    {
        HashSet<GameObject> detectedThisFrame = new HashSet<GameObject>();

        Collider[] hits = Physics.OverlapBox(
            transform.position + Vector3.up * 0.5f,
            checkSize * 0.5f,
            transform.rotation,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (var hit in hits)
        {
            GameObject root = GetValidOccupant(hit);
            if (root != null)
            {
                detectedThisFrame.Add(root);

                // Enter (new this frame)
                if (!occupants.Contains(root))
                {
                    if (root.TryGetComponent(out ThirdPersonController player))
                        player.SetPlatformGhost(ghostTransform);
                    else if (root.TryGetComponent(out InteractableBox box))
                        box.SetPlatformGhost(ghostTransform);
                }
            }
        }

        // Exit (missing this frame)
        foreach (var obj in occupants)
        {
            if (obj == null)
                continue;
            if (!detectedThisFrame.Contains(obj))
            {
                if (obj.TryGetComponent(out ThirdPersonController player))
                {
                    player.ClearPlatformGhost(ghostTransform);
                }
                else if (obj.TryGetComponent(out InteractableBox box))
                    box.ClearPlatformGhost(ghostTransform);
            }
        }

        occupants = detectedThisFrame;

        exposed.Clear();
        foreach (var item in detectedThisFrame)
        //print(item);
            exposed.Add(item);
    }


    private GameObject GetValidOccupant(Collider other)
    {
        var controller = other.GetComponentInParent<ThirdPersonController>();
        if (controller != null)
            return controller.gameObject;

        var box = other.GetComponentInParent<InteractableBox>();
        if (box != null)
            return box.gameObject;

        return null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Detection volume
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Gizmos.matrix = Matrix4x4.TRS(
            transform.position + Vector3.up * 0.5f,
            transform.rotation,
            Vector3.one
        );
        Gizmos.DrawCube(Vector3.zero, checkSize);
    }

#endif

    public void CanMove(bool move)
    {
        canMove = move;
    }
    public Vector3 GetDelta()
    {
        return transform.position - lastPosition;
    }
}
