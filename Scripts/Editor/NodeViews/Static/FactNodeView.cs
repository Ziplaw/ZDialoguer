using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;

public class FactNodeView : StaticNodeView
{
    public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
    {
        this.Q("title-button-container").RemoveFromHierarchy();
        
        int index = 0;
        FactNodeObject factNodeObject = nodeObject as FactNodeObject;

        base.BuildNodeView(nodeObject, graph);
        this.Q("title-label").style.color = colorMap[typeof(Fact)];
        // titleContainer.style.backgroundColor = new StyleColor(colorMap[typeof(Fact)]);
        //
        // PopupField<Fact> factEnumField = new PopupField<Fact>(graph.facts, factNodeObject.fact);
        // currentGraphView._blackBoard.editTextRequested += (blackboard, element, factName) => currentGraphView.schedule
        //     .Execute(() =>
        //     {
        //         factEnumField.RemoveFromHierarchy();
        //         factEnumField = new PopupField<Fact>(graph.facts, factNodeObject.fact);
        //         inputContainer.Insert(0, factEnumField);
        //     }).ForDuration(100);
        // factEnumField.RegisterValueChangedCallback(e => FactEnumChangeCallback(e, factNodeObject));
        //
        // var valueField = GenerateValueField(factNodeObject);
        //
        // inputContainer.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        // inputContainer.Add(factEnumField);

        // inputContainer.Add(valueField);
        // int valueFieldIndex = inputContainer.IndexOf(valueField);
        // factNodeObject.fact.OnFactTypeChange += type => OnFactTypeChange(valueFieldIndex, factNodeObject);
        var port = CreateOutputPort(typeof(Fact), "", titleContainer, nodeObject, ref index);
        port.style.alignSelf = Align.Center;
        title = factNodeObject.fact.nameID;
    }

    private void OnFactTypeChange(int valueFieldIndex, FactNodeObject factNodeObject)
    {
        this.Q("factValueField").RemoveFromHierarchy();
        var field = GenerateValueField(factNodeObject);
        inputContainer.Insert(valueFieldIndex, field);
    }

    private VisualElement GenerateValueField(FactNodeObject factNodeObject)
    {
        VisualElement valueField;

        switch (factNodeObject.fact.factType)
        {
            case Fact.FactType.Float:
                FloatField floatField = new FloatField();
                floatField.SetValueWithoutNotify((float)factNodeObject.fact.Value);
                floatField.RegisterValueChangedCallback(e => FactValueChangeCallback(e.newValue, factNodeObject));
                valueField = floatField;
                break;
            case Fact.FactType.String:
                TextField stringField = new TextField();
                stringField.SetValueWithoutNotify((string)factNodeObject.fact.Value);
                stringField.RegisterValueChangedCallback(e => FactValueChangeCallback(e.newValue, factNodeObject));
                valueField = stringField;
                break;
            default: throw new NotImplementedException();
        }

        valueField.name = "factValueField";

        return valueField;
    }

    private void FactEnumChangeCallback(ChangeEvent<Fact> evt, FactNodeObject nodeObject)
    {
        nodeObject.fact = evt.newValue;
        EditorUtility.SetDirty(nodeObject);
        AssetDatabase.SaveAssets();
    }

    private void FactValueChangeCallback(object value, FactNodeObject nodeObject)
    {
        nodeObject.fact.Value = value;
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