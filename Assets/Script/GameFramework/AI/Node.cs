using System;
using UnityEngine;

public abstract class Node : ScriptableObject
{
    public enum State
    {
        Running,
        Failure,
        Success
    }

    //These are to be hidden from the designer.
    [HideInInspector] public State state = State.Running;
    [HideInInspector] public bool started = false;
    [HideInInspector] public string guid; //Asset Identification.
    [HideInInspector] public Vector2 position;

    public State Update()
    {
        if (!started)
        {
            OnStart();
            started = true;
        }

        state = OnUpdate();

        if (state == State.Failure || state == State.Success)
        {
            OnStop();
            started = false;
        }

        return state;
    }

    public virtual Node Clone() => Instantiate(this);

    protected abstract void OnStart();
    protected abstract void OnStop();
    protected abstract State OnUpdate();
}