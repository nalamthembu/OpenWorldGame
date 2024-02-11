using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerCharacter : BaseCharacter
{
    [SerializeField][Range(0.1f, 5F)] float m_RotationSmoothTime = 0.5F;
    [SerializeField][Range(0.1f, 5F)] float m_SpeedSmoothTime = 0.5F;
    [SerializeField][Range(1, 10)] float m_JumpHeight = 2;

    public static PlayerCharacter Instance;

    private bool m_CameraIsInDebugMode = false;
    public float RunSpeed { get { return m_RunSpeed; } }
    public float WalkSpeed { get { return m_WalkSpeed; } }
    public float SpeedSmoothTime { get { return m_SpeedSmoothTime; } }
    public float RotationSmoothTime { get { return m_RotationSmoothTime; } }

    float rFRotationVelocity;

    public float RefRotationVelocity { get { return rFRotationVelocity; } }

    Vector2 m_InputDir;

    protected override void Awake()
    {
        base.Awake();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected override void Update()
    {
        if (m_CameraIsInDebugMode)
            return;

        base.Update();

        if (!IsGrounded())
        {
            //"Fall" - The Lich (Adventure Time)
            transform.position += Physics.gravity * Time.deltaTime;
        }
    }

    public bool IsGrounded()
    {
        Vector3 start = transform.position + Vector3.up * 0.1F;
        Vector3 end = transform.position + -transform.up * 0.2F;

        Debug.DrawLine(start, end, Color.green);

        return Physics.Linecast(start, end);
    }

    public void Jump()
    {
        if (IsGrounded())
        {
            float jumpVelocity = Mathf.Sqrt(-2 * Physics.gravity.y * m_JumpHeight);
            transform.position += jumpVelocity * Time.deltaTime * Vector3.up;
        }
    }

    public void DoRotation(float rotation, bool IsProne = false)
    {
        if (IsProne)
        {
            if (PlayerController.Instance)
            {
                //Don't rotate if we're trying to do a barrel roll which is (Jump + Left/Right).
                if (PlayerController.Instance.JumpPressed)
                    return;

                m_InputDir = PlayerController.Instance.PlayerMovement.normalized;
            }

            //If we're moving past the speed threshold and we aren't trying to move backwards
            if (m_Animator.velocity.magnitude >= m_WalkSpeed && m_InputDir.y > 0)
            {
                //rotate the player.
                transform.eulerAngles = Vector3.up *
                Mathf.SmoothDampAngle(transform.eulerAngles.y,
                rotation, ref rFRotationVelocity, m_RotationSmoothTime);
            }
        }
        else
        //If we're moving above a certain speed threshold, let the player input take over the rotation.
        if (m_Animator.velocity.magnitude >= m_WalkSpeed + 1F)
        {
            transform.eulerAngles = Vector3.up *
                Mathf.SmoothDampAngle(transform.eulerAngles.y,
                rotation, ref rFRotationVelocity, m_RotationSmoothTime);
        }
        else  //ROTATE 80% faster WHEN AIMING.
        if (PlayerController.Instance &&
            PlayerController.Instance.IsAiming &&
            PlayerWeaponHandler.Instance &&
            PlayerWeaponHandler.Instance.GetEquippedWeapon())
        {
            transform.eulerAngles = Vector3.up *
                Mathf.SmoothDampAngle(transform.eulerAngles.y,
                rotation, ref rFRotationVelocity, m_RotationSmoothTime * 0.2F);
        }
    }
}
