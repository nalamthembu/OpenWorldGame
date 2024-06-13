using UnityEngine;
using UnityEngine.AI;

namespace OWFramework.AI
{
    public enum AIMood
    {
        Relaxed,
        Alert,
        Angry,
        Scared,
        Fearless
    };

    public enum NPCType
    {
        Civvie,
        Hostile,
    };

    public class BaseAIStateMachine : BaseCharacterStateMachine
    {
        #region Private Properties
        protected BaseAIState m_CurrentState;
        protected BlackBoardComponent m_Blackboard = new();
        [SerializeField] protected AIMood m_Mood;
        [SerializeField] protected NPCType m_Type;
        [SerializeField] protected float m_RunSpeed;
        [SerializeField] protected float m_WalkSpeed;
        [SerializeField] LayerMask m_VisibleLayers;
        [SerializeField] protected bool ShouldBeLookingForPlayer;
        private AIController m_Controller;
        protected AIPerceptionSensor m_Sensor;
        public AIPerceptionSensor PerceptionSensor { get { return m_Sensor; } }
        public NPCType NPCType { get { return m_Type; } }
        public Vector3 DangerLocation { get; set; }
        #endregion

        #region Debugging

        [SerializeField] bool m_DebugShowLineOfSight;
        [SerializeField] bool m_DebugFollowTarget;
        [SerializeField] bool m_DebugAimAtTarget;

        //Debug 
        [SerializeField] bool m_DebugShooting;
        [SerializeField] Vector2 m_ShootingDurationValues = new(1, 5);
        [SerializeField] Vector2 m_ShootingCooldownValues = new(1, 5);
        public bool FireAtTarget { get; set; } = false;
        float m_ShootTimer;
        float m_ShootCooldownTimer;
        float m_ShootingCooldownLimit = 1;
        float m_ShootingTimeLimit = 1;

        #endregion

        #region Public Properties
        public bool ShouldLookForPlayer { get; protected set; }
        public float WalkSpeed { get { return m_WalkSpeed; } }
        public float RunSpeed { get { return m_RunSpeed; } }
        public BlackBoardComponent Blackboard { get {  return m_Blackboard; } }
        public AIMood GetMood() => m_Mood;
        public void SetMood(AIMood new_mood) => m_Mood = new_mood;
        public NavMeshAgent Agent { get; protected set; }
        public void SetShouldLookoutForPlayer(bool status) => ShouldLookForPlayer = status;
        #endregion

        public void GoToPosition(Vector3 position)
        {
            if (Agent != null)
                Agent.SetDestination(position);
        }

        protected override void Awake()
        {
            base.Awake();
            Agent = GetComponent<NavMeshAgent>();
            m_Animator = GetComponent<Animator>();
            m_Controller = GetComponent<AIController>();
            m_Sensor = GetComponent<AIPerceptionSensor>();
        }
        
        protected virtual void Start() => InitialiseBlackboard();

        protected virtual void InitialiseBlackboard()
        {
            // Get Player Position
            m_Blackboard.SetValue("m_PlayerTransform", PlayerCharacter.Instance.transform);

            // Player Last Known Position
            m_Blackboard.SetValue("m_LastKnowPlayerPosition", Vector3.zero);
        }

        public virtual void SwitchState(AIStateEnum newState) { }

        public bool GetCanSeePlayer(out Vector3 lastKnownPosition)
        {
            if (PlayerCharacter.Instance == null)
            {
                lastKnownPosition = transform.position;
                return false;
            }

            Transform playerTransform = PlayerCharacter.Instance.transform;

            Vector3 origin = transform.position + transform.up * 0.5f;

            Vector3 target = playerTransform.position + playerTransform.up * 0.5F;

            bool linecastTest = Physics.Linecast(origin, target, out var hitInfo, m_VisibleLayers, QueryTriggerInteraction.Ignore);

            if (m_DebugShowLineOfSight)
                Debug.DrawLine(origin, hitInfo.point, Color.yellow);

            lastKnownPosition = hitInfo.point;

            return linecastTest;
        }

