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

        Button addEntryButton = new Button(() => AddOutputEntry())
            { text = "+", style = { width = 24, height = 24, fontSize = 24 } };
        titleContainer.Add(addEntryButton);
        outputContainer.Add(new Label("Output") { style = { unityTextAlign = TextAnchor.MiddleCenter } });
        GenerateOutputEntries(outputContainer );
        
        Font font = Resources.Load<Font>("Fonts/FugazOne");
        
        mainContainer.Add(new IMGUIContainer((() =>
        {
            GUILayout.Label($"Output: { switchNode.GetValue() ?? "None" }",
                new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, fontSize = 20, font = font });
        })));


    }

    void AddOutputEntry()
    {
        switchNode.outputEntries.Add(new SwitchNodeObject.OutputEntry());
        GenerateOutputEntries(outputContainer);
    }

    void RemoveOutputEntryAt(int position)
    {
        switchNode.outputEntries.RemoveAt(position);
        GenerateOutputEntries(outputContainer);
    }

    private void OnFactTypeChange(Fact.FactType newFactType = Fact.FactType.Float)
    {
        GenerateOutputEntries(outputContainer);
    }

    void UpdateOutputEntriesAt(int index, object value)
    {
        switchNode.outputEntries[index].SetValue(value, switchNode.fact.factType);
        EditorUtility.SetDirty(switchNode);
        AssetDatabase.SaveAssets();
    }

    private void GenerateOutputEntries(VisualElement container)
    {
        container.Q("OutputEntriesContainer")?.RemoveFromHierarchy();

        VisualElement outputEntriesContainer = new VisualElement { name = "OutputEntriesContainer" };

        if (switchNode.fact)
        {
            for (var i = 0; i < switchNode.outputEntries.Count; i++)
            {
                var outputEntry = switchNode.outputEntries[i];
                VisualElement row = new VisualElement
                {
                    name = $"row{i}",
                    style = { flexDirection = FlexDirection.Row, alignItems = Align.FlexEnd, alignSelf = Align.FlexEnd }
                };
                int position = i;


                switch (switchNode.fact.factType)
                {
                    case Fact.FactType.Float:
                        FloatField floatField = new FloatField();
                        floatField.SetValueWithoutNotify(outputEntry.floatValue);
                        floatField.RegisterValueChangedCallback(e => UpdateOutputEntriesAt(position, e.newValue));
                        row.Add(floatField);
                        break;
                    case Fact.FactType.String:
                        TextField stringField = new TextField();
                        stringField.SetValueWithoutNotify(outputEntry.stringValue);
                        stringField.RegisterValueChangedCallback(e => UpdateOutputEntriesAt(position, e.newValue));
                        row.Add(stringField);
                        break;
                }

                int startIndex = position + outputNodeStartIndex;

                var button = new Button(() => RemoveOutputEntryAt(position))
                    { text = "-", style = { alignSelf = Align.FlexEnd, flexGrow = 1, flexShrink = 1 } };

                var port = CreateOutputPort(typeof(SequentialNodeObject), "►", null, switchNode, ref startIndex,
                    Port.Capacity.Single);
                port.style.alignSelf = Align.FlexEnd;
                port.style.flexGrow = 1;

                row.Add(button);
                row.Add(port);
                outputEntriesContainer.Add(row);
            }
        }
        else{
            outputEntriesContainer.Add(new Label("No Fact Selected!"){style = {unityTextAlign = TextAnchor.MiddleCenter}});
        }

        container.Add(outputEntriesContainer);
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
        switchNode.outputEntries[Int32.Parse(new string( new [] {edge.output.viewDataKey.Last()})) - outputNodeStartIndex].output =
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
        switchNode.outputEntries[Int32.Parse(new string( new [] {edge.output.viewDataKey.Last()})) - outputNodeStartIndex].output = null;
    }
}