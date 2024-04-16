using UnityEngine;

public class AIAimingComponent : MonoBehaviour
{
    [Header("Debug Target")]
    [SerializeField] Transform m_DebugTarget;
    [SerializeField][Range(0, 100)] float m_AimAccuracy = 1.0f;
    private BaseCharacterWeaponHandler m_WeaponHandler;
    private Vector3 m_TargetPosition;
    [SerializeField] bool m_DebugHitpoint;

    private void Awake() => m_WeaponHandler = GetComponent<BaseCharacterWeaponHandler>();
    
    public void SetTargetPosition(Vector3 targetPosition) => m_TargetPosition = targetPosition;

    private void Update()
    {
        if (m_WeaponHandler != null && m_WeaponHandler.GetEquippedWeapon())
        {
            if (m_DebugTarget != null)
                m_TargetPosition = m_DebugTarget.position;

            m_WeaponHandler.GetEquippedWeapon().AITarget = 
                Vector3.Lerp(m_WeaponHandler.GetEquippedWeapon().AITarget, m_TargetPosition, Time.deltaTime * m_AimAccuracy);
        }
    }

    public Vector2 GetTargetRotation()
    {
        Vector3 currentPosition = transform.position; 
        Vector3 targetPosition = m_TargetPosition;

        // Calculate the direction vector from current position to target position
        Vector3 direction = targetPosition - currentPosition;

        // Calculate the angle between the direction vector and the forward vector
        float angle = Vector3.Angle(transform.forward, direction);

        // Calculate the cross product of forward vector and direction vector
        Vector3 cross = Vector3.Cross(transform.forward, direction);

        // Adjust the angle based on the cross product's y component
        if (cross.y < 0)
        {
            angle = -angle;
        }

        // Clamp the angle between -90 and 90 degrees
        angle = Mathf.Clamp(angle, -90f, 90f);

        // Convert the angle to radians
        float angleRad = angle * Mathf.Deg2Rad;

        // Calculate the X and Y components of the clamped angle
        float x = Mathf.Cos(angleRad);
        float y = Mathf.Sin(angleRad);

        return new Vector2(x, y);
    }

    private void OnDrawGizmos()
    {
        if (m_DebugHitpoint && m_WeaponHandler != null && m_WeaponHandler.GetEquippedWeapon())
        {
            // Debug Colour
            Gizmos.color = Color.yellow;

            // Debug Direction
            Gizmos.DrawWireSphere(m_WeaponHandler.GetEquippedWeapon().AITarget, 0.25F);
        }
    }
}