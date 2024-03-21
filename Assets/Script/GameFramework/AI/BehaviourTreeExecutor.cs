using UnityEngine;

public class BehaviourTreeExecutor :MonoBehaviour
{
    [SerializeField] BehaviourTree tree;

    private void Start() => InitialiseBehaviourTree();

    private void InitialiseBehaviourTree ()
    {
        //SETUP

        tree = tree.Clone();
    }

    private void Update()
    {
        tree.Update();
    }
}