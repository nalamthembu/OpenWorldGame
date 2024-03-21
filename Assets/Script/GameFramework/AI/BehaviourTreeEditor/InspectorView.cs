using System;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

    Editor editor;

    public InspectorView() { }

    internal void UpdateSelection(NodeView nodeView)
    {
        Clear();

        UnityEngine.Object.DestroyImmediate(editor);

        editor = Editor.CreateEditor(nodeView.node);

        IMGUIContainer container = new(() => { editor.OnInspectorGUI(); });

        Add(container);
    }
}