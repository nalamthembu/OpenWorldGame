using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Behaviour Tree", menuName = "Game/AI/Behaviour Tree")]
public class BehaviourTree : ScriptableObject
{
    public Node rootNode; //entry point
    public Node.State treeState = Node.State.Running; //state of the entire tree
    public List<Node> nodes = new();

    public Node.State Update()
    {
        if (rootNode.state == Node.State.Running)
        {
            return rootNode.Update();
        }

        //TODO : Reset function?

        return treeState;
    }

    public Node CreateNode(System.Type type)
    {
        Node node = CreateInstance(type) as Node;
        node.name = type.Name;
        node.guid = GUID.Generate().ToString();
        nodes.Add(node);

        AssetDatabase.AddObjectToAsset(node, this);
        AssetDatabase.SaveAssets();
        return node;
    }

    public void DeleteNode(Node node)
    {
        nodes.Remove(node);
        AssetDatabase.RemoveObjectFromAsset(node);
        AssetDatabase.SaveAssets();
    }

    public void AddChild(Node parent, Node child)
    {
        if (parent is DecoratorNode decorator)
            decorator.child = child;

        if (parent is RootNode root)
            root.child = child;

        if (parent is CompositeNode compositeNode)
            compositeNode.children.Add(child);
    }

    public void RemoveChild(Node parent, Node child)
    {
        if (parent is DecoratorNode decorator)
            decorator.child = null;

        if (parent is RootNode root)
            root.child = null;

        if (parent is CompositeNode compositeNode)
            compositeNode.children.Remove(child);
    }

    public List<Node> GetChildren(Node parent)
    {
        List<Node> children = new();

        if (parent is DecoratorNode decorator && decorator.child != null)
            children.Add(decorator.child);

        if (parent is RootNode root && root.child != null)
            children.Add(root.child);

        if (parent is CompositeNode compositeNode)
            return compositeNode.children;

        return children;
    }

    public BehaviourTree Clone()
    {
        BehaviourTree tree = Instantiate(this);
        tree.rootNode = tree.rootNode.Clone();
        return tree;
    }
}