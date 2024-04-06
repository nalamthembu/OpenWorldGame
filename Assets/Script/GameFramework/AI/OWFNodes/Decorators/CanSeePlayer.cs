using UnityEngine;
using TheKiwiCoder;

[System.Serializable]
public class CanSeePlayer : Parallel
{
    Vector3 startPos, playerPos;

    bool canSeePlayer;

    protected override void OnStart()
    {
        base.OnStart();

        Vector3 playerPos = PlayerCharacter.Instance.transform.position + PlayerCharacter.Instance.transform.up * 1.5f;
        Vector3 startPos = context.transform.position + context.transform.up * 1.5f;
        canSeePlayer = Physics.Linecast(startPos, playerPos);
    }

    protected override void OnStop() { base.OnStop();  }

    protected override State OnUpdate()
    {
        if (canSeePlayer)
        {
            base.OnUpdate();

            Debug.DrawLine(startPos, playerPos, Color.magenta);

            return State.Success;
        }

        return State.Failure;
    }
}