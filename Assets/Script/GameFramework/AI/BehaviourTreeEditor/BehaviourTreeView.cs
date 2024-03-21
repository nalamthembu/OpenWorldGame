using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;
using System;

public class BehaviourTreeView : GraphView
{
    const string PATH = "Assets\\Script\\GameFramework\\AI\\BehaviourTreeEditor\\BehaviourTreeEditor.uss";
    private readonly IManipulator[] manipulators = new IManipulator[]
    {
        new ContentZoomer(),
        new ContentDragger(),
        new SelectionDragger(),
        new RectangleSelector()
    };

    //Events
    public Action<NodeView> OnNodeSelected;

    public new class UxmlFactory: UxmlFactory<BehaviourTreeView, GraphView.UxmlTraits> { }
    BehaviourTree tree;
    public BehaviourTreeView()
    {
        Insert(0, new GridBackground());

        AddManipulators();

        //Style sheet
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(PATH);
        styleSheets.Add(styleSheet);
    }

    private void AddManipulators()
    {
        foreach (var i in manipulators)
            this.AddManipulator(i);
    }

    NodeView FindNodeView(Node node) => GetNodeByGuid(node.guid) as NodeView;

    internal void PopulateView(BehaviourTree tree)
    {
        this.tree = tree;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        if (tree.rootNode == null)
        {
            tree.rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }

        //Creates node views
        tree.nodes.ForEach(n => CreateNodeView(n));

        //Creates edges
        tree.nodes.ForEach(n =>
        {
            var children = tree.GetChildren(n);

            children.ForEach(c =>
            {
                NodeView parentView = FindNodeView(n);
                NodeView childView = FindNodeView(c);

                Edge edge = parentView.output.ConnectTo(childView.input);
                AddElement(edge);
            });
        });
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        graphViewChange.elementsToRemove?.ForEach(elem =>
        {
            if (elem is NodeView nodeView)
                tree.DeleteNode(nodeView.node);

            if (elem is Edge edge)
            {
                NodeView parentView = edge.output.node as NodeView;
                NodeView childView = edge.input.node as NodeView;
                tree.RemoveChild(parentView.node, childView.node);
            }
        });

        graphViewChange.edgesToCreate?.ForEach(edge =>
        {
            NodeView parentView = edge.output.node as NodeView;
            NodeView childView = edge.input.node as NodeView;
            tree.AddChild(parentView.node, childView.node);
        });

        return graphViewChange;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort =>
        endPort.direction != startPort.direction &&
        endPort.node != startPort.node).ToList();
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        //Action
        {
            var types = TypeCache.GetTypesDerivedFrom(typeof(ActionNode));
            foreach (var type in types)
                evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type));
        }

        //Composite
        {
            var types = TypeCache.GetTypesDerivedFrom(typeof(CompositeNode));
            foreach (var type in types)
                evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type));
        }

        //Decorator
        {
            var types = TypeCache.GetTypesDerivedFrom(typeof(DecoratorNode));
            foreach (var type in types)
                evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type));
        }

    }

    void CreateNode(System.Type type)
    {
        Node node = tree.CreateNode(type);
        CreateNodeView(node);
    }

    void CreateNodeView(Node node)
    {
        NodeView nodeView = new(node)
        {
            OnNodeSelected = OnNodeSelected
        };

        AddElement(nodeView);
    }
}