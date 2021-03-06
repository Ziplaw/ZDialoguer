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

        extensionContainer.Add(new IMGUIContainer(() =>
        {
            GUILayout.Label($"Output: {switchNode.GetValue(out int position) ?? "None"} - ({(position+1 == 0 ? "" : (position+1).ToString() )})",
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
        for (var i = 0; i < currentGraphView.graph.edgeDatas.Count; i++)
        {
            for (int j = 0; j < switchNode.outputEntries.Count; j++)
            {
                if (currentGraphView.graph.edgeDatas.Count == 0) break;
                var graphEdgeData = currentGraphView.graph.edgeDatas[i];
                if (graphEdgeData.outputPortViewDataKey ==
                    NodeObject.guid + " " + (position + j + outputNodeStartIndex))
                {

                    if (j != 0)
                    {
                        graphEdgeData.outputPortViewDataKey =
                            NodeObject.guid + " " + (position + j + outputNodeStartIndex - 1);
                    }
                    else
                    {
                        currentGraphView.graph.edgeDatas.RemoveAt(i);
                        if (i > 0) i--;
                    }
                }
            }
        }
        
        switchNode.outputEntries.RemoveAt(position);
        var port = this.Query<Port>().ToList().First(p => p.GetID(Direction.Output) == position + outputNodeStartIndex);

        // GraphViewChange change = new GraphViewChange
        //     { elementsToRemove = port.connections.Select(e => e as GraphElement).ToList() };
        // currentGraphView.graphViewChanged.Invoke(change);


        for (var i = 0; i < port.connections.ToList().Count; i++)
        {
            var portConnection = port.connections.ToList()[i];
            portConnection.input.Disconnect(portConnection);
            portConnection.output.Disconnect(portConnection);
            portConnection.RemoveFromHierarchy();
            i--;
        }

        port.RemoveFromHierarchy();

        var portList = outputContainer.Query<Port>().ToList();
        
        for (var i = 0; i < portList.Count; i++)
        {
            portList[i].viewDataKey = NodeObject.guid + " " + (i + outputNodeStartIndex);
        }

        

        for (var i = 0; i < switchNode.outputEntries.Count; i++)
        {
            // Debug.Log(i <= position? i : i+1 );
            // Debug.Log(outputContainer.Q($"row{(i <= position? i : i+1 )}"));
            var currPort = portList[i];
            port.viewDataKey= NodeObject.guid + " " + (i + outputNodeStartIndex);
            var container = GenerateRowContainer(currPort, i);

            GenerateField(container, i);
        }
        
        
    }

    VisualElement GenerateRowContainer(Port port, int rowPosition)
    {
        port.contentContainer.Q($"row{rowPosition}")?.RemoveFromHierarchy();
        port.contentContainer.Q($"row{rowPosition+1}")?.RemoveFromHierarchy();

        var container = new VisualElement
        {
            name = $"row{rowPosition}",
            style =
            {
                flexDirection = FlexDirection.Row, alignItems = Align.FlexEnd, alignSelf = Align.FlexEnd, flexGrow = 1
            }
        };

        
        port.contentContainer.Add(container);
        return container;
    }

    Port GenerateOutputPort(int position, int rowPosition)
    {
        var port = CreateOutputPort(typeof(SequentialNodeObject), "►", null, switchNode, ref position,
            Port.Capacity.Single);
        port.style.alignSelf = Align.FlexEnd;

        GenerateRowContainer(port, rowPosition);
        
        outputContainer.Add(port);
        return port;
    }

    void GenerateField(VisualElement container, int outputEntryPosition)
    {
        container?.Q("valueField")?.RemoveFromHierarchy();
        container?.Q("removeButton")?.RemoveFromHierarchy();
        
        var button = new Button(() => RemoveOutputPort(outputEntryPosition))
            { name = "removeButton", text = $"-", style = { alignSelf = Align.FlexEnd, flexGrow = 0, flexShrink = 0 } };

        container?.Add(button);

        if (switchNode.FactInstance != null)
        {
            var localIndex = outputEntryPosition;
            var outputEntry = switchNode.outputEntries[outputEntryPosition];

            switch (switchNode.FactInstance.factType)
            {
                case Fact.FactType.Float:
                    FloatField floatField = new FloatField
                        { name = "valueField", style = { flexGrow = 0, flexShrink = 1 } };
                    floatField.SetValueWithoutNotify(outputEntry.floatValue);
                    // floatField.SetValueWithoutNotify(localIndex);
                    floatField.RegisterValueChangedCallback(e =>
                        UpdateOutputEntriesAt(localIndex, e.newValue));
                    container?.Insert(0, floatField);

                    break;
                case Fact.FactType.String:
                    TextField stringField = new TextField
                        { name = "valueField", style = { flexGrow = 0, flexShrink = 1 } };
                    stringField.SetValueWithoutNotify(outputEntry.stringValue);
                    // stringField.SetValueWithoutNotify(localIndex.ToString());
                    stringField.RegisterValueChangedCallback(e =>
                        UpdateOutputEntriesAt(localIndex, e.newValue));
                    container?.Insert(0, stringField);
                    break;
            }
        }
    }

    private void OnFactTypeChange(Fact.FactType newFactType = Fact.FactType.Float)
    {
        for (var i = 0; i < switchNode.outputEntries.Count; i++)
        {
            GenerateField(outputContainer.Q($"row{i}"), i);
        }
    }

    void UpdateOutputEntriesAt(int index, object value)
    {
        switchNode.outputEntries[index].SetValue(value, switchNode.FactInstance.factType);
        EditorUtility.SetDirty(switchNode);
        AssetDatabase.SaveAssets();
    }

    public override void OnConnectEdgeToInputPort(Edge edge)
    {
        edge.IsInputKey(1, () =>
        {
            var switchNode = NodeObject as SwitchNodeObject;
            switchNode.factIndex = ((edge.output.node as FactNodeView).NodeObject as FactNodeObject).factIndex;
            // switchNode.fact.OnFactTypeChange += OnFactTypeChange;
            OnFactTypeChange();
        });
    }


    public override void OnConnectEdgeToOutputPort(Edge edge)
    {
        switchNode.outputEntries[edge.output.GetID(Direction.Output) - outputNodeStartIndex].output =
            (edge.input.node as SequentialNodeView).NodeObject as SequentialNodeObject;
    }

    public override void OnDisconnectEdgeFromInputPort(Edge edge)
    {
        edge.IsInputKey(1, () =>
        {
            var switchNode = NodeObject as SwitchNodeObject;
            // switchNode.fact.OnFactTypeChange -= OnFactTypeChange;
            switchNode.factIndex = -1;
            OnFactTypeChange();
        });
    }

    public override void OnDisconnectEdgeFromOutputPort(Edge edge)
    {
        int disconnectEntryPosition = edge.output.GetID(Direction.Output) - outputNodeStartIndex;
        if (switchNode.outputEntries.Count > disconnectEntryPosition)
            switchNode.outputEntries[disconnectEntryPosition].output = null;
    }
}