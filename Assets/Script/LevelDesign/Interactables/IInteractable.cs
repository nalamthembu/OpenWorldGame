using UnityEngine;
public interface IInteractable
{
    public abstract void TriggerInteractable();

    public abstract void OnTriggerStay(Collider other);
}