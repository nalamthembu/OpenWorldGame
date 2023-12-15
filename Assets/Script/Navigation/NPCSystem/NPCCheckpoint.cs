using UnityEngine;

namespace NPCSystem
{
    public class NPCCheckpoint : IPathNode
    {
        public override void OnDrawGizmos()
        {
            if (drawPath)
            {
                Gizmos.color = Color.yellow;

                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.25F);

                Gizmos.DrawLine(transform.position, transform.position + transform.up);

                Gizmos.color = Color.blue;
            }
        }
    }
}