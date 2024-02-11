using System;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using static ObjectiveType;

public class MissionManager : MonoBehaviour
{
    [SerializeField] Objective[] m_Objectives;

    Objective m_CurrentObjective;

    private float m_CompletetionPercent;
    private float m_TimeElapsed;
    private bool m_MissionComplete;
    private int m_CurrentObjectiveIndex;
    private int m_CompletedObjectives;
    public static event Action OnMissionComplete;

    [SerializeField] bool m_DebugMission;
    [ConditionalField("m_DebugMission", false, true)] public int m_DebugObjectiveIndex = -1;


    private void Start()
    {
        if (InteractiveMusicManager.Instance)
            InteractiveMusicManager.Instance.StartSoundTrack();

        GetNextObjective();
    }

    private void OnEnable()
    {
        foreach(Objective objective in m_Objectives)
        {
            objective.OnEnable();
        }
    }

    private void OnDisable()
    {
        foreach (Objective objective in m_Objectives)
        {
            objective.OnDisable();
        }
    }

    private void Update()
    {
        if (m_MissionComplete)
            return;

        m_TimeElapsed += Time.deltaTime;

        EvaluateObjective();
    }

    private void EvaluateObjective()
    {
        if (m_CurrentObjective != null && m_CurrentObjective.IsComplete())
            GetNextObjective();
    }

    private void GetNextObjective()
    {
        m_CurrentObjectiveIndex++;

        if (m_CurrentObjectiveIndex >= m_Objectives.Length)
        {
            m_MissionComplete = true;
            Debug.Log("Mission Complete : " + m_TimeElapsed.GetFloatStopWatchFormat());
            if (InteractiveMusicManager.Instance)
                InteractiveMusicManager.Instance.StopSoundTrack();
            return;
        }
        else
        {
            m_CurrentObjective = m_Objectives[m_CurrentObjectiveIndex];

            if (InteractiveMusicManager.Instance)
                InteractiveMusicManager.Instance.SetIntensity(m_CurrentObjective.interactiveMusicIntensity);
        }
    }

    private void OnDrawGizmos()
    {
        if (m_CurrentObjective != null)
        {
            if (m_MissionComplete)
                return;

            Gizmos.color = Color.yellow;

            switch (m_CurrentObjective.objectiveType)
            {
                case GO_TO_DESTINATION:
                    if (m_CurrentObjective.TargetDestination != null)
                        Gizmos.DrawSphere(m_CurrentObjective.TargetDestination.position, 0.25F);

                    break;

                case GET_PICKUP:
                    if (m_CurrentObjective.TargetPickup != null)
                        Gizmos.DrawSphere(m_CurrentObjective.TargetPickup.transform.position, 0.25F);
                    break;

                case KILL_CHARACTER:
                    if (m_CurrentObjective.TargetCharacter)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(m_CurrentObjective.TargetCharacter.transform.position + Vector3.up * 1.8f, 0.25F);
                    }
                    break;

                case LEAVE_THE_AREA:
                    Gizmos.DrawWireSphere(m_CurrentObjective.AreaCentreTransform.position, m_CurrentObjective.areaRadius);
                    break;
            }
        }
    }
}


[Serializable]
public class Objective
{
    public ObjectiveType objectiveType;

    public MusicIntensity interactiveMusicIntensity;

    private bool m_IsComplete = false;

    [ConditionalField("objectiveType", false, GET_PICKUP)] public BasePickup TargetPickup;
    [ConditionalField("objectiveType", false, KILL_CHARACTER)] public BaseCharacter TargetCharacter;
    [ConditionalField("objectiveType", false, KILL_MULTIPLE_CHARACTERS)] public BaseCharacter[] TargetCharacters;
    [ConditionalField("objectiveType", false, GET_IN_VEHICLE)] public Vehicle TargetVehicle;
    [ConditionalField("objectiveType", false, GO_TO_DESTINATION)] public Transform TargetDestination;
    [ConditionalField("objectiveType", false, GO_TO_DESTINATION)] public float MinDistance;
    [ConditionalField("objectiveType", false, LEAVE_THE_AREA)] public Transform AreaCentreTransform;
    [ConditionalField("objectiveType", false, LEAVE_THE_AREA)] public float areaRadius;

    public void OnEnable()
    {
        BasePickup.OnPickUp += OnPickUpCollected;
    }

    public void OnDisable()
    {
        BasePickup.OnPickUp -= OnPickUpCollected;
    }

    private void OnPickUpCollected(BasePickup pickupInQuestion)
    {
        if (pickupInQuestion == TargetPickup)
            m_IsComplete = true;
    }

    public bool IsComplete()
    {
        if (PlayerCharacter.Instance == null)
        {
            Debug.LogError("There is no player character in the scene!");
            return false;
        }

        switch (objectiveType)
        {
            case GET_PICKUP:
                return m_IsComplete;

            case GET_IN_VEHICLE:
                if (TargetVehicle.Owner == PlayerCharacter.Instance)
                    return true;
                break;

            case GO_TO_DESTINATION:
                if (Vector3.Distance(PlayerCharacter.Instance.transform.position, TargetDestination.position) <= MinDistance)
                    return true;
                break;

            case KILL_CHARACTER:
                if (TargetCharacter.TryGetComponent<HealthComponent>(out var healthComponent) && healthComponent.IsDead)
                    return true;
                break;

            case KILL_MULTIPLE_CHARACTERS:

                int num_of_dead_characters = 0;

                foreach(BaseCharacter character in TargetCharacters)
                    if (character && character.TryGetComponent<HealthComponent>(out var healthComp))
                        num_of_dead_characters++;
   
                //all the characters are dead.
                if (num_of_dead_characters >= TargetCharacters.Length)
                    return true;

                break;

            case LEAVE_THE_AREA:
                if (PlayerCharacter.Instance && Vector3.Distance(PlayerCharacter.Instance.transform.position, AreaCentreTransform.position) >= areaRadius)
                    return true;
                break;
        }

        return false;
    }
}

public enum ObjectiveType
{
    GET_PICKUP,
    KILL_CHARACTER,
    GO_TO_DESTINATION,
    GET_IN_VEHICLE,
    LEAVE_THE_AREA,
    KILL_MULTIPLE_CHARACTERS
};