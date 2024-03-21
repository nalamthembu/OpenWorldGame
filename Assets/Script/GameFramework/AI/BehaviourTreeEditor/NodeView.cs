using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public Node node;
    public Port input;
    public Port output;
    
    //Events
    public Action<NodeView> OnNodeSelected;

    public NodeView(Node node)
    {
        this.node = node;
        title = node.name;
        viewDataKey = node.guid;

        //Styling (sets position)
        style.left = node.position.x;
        style.top = node.position.y;

        CreateInputPorts();
        CreateOutputPorts();
    }

    private void CreateInputPorts()
    {
        switch (node)
        {
            case DecoratorNode:
            case CompositeNode:
            case ActionNode:
                input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
                break;
            case RootNode: //Has no input
                break;
        }

        if (input != null)
        {
            input.portName = string.Empty;
            inputContainer.Add(input);
        }
    }

    private void CreateOutputPorts()
    {
        switch (node)
        {
            //Action node has no children.
            case CompositeNode:
                output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
                break;
            case DecoratorNode:
                output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                break;
            case RootNode: //Same as decorator, only has one child.
                output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                break;
        }

        if (output != null)
        {
            output.portName = string.Empty;
            outputContainer.Add(output);
        }
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        node.position = new(newPos.xMin, newPos.yMin);
    }

    public override void OnSelected()
    {
        base.OnSelected();
        OnNodeSelected?.Invoke(this);
    }
}