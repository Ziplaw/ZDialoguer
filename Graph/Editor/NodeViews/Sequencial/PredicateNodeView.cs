using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.UIElements;
using ZDialoguer;
using ZDialoguerEditor;

public class PredicateNodeView : SequencialNodeView
{
    private PredicateNodeObject _predicateNodeObject => NodeObject as PredicateNodeObject;
    public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
    {
        int index = 0;
        var predicateNodeObject = nodeObject as PredicateNodeObject;
        base.BuildNodeView(nodeObject, graph);
        CreateInputPort(typeof(SequencialNodeObject), "►", inputContainer, nodeObject, ref index, Port.Capacity.Multi);
        CreateOutputPort(typeof(SequencialNodeObject), "True ►",outputContainer, nodeObject, ref index);
        CreateOutputPort(typeof(SequencialNodeObject), "False ►",outputContainer, nodeObject, ref index);
        titleContainer.style.backgroundColor = new StyleColor(new Color(0.64f, 0.96f, 0.88f));

        PopupField<string> operationEnumField =
            new PopupField<string>(new List<string> { "=", ">", "<", "≥", "≤", "≠" }, (int)predicateNodeObject.operation);
        operationEnumField.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
        // Debug.Log(operationEnumField.Q<TextElement>());
        operationEnumField.Q<TextElement>().style.unityTextAlign = TextAnchor.MiddleCenter;
        // Debug.LogWarning(operationEnumField.ElementAt(0));
        operationEnumField.Q<VisualElement>().Query<VisualElement>().ToList().First(x => x.GetClasses().Any(c => c == "unity-base-popup-field__arrow")).RemoveFromHierarchy();
        operationEnumField.style.width = 20;
        operationEnumField.Q<TextElement>().style.width = 20;
        operationEnumField.ElementAt(0).style.width = 20;
        operationEnumField.ElementAt(0).style.minWidth = 0;
        // operationEnumField.Query<VisualElement>().ForEach(Debug.Log);
        // Debug.Log("---");
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

        Font font = Resources.Load<Font>("Fonts/FugazOne");

        IMGUIContainer factNameContainer = new IMGUIContainer((() =>
        {
            GUILayout.Label(predicateNodeObject.fact? predicateNodeObject.fact.nameID : "Fact",
                new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, fontSize = 20, font = font });
        }));
        
        IMGUIContainer autoUpdateContainer = new IMGUIContainer((() =>
        {
            GUILayout.Label(predicateNodeObject.GetPredicate().ToString(),
                new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, fontSize = 20, font = font });
        }));
        autoUpdateContainer.style.borderTopColor = new StyleColor(new Color(.25f,.25f,.25f));
        autoUpdateContainer.style.borderTopWidth = new StyleFloat(1);
        VisualElement horizontalContainer = new VisualElement();
        horizontalContainer.style.paddingBottom = 5;
        horizontalContainer.style.paddingTop = 5;
        horizontalContainer.style.paddingLeft = 5;
        horizontalContainer.style.paddingRight = 5;
        horizontalContainer.style.alignItems = Align.Center;
        horizontalContainer.style.alignSelf = Align.Center;
        horizontalContainer.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        horizontalContainer.Add(factNameContainer);
        horizontalContainer.Add(operationEnumField);
        horizontalContainer.Add(valueField);
        mainContainer.Add(horizontalContainer);
        mainContainer.Add(autoUpdateContainer);

        title = "Predicate Node";
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
        edge.IsInputKey('3', () =>
        {
            _predicateNodeObject.fact = ((FactNodeObject)((NodeView)edge.output.node).NodeObject).fact;
        });
    }

    public override void OnConnectEdgeToOutputPort(Edge edge)
    {
        edge.IsOutputKey('1', () => _predicateNodeObject.childIfTrue = ((NodeView)edge.input.node).NodeObject);
        edge.IsOutputKey('2', () => _predicateNodeObject.childIfFalse = ((NodeView)edge.input.node).NodeObject);
    }

    public override void OnDisconnectEdgeFromInputPort(Edge edge)
    {
        edge.IsInputKey('3', () => _predicateNodeObject.fact = null);
    }

    public override void OnDisconnectEdgeFromOutputPort(Edge edge)
    {
        edge.IsOutputKey('1', () => _predicateNodeObject.childIfTrue = null);
        edge.IsOutputKey('2', () => _predicateNodeObject.childIfFalse = null);
    }
}
