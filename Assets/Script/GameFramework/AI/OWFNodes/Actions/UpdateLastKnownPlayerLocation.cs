using UnityEngine;
using TheKiwiCoder;

[System.Serializable]
public class UpdateLastKnownPlayerLocation : ActionNode
{
    [Tooltip("Player Location Property")]
    public NodeProperty<Vector3> m_LastKnownPosition = new NodeProperty<Vector3> { defaultValue = Vector3.zero };

    private string m_ValueReference;

    protected override void OnStart() { m_ValueReference = m_LastKnownPosition.reference.name; }

    protected override void OnStop() { }

    protected override State OnUpdate()
    {
        if (PlayerCharacter.Instance != null)
        {
            Vector3 playerPos = PlayerCharacter.Instance.transform.position + PlayerCharacter.Instance.transform.up * 1.5f;

            blackboard.SetValue(m_ValueReference, new Vector3()
            {
                x = playerPos.x,
                y = 0,
                z = playerPos.z
            });

            return State.Success;
        }

        return State.Failure;
    }
}
