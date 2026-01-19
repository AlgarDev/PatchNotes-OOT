using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableButton : MonoBehaviour
{
    [SerializeField] private UnityEvent buttonPressed;
    [SerializeField] private UnityEvent buttonReleased;
    private HashSet<GameObject> occupants = new HashSet<GameObject>();
    public List<GameObject> exposed = new List<GameObject>();
    private void OnTriggerEnter(Collider other)
    {
        GameObject root = GetValidPusher(other);
        if (root == null)
            return;

        bool wasEmpty = occupants.Count == 0;
        occupants.Add(root);

        if (wasEmpty && occupants.Count == 1)
            buttonPressed.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject root = GetValidPusher(other);
        if (root == null)
            return;

        occupants.Remove(root);

        if (occupants.Count == 0)
            buttonReleased.Invoke();
    }

    private void Update()
    {
        CleanupDestroyedOccupants();
        exposed.Clear();
        foreach (var item in occupants)
        {
            exposed.Add(item);
        }
    }

    private void CleanupDestroyedOccupants()
    {
        if (occupants.Count == 0)
            return;

        bool removedAny = false;

        var toRemove = new List<GameObject>();

        foreach (var obj in occupants)
        {
            if (obj == null || !obj.activeInHierarchy)
                toRemove.Add(obj);
        }

        foreach (var obj in toRemove)
        {
            occupants.Remove(obj);
            removedAny = true;
        }

        if (removedAny && occupants.Count == 0)
            buttonReleased.Invoke();
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
}
