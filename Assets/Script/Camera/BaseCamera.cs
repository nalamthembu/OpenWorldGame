using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public abstract class BaseCamera
{
    [SerializeField] protected List<Transform> m_Targets;
    
    [SerializeField] protected Camera m_AttachedCamera;

    [SerializeField] [Range(0.1F, 5)] float m_FOVSmoothTime = 0.25F;

    public bool m_IsActive;

    protected float m_FOV;

    protected float m_FOVSmoothVel;

    protected Transform transform;

    protected float m_Pitch;

    protected float m_Yaw;

    public virtual void Initialise(Transform selfTransform, float initialFOV)
    {
        transform = selfTransform;
        m_FOV = initialFOV;
    }


    protected virtual void DoUpdateFOV()
    {
        m_AttachedCamera.fieldOfView = Mathf.SmoothDamp(m_AttachedCamera.fieldOfView, m_FOV, ref m_FOVSmoothVel, m_FOVSmoothTime);
    }

    public virtual void Update()
    {
        DoUpdateFOV();
    }

    protected Vector3 GetCentrePoint()
    {
        if (m_Targets.Count == 1)
            return m_Targets[0].position;

        var bounds = new Bounds(m_Targets[0].position, Vector3.zero);

        for (int i = 0; i < m_Targets.Count; i++)
        {
            bounds.Encapsulate(m_Targets[i].position);
        }

        return bounds.center;
    }

    protected float GetGreatestDistance()
    {
        var bounds = new Bounds(m_Targets[0].position, Vector3.zero);

        for (int i = 0; i < m_Targets.Count; i++)
        {
            bounds.Encapsulate(m_Targets[i].position);
        }

        return bounds.size.x;
    }

    public virtual void LateUpdate()
    {
        DoUpdateRotation();
        DoUpdatePosition();
        DoUpdateSpeed();
    }

    public abstract void DoUpdatePosition();
    public abstract void DoUpdateRotation();
    protected virtual void DoUpdateSpeed() { }
}