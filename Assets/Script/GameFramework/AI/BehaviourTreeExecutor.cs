using UnityEngine;

public class BehaviourTreeExecutor :MonoBehaviour
{
    BehaviourTree tree;

    private void Start()
    {
        tree = ScriptableObject.CreateInstance<BehaviourTree>();

        var log = ScriptableObject.CreateInstance<DebugLogNode>();

        log.message = "Hello";

        var loop = ScriptableObject.CreateInstance<RepeatNode>();

        loop.child = log;

        tree.rootNode = loop;
    }

    private void Update()
    {
        tree.Update();
    }
}