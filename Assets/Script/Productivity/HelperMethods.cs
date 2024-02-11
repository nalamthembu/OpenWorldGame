using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

public static class HelperMethods
{
    #region NUMERICTYPE_FLOAT
    public static string GetFloatStopWatchFormat(this float value)
    {
        TimeSpan t = TimeSpan.FromSeconds(value);

        var sb = new StringBuilder();

        return sb.Append(string.Format
            (
                "{0:00}:{1:00}:{2:000}",
                 t.Minutes,
                 t.Seconds,
                 Mathf.FloorToInt(t.Milliseconds) / 10f
            )).ToString();

    }

    public static string GetFloat24HourTime(this float value)
    {
        TimeSpan t = TimeSpan.FromSeconds(value);

        var sb = new StringBuilder();

        return sb.Append(string.Format
            (
                "{0:00}:{1:00}",
                 t.Minutes,
                 t.Seconds
            )).ToString();
    }
    #endregion

    #region NUMERICTYPE_ANIMATOR
    public static float GetCurrentAnimatorTime(this Animator targetAnim, int layer = 0)
    {
        AnimatorStateInfo animState = targetAnim.GetCurrentAnimatorStateInfo(layer);
        float currentTime = animState.normalizedTime % 1;
        return currentTime;
    }

    public static bool IsCurrentStateTag(this Animator targetAnim, string tag, int layer = 0)
    {
        AnimatorStateInfo animState = targetAnim.GetCurrentAnimatorStateInfo(layer);
        return animState.IsTag(tag);
    }

    public static string GetCurrentAnimStateNames(this Animator targetAnim, int layer = 0)
    {
        string names = String.Empty;

        for (int i = 0; i < names.Length; i++)
        {
            names += targetAnim.GetCurrentAnimatorClipInfo(0)[i].clip.name + "\n";
        }

        return names;
    }

    public static bool IsCurrentStateName(this Animator targetAnim, string name, int layer = 0)
    {
        AnimatorStateInfo animState = targetAnim.GetCurrentAnimatorStateInfo(layer);
        return animState.IsName(name);
    }

    public static void SetAnimatorSpeed(this Animator targetAnim, float speed)
    {
        targetAnim.speed = speed;
    }

    #endregion

    #region OBJECTS

    public static float SqrDistance(this System.Object _, Vector3 a, Vector3 b) 
        =>  Mathf.Abs((a - b).sqrMagnitude);
    
    #endregion
}