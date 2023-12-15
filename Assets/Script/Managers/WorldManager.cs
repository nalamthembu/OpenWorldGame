using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public const int TWENTY_FOUR_MINUTES = 1440; //Seconds

    [SerializeField] TimeOfDay timeOfDay;

    [SerializeField] WorldDebug debug;

    private void Start()
    {
        debug.Start();
    }

    private void Update()
    {
        timeOfDay.Update();

        if (debug.enabled)
            debug.Update();
    }

    private void OnValidate()
    {
        debug.OnValidate();
    }
}

[System.Serializable]
public struct TimeOfDay
{
    public float currentTimeOfDay;

    public int daysPassed;

    public DayOfTheWeek currentDay;

    public void Update()
    {
        if (GameStateMachine.Instance.currentState is GameStatePaused)
            return;

        currentTimeOfDay += Time.deltaTime;

        if (currentTimeOfDay >= WorldManager.TWENTY_FOUR_MINUTES)
        {
            currentTimeOfDay = 0;
            currentDay++;
            daysPassed++;

            if ((int)currentDay >= 7 - 1)
            {
                currentDay = 0;
            }
        }
    }
}

public enum DayOfTheWeek
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
}


[System.Serializable]
public struct WorldDebug
{
    public bool enabled;

    private float timeScale;

    public float maxTimeScale;

    public float minTimeScale;

    public void OnValidate()
    {
        if (minTimeScale < 0)
        {
            minTimeScale = 0;
        }

        if (maxTimeScale < minTimeScale)
        {
            maxTimeScale = minTimeScale * 2;
        }
    }

    public void Start()
    {
        timeScale = 1;
    }

    public void Update()
    {
        HandleTimeDebugging();
    }

    public void HandleTimeDebugging()
    {
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            timeScale -= 1;

            if (timeScale < minTimeScale)
            {
                Debug.Log("Min Time Speed Reached (x " + minTimeScale + ")");

                timeScale = minTimeScale;
            }
        }

        if (Input.GetKey(KeyCode.Alpha8))
        {
            timeScale -= 0.25F;

            if (timeScale < minTimeScale)
            {
                Debug.Log("Min Time Speed Reached (x " + minTimeScale + ")");

                timeScale = minTimeScale;
            }
        }

        if (Input.GetKey(KeyCode.Alpha0))
        {
            timeScale += 0.25F;

            if (timeScale > maxTimeScale)
            {
                Debug.Log("Min Time Speed Reached (x " + maxTimeScale + ")");

                timeScale = maxTimeScale;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            timeScale += 1;

            if (timeScale > maxTimeScale)
            {
                Debug.Log("Min Time Speed Reached (x " + maxTimeScale + ")");

                timeScale = maxTimeScale;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            timeScale = 1;

            Debug.Log("Time Speed set to Normal");
        }

        Time.timeScale = Mathf.Lerp(Time.timeScale, timeScale, Time.deltaTime);

    }
}