        protected override void Update()
        {
            base.Update();

            // Update the current state and always check if we need to switch states. 
            if (m_CurrentState != null)
            {
                m_CurrentState.OnUpdate(this);
                m_CurrentState.OnCheckTransition(this);

                // Determine Speed Based on Mood
                Agent.speed = GetMood() switch
                {
                    AIMood.Relaxed => WalkSpeed, // No worries, man.
                    AIMood.Fearless => RunSpeed, // Fight 
                    AIMood.Scared => RunSpeed, // Flight
                    AIMood.Alert => RunSpeed / 2, // Just go for a light jog.
                    _ => WalkSpeed
                };
            }
        }

        protected virtual void OnDrawGizmos() => m_CurrentState?.OnDrawGizmos();

        protected virtual void OnAnimatorIK() => m_CurrentState?.OnAnimateIK();

        protected override void SetAnimationValues()
        {
            base.SetAnimationValues();

            float agentSpeed = Agent.velocity.magnitude;
            m_Animator.SetFloat(GameStrings.INPUT_MAGNITUDE, Agent.velocity.normalized.magnitude);
            m_Animator.SetFloat(GameStrings.SPEED, agentSpeed, 0.25F, Time.deltaTime);
            m_Animator.SetBool(GameStrings.LU, agentSpeed <= 0);
            m_Animator.SetBool(GameStrings.IS_GROUNDED, Character.IsGrounded());
            m_Animator.SetFloat(GameStrings.INPUT_X, m_Controller.GetInputDirection().x, 0.25F, Time.deltaTime);
            m_Animator.SetFloat(GameStrings.INPUT_Y, m_Controller.GetInputDirection().y, 0.25F, Time.deltaTime);

            float Rotation = Mathf.Atan2(m_Controller.GetInputDirection().x, m_Controller.GetInputDirection().y);
            m_Animator.SetFloat(GameStrings.TARGET_ROTATION, Rotation, 0.25F, Time.deltaTime);

            if (m_WeaponHandler)
            {
                Gun equippedWeapon = m_WeaponHandler.GetEquippedWeapon();
                GunData equippedWeaponGunData = null;

                if (equippedWeapon)
                {
                    equippedWeaponGunData = (GunData)equippedWeapon.WeaponData;
                }

                // TODO : Refine is Aiming

                bool isAiming = equippedWeapon != null;

                m_Character.IsAiming = isAiming;

                // TODO : Is Firing

                if (m_DebugShooting || FireAtTarget)
                {
                    if (m_ShootTimer < m_ShootingTimeLimit)
                    {
                        m_ShootTimer += Time.deltaTime;
                    }
                    else if (m_ShootCooldownTimer < m_ShootingCooldownLimit)
                    {
                        m_ShootCooldownTimer += Time.deltaTime;
                    }
                    else
                    {
                        m_ShootTimer = 0;
                        m_ShootingTimeLimit = Random.Range(m_ShootingDurationValues.x, m_ShootingDurationValues.y);
                        m_ShootCooldownTimer = 0;
                        m_ShootingCooldownLimit = Random.Range(m_ShootingCooldownValues.x, m_ShootingCooldownValues.y);
                    }

                    m_Character.IsFiring = m_ShootTimer < m_ShootingTimeLimit;
                }

                m_Animator.SetBool(GameStrings.IS_RIFLE, equippedWeapon != null && equippedWeaponGunData.WeaponClass == WeaponClassification.Rifle);
                m_Animator.SetBool(GameStrings.IS_PISTOL, equippedWeapon != null && equippedWeaponGunData.WeaponClass == WeaponClassification.Pistol);
                m_Animator.SetBool(GameStrings.IS_AIMING, m_Character.IsAiming);
                m_Animator.SetBool(GameStrings.IS_FIRING, m_Character.IsFiring);
            }
        }

        public void SetNavigationEnabled(bool enable) => Agent.isStopped = !enable;
    }
}