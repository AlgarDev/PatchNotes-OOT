using UnityEngine;
interface IInteractable
{
    public void Interact(ThirdPersonController interactor);
}
interface IGrabbable
{
    public void Pickup(ThirdPersonController interactor);
    public void Drop(ThirdPersonController interactor);
}
[RequireComponent(typeof(Rigidbody))]
public class InteractableBox : MonoBehaviour, IInteractable, IGrabbable
{
    private Rigidbody rb;
    private bool isHeld;
    private ThirdPersonController holder;
    private Collider boxCollider;
    
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
}
