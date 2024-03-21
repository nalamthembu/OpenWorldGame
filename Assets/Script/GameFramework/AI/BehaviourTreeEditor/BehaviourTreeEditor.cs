using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BehaviourTreeEditor : EditorWindow
{
    const string VERSION_NUMBER = "1.0";
    const string PATH = "Assets/Script/GameFramework/AI/BehaviourTreeEditor/BehaviourTreeEditor.uxml";

    BehaviourTreeView treeView;
    InspectorView inspectorView;

    [MenuItem("OWFramework/Behaviour Tree Editor")]
    public static void OpenWindow()
    {
        BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
        wnd.titleContent = new GUIContent($"Behaviour Tree Editor (v{VERSION_NUMBER})");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PATH);

        //Check that the file exists
        if (!File.Exists(PATH))
        {
            //Check that it is a UXML file.
            if (!PATH.ToLower().Contains(".uxml"))
            {
                Debug.Log($"File at {PATH} is not a UXML file!");
            }
            return;
        }

        visualTree.CloneTree(root);
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(PATH);
        root.styleSheets.Add(styleSheet);

        //Set Instances
        treeView = root.Q<BehaviourTreeView>();
        inspectorView = root.Q<InspectorView>();
        treeView.OnNodeSelected = OnNodeSelectionChanged;
        OnSelectionChange();
    
    }

    private void OnSelectionChange()
    {
        BehaviourTree tree = Selection.activeObject as BehaviourTree;
        if (tree && AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
            treeView.PopulateView(tree);
    }

    void OnNodeSelectionChanged(NodeView node) => inspectorView.UpdateSelection(node);
}
