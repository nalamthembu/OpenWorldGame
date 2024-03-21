using UnityEngine;

[CreateAssetMenu()]
public class BehaviourTree : ScriptableObject
{
    public Node rootNode; //entry point
    public Node.State treeState = Node.State.Running; //state of the entire tree

    public Node.State Update()
    {
        if (rootNode.state == Node.State.Running)
        {
            return rootNode.Update();
        }

        //TODO : Reset function?

        return treeState;
    }
}