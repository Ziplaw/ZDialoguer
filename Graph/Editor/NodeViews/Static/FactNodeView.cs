using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;

public class FactNodeView : StaticNodeView
{
    public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph, ref int index)
    {
        base.BuildNodeView(nodeObject, graph, ref index);
        titleContainer.style.backgroundColor = new StyleColor(colorMap[typeof(Fact)]);
        FactNodeObject factNodeObject = nodeObject as FactNodeObject;

        PopupField<Fact> factEnumField = new PopupField<Fact>(graph.facts, factNodeObject.fact);
        factEnumField.RegisterValueChangedCallback(e => FactEnumChangeCallback(e, factNodeObject));
        FloatField field = new FloatField();
        field.SetValueWithoutNotify( factNodeObject.fact.value);
        field.RegisterValueChangedCallback(e => FactValueChangeCallback(e, factNodeObject));
        inputContainer.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        inputContainer.Add(factEnumField);
        inputContainer.Add(field);
        CreateOutputPort(typeof(Fact), "Fact", outputContainer, nodeObject, ref index);
        title = "Fact Node";
    }

    private void FactEnumChangeCallback(ChangeEvent<Fact> evt, FactNodeObject nodeObject)
    {
        nodeObject.fact = evt.newValue;
        EditorUtility.SetDirty(nodeObject);
        AssetDatabase.SaveAssets();
    }

    private void FactValueChangeCallback(ChangeEvent<float> evt, FactNodeObject nodeObject)
    {
        nodeObject.fact.value = evt.newValue;
        EditorUtility.SetDirty(nodeObject);
        AssetDatabase.SaveAssets();
    }

    public override void OnConnectEdgeToInputPort(Edge edge)
    {
    }

    public override void OnConnectEdgeToOutputPort(Edge edge)
    {
    }

    public override void OnDisconnectEdgeFromInputPort(Edge edge)
    {
    }

    public override void OnDisconnectEdgeFromOutputPort(Edge edge)
    {
    }
}