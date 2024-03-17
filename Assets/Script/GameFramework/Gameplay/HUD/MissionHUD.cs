using System.Collections;
using TMPro;
using UnityEngine;

public class MissionHUD : BaseHUD
{
    [Header("During Mission UI")]
    [SerializeField] private TMP_Text m_ObjectiveInstructions; //Below subtitles.
    [SerializeField] private GameObject m_ObjectivePanel;
    [SerializeField] private float m_DelayBeforeObjectiveClearScreen = 5.0F;

    [Header("Mission Passed UI")]
    [SerializeField] private TMP_Text m_MissionTitle;
    [SerializeField] private GameObject m_MissionPassedPanel;
    [SerializeField] private float m_DelayBeforeMissionPassedClearScreen = 5.0F;


    protected override void Awake()
    {
        base.Awake();

        m_MissionPassedPanel.SetActive(false);
        m_ObjectivePanel.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        MissionManager.OnNextObjective += ShowObjective;
        MissionManager.OnMissionComplete += OnMissionComplete;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        MissionManager.OnNextObjective -= ShowObjective;
        MissionManager.OnMissionComplete -= OnMissionComplete;
    }

    private void OnMissionComplete(string missionTitle)
    {
        m_ObjectivePanel.SetActive(false);
        m_MissionTitle.text = missionTitle;
        m_MissionPassedPanel.SetActive(true);
        StartCoroutine(ShowMissionPassedForCertainPeriodOfTime(m_DelayBeforeMissionPassedClearScreen));

        //todo : play mission passed chime
    }

    private void ShowObjective(string objectiveString) => StartCoroutine(ShowObjectForCertainPeriodOfTime(objectiveString, m_DelayBeforeObjectiveClearScreen));

    private IEnumerator ShowMissionPassedForCertainPeriodOfTime(float delay)
    {
        float timer = delay;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        m_MissionPassedPanel.SetActive(false);
    }

    private IEnumerator ShowObjectForCertainPeriodOfTime(string objective, float delay)
    {
        m_ObjectiveInstructions.text = objective;

        m_ObjectivePanel.SetActive(true);

        float timer = delay;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        m_ObjectivePanel.SetActive(false);
    }
}