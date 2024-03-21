using System.Collections.Generic;
using UnityEngine;

public abstract class CompositeNode : Node
{
    [HideInInspector] public List<Node> children = new();

    public override Node Clone()
    {
        CompositeNode node = Instantiate(this);
        node.children = children.ConvertAll(c => c.Clone());
        return node;
    }
}