using OWFramework.AI;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.AI;

namespace OWFramework
{
    public enum AIStateEnum
    {
        Idle,
        Wandering,
        Chase,
        Follow,
        AimAtTarget,
        FireAtTarget
    };

    // Idle State (Stand around and do nothing)

    [System.Serializable]
    public class AIIdle : BaseAIState
    {
        [SerializeField] Vector2 IdleInterval = new(5, 10);
        float calculatedInterval;
        float idleTimer = 0;

        public override void OnCheckTransition(BaseAIStateMachine machine)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= calculatedInterval)
            {
                idleTimer = 0;
                calculatedInterval = CalculateInterval();

                //Switch to Wandering State
                machine.SwitchState(AIStateEnum.Wandering);
            }
        }
        public override void OnEnter(BaseAIStateMachine machine)
        {
            calculatedInterval = CalculateInterval();

            // Stop the character from moving.
            machine.SetNavigationEnabled(false);

        }
        public override void OnExit(BaseAIStateMachine machine)
        {
            // Enable the character to move.
            machine.SetNavigationEnabled(true);
        }
        private float CalculateInterval() => Random.Range(IdleInterval.x, IdleInterval.y);
        public override void OnUpdate(BaseAIStateMachine machine) { }
    }

    // Wander State (Walk/Run around aimlessly)

    [System.Serializable]
    public class AIWander : BaseAIState
    {
        [SerializeField] float maxDistance;
        [SerializeField] float minDistanceFromTarget;
        [SerializeField] float WanderDuration;

        //Private Members
        Vector3     calculatedRandomPosition;
        Vector3     centre; // This is used during random point calculations.
        Transform   aiTransform; // This is the AI Attached to this State Machine.
        float       wanderTimer;

        public override void OnCheckTransition(BaseAIStateMachine machine)
        {
            wanderTimer += Time.deltaTime;

            if (wanderTimer >= WanderDuration)
            {
                wanderTimer = 0;
                machine.SwitchState(AIStateEnum.Idle);
            }
        }

        public override void OnExit(BaseAIStateMachine machine)
        { }

        public override void OnEnter(BaseAIStateMachine machine)
        {
            if (aiTransform == null)
                aiTransform = machine.transform;

            centre = machine.Agent.transform.position;

            // Enable Movement
            machine.SetNavigationEnabled(true);

            // Calculate New Destination
            CalculateRandomPosition();
            GoToCalculatedPosition(machine.Agent);
        }

        public override void OnUpdate(BaseAIStateMachine machine)
        {
            if (machine.Agent == null) return;

            // Determine Speed Based on Mood
            machine.Agent.speed = machine.GetMood() switch
            {
                AIMood.Relaxed => machine.WalkSpeed, // No worries, man.
                AIMood.Fearless => machine.RunSpeed, // Fight 
                AIMood.Scared => machine.RunSpeed, // Flight
                AIMood.Alert => machine.RunSpeed / 2, // Just go for a light jog.
                _ => machine.WalkSpeed
            };

            // Keep finding a new random location to go to.
            if (machine.Agent.remainingDistance <= minDistanceFromTarget || machine.Agent.destination == null)
            {
                CalculateRandomPosition();
                GoToCalculatedPosition(machine.Agent);
            }
        }

        public void SetRandomParameters(Vector3 centre, float maxDistance)
        {
            this.centre = centre;
            this.maxDistance = maxDistance;
        }

        private void CalculateRandomPosition()
        {
            int maxIterations = 100;

            for (int i = 0; i < maxIterations; i++)
            {
                calculatedRandomPosition = centre + (Vector3)Random.insideUnitCircle * maxDistance;

                // Check if its on a Nav Mesh.
                if (NavMesh.SamplePosition(calculatedRandomPosition, out var hit, maxDistance, NavMesh.AllAreas))
                    calculatedRandomPosition = hit.position;
            }
        }

        private void GoToCalculatedPosition(NavMeshAgent agent)
        {
            if (agent != null)
            {
                agent.SetDestination(calculatedRandomPosition);
            }
        }

        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(calculatedRandomPosition, 10);
        }
    }

    // Chase State (Target is running)

    [System.Serializable]
    public class AIChase : BaseAIState
    {
        [SerializeField] bool   m_ShouldBeRunning;
        [SerializeField] bool   m_SearchIfLostTarget;
        [SerializeField] bool   m_ShouldAnnounceTargetLocation;
        [SerializeField] bool   m_ShootAtTarget;
        [SerializeField] float  m_IdealDistance; // How close should I be before switching states?
        float m_DistFromTarget;

        #region DEBUG_OPTIONS
        [SerializeField] bool m_DebugSwitchToIdle;
        #endregion

        public override void OnCheckTransition(BaseAIStateMachine machine)
        {
            if (!machine.GetCanSeePlayer(out var lastKnownPosition))
            {
                machine.Blackboard.SetValue(GameStrings.LAST_KNOWN_PLAYER_POSITION, lastKnownPosition);

                // React
                machine.CharacterSpeech.DoReaction(ReactionType.LostTarget);

                // Should I search?
                if (m_SearchIfLostTarget)
                {
                    // TODO : Switch to Search state.

                    // DEBUG

                    if (m_DebugSwitchToIdle)
                        machine.SwitchState(AIStateEnum.Idle);
                }
            }

            m_DistFromTarget = Vector3.Distance(machine.transform.position, lastKnownPosition);
            if (m_ShootAtTarget && m_DistFromTarget <= m_IdealDistance)
                machine.SwitchState(AIStateEnum.AimAtTarget);
        }

        public override void OnEnter(BaseAIStateMachine machine)
        {
            // Check if I should announce the targets location?

            Debug.Log("Chasing Target!");

            if (m_ShouldAnnounceTargetLocation)
            {
                // TODO : Play Speech (There he is!)

                // TODO : Send out event to alert others.
            }

            // Should I run to the player?
            machine.Agent.speed = m_ShouldBeRunning ? machine.RunSpeed : machine.WalkSpeed;
        }

        public override void OnExit(BaseAIStateMachine machine)
        { }

        public override void OnUpdate(BaseAIStateMachine machine)
        {
            if (!machine.GetCanSeePlayer(out var lastKnownPosition))
                return;

            machine.Agent.SetDestination(lastKnownPosition);

        }
    }

    // Follow State (Follow the target, NPC behaviour)

    [System.Serializable]
    public class AIFollow : BaseAIState
    {
        [SerializeField] Transform  m_TargetTransform;
        [SerializeField] float      m_MinDistance; // How close should I be, ideally?
        [SerializeField] float      m_MaxDistance; // How far am I allowed to let the target be?
        [SerializeField] bool       m_LookAtTarget; // Should I look at the target?
        private BaseCharacter       m_TargetCharacter;
        public bool GetShouldLookAtTarget() => m_LookAtTarget;
        public Transform GetTargetTransform() => m_TargetTransform;
        private bool m_TargetIsCharacter;

        public bool IsTargetACharacter(out BaseCharacter Character)
        {
            Character = m_TargetIsCharacter ? m_TargetCharacter : null;

            return m_TargetIsCharacter;
        }

        public override void OnCheckTransition(BaseAIStateMachine machine)
        { }

        public override void OnEnter(BaseAIStateMachine machine)
        {
            if (machine.Agent)
                machine.Agent.stoppingDistance = m_MinDistance;

            if (m_TargetTransform != null)
            {
                m_TargetCharacter = m_TargetTransform.GetComponent<BaseCharacter>();

                m_TargetIsCharacter = m_TargetCharacter != null;
            }
        }

        public override void OnExit(BaseAIStateMachine machine)
        { }

        public override void OnUpdate(BaseAIStateMachine machine)
        {
            machine.Agent.SetDestination(m_TargetTransform.position);

            float sqrDist = Mathf.Abs((m_TargetTransform.position - machine.transform.position).sqrMagnitude);

            machine.Agent.speed = sqrDist >= m_MaxDistance * m_MaxDistance ? machine.RunSpeed : machine.WalkSpeed;
        }
    }

    // Search State (Go to the last known player position, look around the area for a set time, if you find nothing, give up and Wander)

    // Investigate State (Investigate a noise or something you saw)

    // Find Cover (Caused by High Danger, like getting shot at)

    // Run and Gun (Trait of a fearless AI)

    // Fire At Target - WHEN SHOT AT OR AIMED AT (Blind fire if Danger is high, if not in cover should where you are, If you get shot at, Find Cover)
    // Aim At Target (Look at Target but don't shoot at them, useful for cops attempting to arrest someone)
    [System.Serializable]
    public class AIAimAtTarget : BaseAIState
    {
        [SerializeField] Transform  m_TargetTransform;
        [SerializeField] GameObject m_WeaponPrefab;
        [SerializeField] float      m_MinDistance; // How close should I be, ideally?
        [SerializeField] float      m_MaxDistance; // How far am I allowed to let the target be?
        [SerializeField][Min(0.1F)] float m_SpeedDivider = 2; // How much slower should we be when aiming?

        float m_SqrDistanceFromTarget;

        private Weapon m_WeaponInstance;
        private AIAimingComponent m_AIAimingComponent;
        private BaseCharacter m_TargetCharacter;
        private BaseCharacterWeaponHandler m_TargetCharacterWeaponHandler;

        public void SetTargetCharacter(BaseCharacter targetCharacter)
        {
            m_TargetCharacter = targetCharacter;

            m_TargetCharacterWeaponHandler = m_TargetCharacter.GetComponent<BaseCharacterWeaponHandler>();

            //Aim for the biggest spot on the character (ie. Chest - (CONNECTED TO THE SPINE BONE))

            m_TargetTransform = m_TargetCharacter.Animator.GetBoneTransform(HumanBodyBones.Spine);
        }

        public override void OnCheckTransition(BaseAIStateMachine machine)
        {
            if (machine.CharacterSpeech && TargetIsFiringAtMe())
            {
                machine.CharacterSpeech.DoReaction(ReactionType.TakeCover);

                //Switch to Find Cover State.
            }

            //If they're too far
            if (Vector3.Distance(machine.transform.position, m_TargetTransform.position) >= m_MaxDistance)
            {
                // React
                machine.CharacterSpeech.DoReaction(ReactionType.Chase);

                // Chase them
                machine.SwitchState(AIStateEnum.Chase);
            }
        }

        public override void OnEnter(BaseAIStateMachine machine)
        {
            m_AIAimingComponent = machine.GetComponent<AIAimingComponent>();

            //Lets check if the assigned target is a character
            if (m_TargetTransform.root.TryGetComponent<BaseCharacter>(out var characterComp))
                SetTargetCharacter(characterComp);


            if (machine.Agent)
                machine.Agent.stoppingDistance = m_MinDistance;

            if (machine.WeaponHandler)
            {
                if (!machine.WeaponHandler.HasAnyWeapon())
                {
                    machine.WeaponHandler.AddWeapon(m_WeaponInstance = Object.Instantiate(m_WeaponPrefab).GetComponent<Weapon>());

                    m_WeaponInstance.DoGenericCharacterPickup(machine.Character);

                    machine.WeaponHandler.EquipFirstAvailableWeapon();
                }
                else
                {
                    machine.WeaponHandler.EquipFirstAvailableWeapon();
                }
            }
        }

        public override void OnExit(BaseAIStateMachine machine)
        { }


        public override void OnUpdate(BaseAIStateMachine machine)
        {
            // Follow target

            machine.Agent.SetDestination(m_TargetTransform.position);

            m_SqrDistanceFromTarget = Mathf.Abs((m_TargetTransform.position - machine.transform.position).sqrMagnitude);

            // Make sure we never divide by 0
            if (m_SpeedDivider == 0)
                m_SpeedDivider = 1.0F;

            machine.Agent.speed = m_SqrDistanceFromTarget >= m_MaxDistance * m_MaxDistance ? machine.RunSpeed / m_SpeedDivider : machine.WalkSpeed / m_SpeedDivider;

            Vector3 direction = (m_TargetTransform.position - machine.transform.position).normalized;

            // Calculate the rotation without pitch and roll
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            // Create a new rotation with the same yaw angle but no pitch or roll
            Quaternion finalRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);

            // Apply the rotation to the machine's transform
            machine.transform.rotation = finalRotation;

            // Aim at target
            m_AIAimingComponent.SetTargetPosition(m_TargetTransform.position);

            // Animate & Camera XY
            machine.Animator.SetFloat(GameStrings.CAMERA_X, m_AIAimingComponent.GetTargetRotation().x, 0.1F, Time.deltaTime);
            machine.Animator.SetFloat(GameStrings.CAMERA_Y, m_AIAimingComponent.GetTargetRotation().y, 0.1F, Time.deltaTime);

            // Shoot at the target if they threaten you.
            if (ThereIsAThreat(machine.transform) && !machine.FireAtTarget)
            {
                machine.CharacterSpeech.DoReaction(ReactionType.DrawGun);
                machine.FireAtTarget = true;
            }
        }

        private bool TargetIsFiringAtMe() => m_TargetCharacter && m_TargetCharacter.IsFiring;

        private bool ThereIsAThreat(Transform myTransform)
        {
            Gun characterGun = m_TargetCharacterWeaponHandler.GetEquippedWeapon();
            bool targetHasAWeaponOut = m_TargetCharacter != null && m_TargetCharacterWeaponHandler != null && characterGun != null;
            bool targetIsAimingAtMe = m_TargetCharacter.IsAiming &&
                Physics.Linecast(characterGun.transform.position,
                myTransform.position + myTransform.up * 0.5F,
                characterGun.GetBulletInteractionLayers,
                QueryTriggerInteraction.Ignore);

            if (targetHasAWeaponOut && targetIsAimingAtMe)
                return true;

            return false;
        }
    }

    // Conversation State (Talk to another AI)

    // Flee (No! F##K this, I'm out of here!)
}