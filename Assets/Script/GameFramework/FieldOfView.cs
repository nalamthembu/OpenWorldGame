#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;

namespace OWFramework.AI
{
    public class FieldOfView : MonoBehaviour
    {
        [SerializeField][Range(1, 100)] float m_ViewRadius;
        [SerializeField][Range(0, 360)] float m_ViewAngle;
        [SerializeField][Range(0, 100)] float m_NoticeSpeed; // How fast do they notice?
        [SerializeField] LayerMask m_TargetMask;
        [SerializeField] LayerMask m_ObstacleMask;

        float m_NoticeAmount; //How much do you notice what you're seeing.
        public float NoticeAmount { get { return m_NoticeAmount; } }

        public float GetViewRadius() => m_ViewRadius;
        public float GetViewAngle() => m_ViewAngle;
        public void SetMasks(LayerMask targetMask, LayerMask obstacleMask)
        { m_TargetMask = targetMask; m_ObstacleMask = obstacleMask; }

        // TODO : Return all of them?
        // Returns the first visible Target
        public bool HasVisibleTarget(out Transform visibleTarget)
        {
            visibleTarget = null;

            Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, m_ViewRadius, m_TargetMask, QueryTriggerInteraction.Ignore);

            foreach (Collider targetCol in targetsInViewRadius)
            {
                Transform target = targetCol.transform;
                Vector3 dirToTarget = (target.position - transform.position).normalized;

                //If target is in view
                if (Vector3.Angle(transform.forward, dirToTarget) < m_ViewAngle / 2)
                {
                    float distToTarget = Vector3.Distance(transform.position, target.position);

                    if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, m_ObstacleMask, QueryTriggerInteraction.Ignore))
                    {
                        Debug.DrawRay(target.position, transform.up * 2, Color.magenta);

                        visibleTarget = target;

                        // Take notice of what you see.
                        m_NoticeAmount += Time.deltaTime * (m_NoticeSpeed * 0.100F);

                        // Clamp it.
                        if (m_NoticeAmount > 1) m_NoticeAmount = 1;

                        return true;
                    }
                }
            }

            // Slowly lose track of what you saw
            m_NoticeAmount -= Time.deltaTime;

            // Clamp it
            if (m_NoticeAmount <= 0) m_NoticeAmount = 0;

            return false;
        }

        public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
        {
            if (!angleIsGlobal)
                angleInDegrees += transform.eulerAngles.y;

            return new()
            {
                x = Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),
                y = 0,
                z = Mathf.Cos(angleInDegrees * Mathf.Deg2Rad)
            };
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(FieldOfView))]
    public class FieldOfViewEditor : Editor
    {
        private void OnSceneGUI()
        {
            FieldOfView fov = (FieldOfView)target;
            Handles.color = Color.white;
            Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.GetViewRadius());
            
            Vector3 viewAngleA = fov.DirectionFromAngle(-fov.GetViewAngle() / 2, false);
            Vector3 viewAngleB = fov.DirectionFromAngle(fov.GetViewAngle() / 2, false);

            Handles.color = Color.green;
            Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.GetViewRadius());
            Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.GetViewRadius());

        }
    }

#endif
}