using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;
using ZDialoguerEditor;

public class PredicateNodeView : SequencialNodeView
{
    private PredicateNodeObject _predicateNodeObject => NodeObject as PredicateNodeObject;
    public override NodeView BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph, ref int index)
    {
        var predicateNodeObject = nodeObject as PredicateNodeObject;
        
        base.BuildNodeView(nodeObject, graph, ref index);
        titleContainer.style.backgroundColor = new StyleColor(new Color(0.64f, 0.96f, 0.88f));

        PopupField<string> operationEnumField =
            new PopupField<string>(new List<string> { "=", ">", "<", "≥", "≤", "≠" }, (int)predicateNodeObject.operation);
        operationEnumField.RegisterValueChangedCallback(e =>
            OperationChangeCallback(e, predicateNodeObject));

        CreateInputPort(typeof(Fact), "Fact", inputContainer, predicateNodeObject, ref index);
            
        Button testButton = new Button(() =>
        {
            var window = EditorWindow.GetWindow<ZDialogueGraphEditorWindow>();
            window.graphView.GetNodeByGuid(predicateNodeObject.SequenceChild.guid).Select(window.graphView, false);
        });
        testButton.text = "Test";
        outputContainer.Add(testButton);
            
        FloatField valueField = new FloatField();
        valueField.SetValueWithoutNotify(predicateNodeObject.value);
        valueField.RegisterValueChangedCallback(e =>
            UpdatePredicateNodeValue(e, predicateNodeObject));

        inputContainer.Add(operationEnumField);
        inputContainer.Add(valueField);

        Font font = Resources.Load<Font>("Fonts/FugazOne");

        IMGUIContainer autoUpdateContainer = new IMGUIContainer((() =>
        {
            GUILayout.Label(predicateNodeObject.GetPredicate().ToString(),
                new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, fontSize = 20, font = font });
        }));
        mainContainer.Add(autoUpdateContainer);

        title = "Predicate Node";
        return this;
    }
    private void UpdatePredicateNodeValue(ChangeEvent<float> evt, PredicateNodeObject predicateNodeObject)
    {
        predicateNodeObject.value = evt.newValue;
        EditorUtility.SetDirty(predicateNodeObject);
        AssetDatabase.SaveAssets();
    }
    private void OperationChangeCallback(ChangeEvent<string> evt, PredicateNodeObject predicateNodeObject)
    {
        Dictionary<string, PredicateNodeObject.Operation> stringMap =
            new Dictionary<string, PredicateNodeObject.Operation>()
            {
                { "=", PredicateNodeObject.Operation.Equals },
                { ">", PredicateNodeObject.Operation.Greater },
                { "<", PredicateNodeObject.Operation.Lower },
                { "≥", PredicateNodeObject.Operation.GreaterEqual },
                { "≤", PredicateNodeObject.Operation.LowerEqual },
                { "≠", PredicateNodeObject.Operation.Not },
            };

        predicateNodeObject.operation = stringMap[evt.newValue];
        EditorUtility.SetDirty(predicateNodeObject);
        AssetDatabase.SaveAssets();
    }
    
    public override void OnConnectEdgeToInputPort(Edge edge)
    {
        switch (edge.input.viewDataKey.Last())
        {
            case '3': // ID of Fact Port
                _predicateNodeObject.fact = ((FactNodeObject)((NodeView)edge.output.node).NodeObject).fact;
                edge.input.portName = ((PredicateNodeObject)NodeObject).fact.nameID;
                break;
        }
    }

    public override void OnConnectEdgeToOutputPort(Edge edge)
    {
        switch (edge.output.viewDataKey.Last())
        {
            case '1': // ID of True Port
                _predicateNodeObject.childIfTrue =((NodeView)edge.input.node).NodeObject; // Will need rework
                break;
            case '2': // ID of False Port
                _predicateNodeObject.childIfFalse = ((NodeView)edge.input.node).NodeObject; // Will need rework
                break;
        }
    }

    public override void OnDisconnectEdgeFromInputPort(Edge edge)
    {
        switch (edge.input.viewDataKey.Last())
        {
            case '3': // ID of Fact Port
                _predicateNodeObject.fact = null;
                edge.input.portName = "Fact";
                break;
        }
    }

    public override void OnDisconnectEdgeFromOutputPort(Edge edge)
    {
        switch (edge.output.viewDataKey.Last())
        {
            case '1': // ID of True Port
                _predicateNodeObject.childIfTrue = null;
                break;
            case '2': // ID of False Port
                _predicateNodeObject.childIfFalse = null;
                break;
        }
    }
}
