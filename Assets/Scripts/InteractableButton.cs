using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableButton : MonoBehaviour
{
    [SerializeField] private UnityEvent buttonPressed;
    [SerializeField] private UnityEvent buttonReleased;
    private HashSet<GameObject> occupants = new HashSet<GameObject>();
    public List<GameObject> exposed = new List<GameObject>();
    public bool IsPressed => occupants.Count > 0;
    public UnityEvent onStateChanged;

    [Header("Detection")]
    [SerializeField] private Vector3 checkSize = new Vector3(1f, 0.3f, 1f);
    [SerializeField] private LayerMask pusherMask;

    private bool wasPressedLastFrame;
    private void FixedUpdate()
    {
        HashSet<GameObject> detectedThisFrame = new HashSet<GameObject>();

        Collider[] hits = Physics.OverlapBox(
            transform.position,
            checkSize * 0.5f,
            transform.rotation,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (var hit in hits)
        {
            GameObject root = GetValidPusher(hit);
            if (root != null)
                detectedThisFrame.Add(root);
        }

        // State transition detection
        bool isPressedNow = detectedThisFrame.Count > 0;

        if (!wasPressedLastFrame && isPressedNow)
        {
            buttonPressed.Invoke();
            onStateChanged.Invoke();
        }
        else if (wasPressedLastFrame && !isPressedNow)
        {
            buttonReleased.Invoke();
            onStateChanged.Invoke();
        }

        wasPressedLastFrame = isPressedNow;

        occupants.Clear();
        foreach (var obj in detectedThisFrame)
            occupants.Add(obj);
    }

    private void Update()
    {
        exposed.Clear();
        foreach (var item in occupants)
            exposed.Add(item);
    }

    private GameObject GetValidPusher(Collider other)
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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, checkSize);
    }
#endif
}
