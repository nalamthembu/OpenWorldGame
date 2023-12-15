using System.Collections.Generic;
using UnityEngine;
using NPCSystem;

public abstract class IPathNode : MonoBehaviour
{
    [SerializeField] public IPathNode[] nextCheckpoints;

    [SerializeField] protected bool drawPath;

    private void Awake()
    {
        if (TryGetComponent(out Collider _) is false)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();

            collider.size = Vector3.right * 10F + Vector3.forward * 5 + Vector3.up * 10F;

            collider.isTrigger = true;
        }
    }

    public virtual void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0);

        Gizmos.DrawSphere(transform.position, 0.25f);

        if (drawPath)
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < nextCheckpoints.Length; i++)
            {
                if (nextCheckpoints[i] is null)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(transform.position, transform.up);
                    continue;
                }

                Gizmos.DrawLine(transform.position, nextCheckpoints[i].transform.position);
            }
        }
    }
}