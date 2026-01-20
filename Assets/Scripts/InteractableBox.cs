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
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<Collider>();

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

        gameObject.transform.position = interactor.grabPivot.position;
        boxCollider.isTrigger = true;
        transform.SetParent(interactor.grabPivot);
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

    }

    public void Drop(ThirdPersonController interactor)
    {

        isHeld = false;
        boxCollider.isTrigger = false;

        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.useGravity = true;
        transform.SetParent(null);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        holder = null;
    }
    public void Throw(ThirdPersonController interactor)
    {
        if (interactor == null)
            return;

        isHeld = false;
        boxCollider.isTrigger = false;

        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Calculate throw direction
        Vector3 throwDirection = (interactor.transform.forward + Vector3.up * upwardBias).normalized;

        // Apply impulse
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        holder = null;
    }
}
