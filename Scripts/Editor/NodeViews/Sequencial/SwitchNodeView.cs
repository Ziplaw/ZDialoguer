using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;
using Debug = UnityEngine.Debug;

public class SwitchNodeView : SequentialNodeView
{
    private SwitchNodeObject switchNode;
    private int outputNodeStartIndex = 2;

    public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
    {
        switchNode = nodeObject as SwitchNodeObject;
        base.BuildNodeView(nodeObject, graph);

        int index = 0;
        titleContainer.style.backgroundColor = new Color(0.64f, 0.96f, 0.88f);
        title = "Switch Node";

        CreateInputPort(typeof(SequentialNodeObject), "►", inputContainer, nodeObject, ref index);
        CreateInputPort(typeof(Fact), "Fact", inputContainer, nodeObject, ref index);

        Button addEntryButton = new Button(AddOutputEntry)
            { text = "+", style = { width = 24, height = 24, fontSize = 24 } };
        titleContainer.Add(addEntryButton);
        outputContainer.Add(new Label("Output") { style = { unityTextAlign = TextAnchor.MiddleCenter } });

        for (int i = 0; i < switchNode.outputEntries.Count; i++)
        {
            var position = i + index;
            GenerateOutputPort(position, i);
            GenerateField(outputContainer.Q($"row{i}"), i);
        }

        Font font = Resources.Load<Font>("Fonts/FugazOne");

        mainContainer.Add(new IMGUIContainer(() =>
        {
            GUILayout.Label($"Output: {switchNode.GetValue(out int position) ?? "None"} - ({position+1})",
                new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, fontSize = 20, font = font });
        }));
    }

    void AddOutputEntry()
    {
        switchNode.outputEntries.Add(new SwitchNodeObject.OutputEntry());

        int position = switchNode.outputEntries.Count - 1;

        if (outputContainer.Q($"row{position}") == null) GenerateOutputPort(position + outputNodeStartIndex, position);
        GenerateField(outputContainer.Q($"row{position}"), position);
    }

    private void RemoveOutputPort(int position)
    {
        switchNode.outputEntries.RemoveAt(position);
        var port = this.Query<Port>().ToList().First(p =>
            Int32.Parse(new string(new[] { p.viewDataKey.Last() })) == position + outputNodeStartIndex);

        GraphViewChange change = new GraphViewChange
            { elementsToRemove = port.connections.Select(e => e as GraphElement).ToList() };
        currentGraphView.graphViewChanged.Invoke(change);

        foreach (var portConnection in port.connections)
        {
            portConnection.RemoveFromHierarchy();
        }
        port.RemoveFromHierarchy();
    }

    void GenerateOutputPort(int position, int rowPosition)
    {
        var port = CreateOutputPort(typeof(SequentialNodeObject), "►", null, switchNode, ref position,
            Port.Capacity.Single);
        port.style.alignSelf = Align.FlexEnd;

        var container = new VisualElement
        {
            name = $"row{rowPosition}",
            style =
            {
                flexDirection = FlexDirection.Row, alignItems = Align.FlexEnd, alignSelf = Align.FlexEnd, flexGrow = 1
            }
        };

        var button = new Button(() => RemoveOutputPort(rowPosition))
            { text = "-", style = { alignSelf = Align.FlexEnd, flexGrow = 0, flexShrink = 0 } };

        container.Add(button);
        port.contentContainer.Add(container);
        outputContainer.Add(port);
    }

    void GenerateField(VisualElement container, int outputEntryPosition)
    {
        container?.Q("valueField")?.RemoveFromHierarchy();

        if (switchNode.fact)
        {
            var localIndex = outputEntryPosition;
            var outputEntry = switchNode.outputEntries[outputEntryPosition];

            switch (switchNode.fact.factType)
            {
                case Fact.FactType.Float:
                    FloatField floatField = new FloatField
                        { name = "valueField", style = { flexGrow = 0, flexShrink = 1 } };
                    floatField.SetValueWithoutNotify(outputEntry.floatValue);
                    floatField.RegisterValueChangedCallback(e =>
                        UpdateOutputEntriesAt(localIndex, e.newValue));
                    container.Insert(0, floatField);

                    break;
                case Fact.FactType.String:
                    TextField stringField = new TextField
                        { name = "valueField", style = { flexGrow = 0, flexShrink = 1 } };
                    stringField.SetValueWithoutNotify(outputEntry.stringValue);
                    stringField.RegisterValueChangedCallback(e =>
                        UpdateOutputEntriesAt(localIndex, e.newValue));
                    container.Insert(0, stringField);
                    break;
            }
        }
    }

    private void OnFactTypeChange(Fact.FactType newFactType = Fact.FactType.Float)
    {
        // for (var i = 0; i < switchNode.outputEntries.Count; i++)
        // {
        //     GenerateField(outputContainer.Q($"row{i}"), i);
        // }
    }

    void UpdateOutputEntriesAt(int index, object value)
    {
        switchNode.outputEntries[index].SetValue(value, switchNode.fact.factType);
        EditorUtility.SetDirty(switchNode);
        AssetDatabase.SaveAssets();
    }

    public override void OnConnectEdgeToInputPort(Edge edge)
    {
        edge.IsInputKey('1', () =>
        {
            var switchNode = NodeObject as SwitchNodeObject;
            switchNode.fact = ((edge.output.node as FactNodeView).NodeObject as FactNodeObject).fact;
            switchNode.fact.OnFactTypeChange += OnFactTypeChange;
            OnFactTypeChange();
        });
    }


    public override void OnConnectEdgeToOutputPort(Edge edge)
    {
        switchNode.outputEntries[
                Int32.Parse(new string(new[] { edge.output.viewDataKey.Last() })) - outputNodeStartIndex].output =
            (edge.input.node as SequentialNodeView).NodeObject as SequentialNodeObject;
    }

    public override void OnDisconnectEdgeFromInputPort(Edge edge)
    {
        edge.IsInputKey('1', () =>
        {
            var switchNode = NodeObject as SwitchNodeObject;
            switchNode.fact.OnFactTypeChange -= OnFactTypeChange;
            switchNode.fact = null;
            OnFactTypeChange();
        });
    }

    public override void OnDisconnectEdgeFromOutputPort(Edge edge)
    {
        int disconnectEntryPosition =
            Int32.Parse(new string(new[] { edge.output.viewDataKey.Last() })) - outputNodeStartIndex;
        if (switchNode.outputEntries.Count > disconnectEntryPosition)
            switchNode.outputEntries[disconnectEntryPosition].output = null;
    }
}