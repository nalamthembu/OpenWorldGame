using UnityEngine;

[
    RequireComponent
    (
        typeof(BoxCollider)
    )
]

//Any script that is interactable must inherit this.
//Please use DoorInteraction.cs as an example of an interactable script.
public class Interactable : MonoBehaviour, IInteractable
{
    new private BoxCollider collider;

    //Can be overriden
    protected virtual void Awake()
    {
        collider = GetComponent<BoxCollider>();

        //Making sure its a trigger
        collider.isTrigger = true;
    }

    //Make sure to override this in the new interactable
    public virtual void OnTriggerStay(Collider other)
    {
        throw new System.NotImplementedException();
    }

    //Make sure to override this in the new interactable
    public virtual void TriggerInteractable()
    {
        throw new System.NotImplementedException();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5F + transform.right / 2, Vector3.one / 2);
    }
}