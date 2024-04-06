using TheKiwiCoder;

//Purpose : This just checks if the AI is dead.
[System.Serializable]
public class IsAlive : Sequencer
{
    private BaseCharacter m_ThisCharacter;

    protected override void OnStart()
    {
        if (m_ThisCharacter == null)
            m_ThisCharacter = context.gameObject.GetComponent<BaseCharacter>();
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() 
    {
        base.OnUpdate();

        if (m_ThisCharacter.GetHealthComponent().IsDead)
            return State.Failure; //The AI is alive!

        //The AI is dead.
        return State.Success;
    }
}
