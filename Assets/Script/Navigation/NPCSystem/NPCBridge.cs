using UnityEngine;

namespace NPCSystem
{
    public class NPCBridge : IPathNode
    {
        [SerializeField] IPathNode[] ends;

        public override void OnDrawGizmos()
        {
            if (drawPath)
            {
                Gizmos.color = Color.yellow;

                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.25F);

                Gizmos.DrawLine(transform.position, transform.position + transform.up);

                Gizmos.color = Color.red;

                for (int i = 0; i < ends.Length; i++)
                {
                    if (ends[i] is null)
                    {
                        continue;
                    }

                    Gizmos.DrawLine(transform.position, ends[i].transform.position);
                }
            }
        }
    }
}