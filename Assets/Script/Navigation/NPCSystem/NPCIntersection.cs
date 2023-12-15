using UnityEngine;

namespace NPCSystem
{
    public class NPCIntersection : IPathNode
    {
        public override void OnDrawGizmos()
        {
            Gizmos.matrix = transform.worldToLocalMatrix;

            Gizmos.color = new Color(1, 1, 0);

            Gizmos.DrawSphere(transform.position, 0.25f);

            Gizmos.DrawWireCube(transform.position, new Vector3(8.0F, 0.15F, 2.0F));

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
}