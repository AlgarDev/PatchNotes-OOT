using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
interface IInteractable
{
    public void Interact(ThirdPersonController interactor);
}
interface IGrabbable
{
    public void Pickup(ThirdPersonController interactor);
    public void Drop(ThirdPersonController interactor);
    public void Throw(ThirdPersonController interactor);
}
[RequireComponent(typeof(Rigidbody))]
public class InteractableBox : MonoBehaviour, IInteractable, IGrabbable
{
    private Rigidbody rb;
    private bool isHeld;
    private ThirdPersonController holder;
    private Collider boxCollider;
    [SerializeField] private float throwForce = 8f;
    [SerializeField] private float upwardBias = 0.2f;
    private Transform platformGhost;
    private Vector3 lastPlatformGhostPosition;
    private void Awake()
    {
        if (TryGetComponent<Rigidbody>(out Rigidbody rigidBody))
        {
            rb = rigidBody;
        }
        boxCollider = GetComponent<Collider>();           
    }
    private void FixedUpdate()
    {
        if (platformGhost == null)
            return;

        if (rb.isKinematic)
            return;

        Vector3 platformDelta = platformGhost.position - lastPlatformGhostPosition;
        rb.MovePosition(rb.position + platformDelta);
        lastPlatformGhostPosition = platformGhost.position;
    }

    public void Interact(ThirdPersonController interactor)
    {
        if (!isHeld)
        {
            Pickup(interactor);

        }
    }

    public void Pickup(ThirdPersonController interactor)
    {
        if (interactor == null || interactor.transform == null)
            return;
        holder = interactor;
        isHeld = true;

        rb.useGravity = false;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        gameObject.transform.position = interactor.grabPivot.position;
        boxCollider.isTrigger = true;
        transform.SetParent(interactor.grabPivot);
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.None;
        transform.rotation = Quaternion.identity;



    }

    public void Drop(ThirdPersonController interactor)
    {
        isHeld = false;

        transform.SetParent(null);
        if (interactor.isTooClose)
        {
            transform.position = interactor.transform.position + Vector3.up * 5f;
            print("wall");
        }
        else
        {
            transform.position = interactor.grabPivot.position + interactor.transform.forward * 2.5f;
            print("no wall");

        }
        transform.rotation = Quaternion.identity;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        StartCoroutine(EnableColliderNextFixedFrame());
        holder = null;
    }
    public void Throw(ThirdPersonController interactor)
    {
        if (interactor == null)
            return;

        isHeld = false;
        boxCollider.isTrigger = false;

        transform.SetParent(null);
        transform.rotation = Quaternion.identity;

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Calculate throw direction
        Vector3 throwDirection = (interactor.transform.forward + Vector3.up * upwardBias).normalized;

        // Apply impulse
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        holder = null;
    }
    public void EnableControl(bool value)
    {
        rb.isKinematic = !value;    
        rb.useGravity = value;
    }
    public void SetPlatformGhost(Transform ghost)
    {
        platformGhost = ghost;
        lastPlatformGhostPosition = ghost.position;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

    }

    public void ClearPlatformGhost(Transform ghost)
    {
        if (platformGhost == ghost)
            platformGhost = null;
    }
    private IEnumerator EnableColliderNextFixedFrame()
    {
        yield return new WaitForFixedUpdate();
        boxCollider.isTrigger = false;
    }

}
    