using UnityEngine;

public class WaitNode : ActionNode
{
    public float duration = 1.0F;
    public float startTime;

    protected override void OnStart() => startTime = Time.time;

    protected override void OnStop()
    {

    }

    protected override State OnUpdate()
    {
        if (Time.time - startTime > duration)
            return State.Success;
        else
            return State.Running;
    }
}