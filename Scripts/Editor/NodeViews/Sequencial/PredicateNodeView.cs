using System;
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

public class PredicateNodeView : SequentialNodeView
{
    private PredicateNodeObject _predicateNodeObject => NodeObject as PredicateNodeObject;

    public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
    {
        int index = 0;
        var predicateNodeObject = nodeObject as PredicateNodeObject;
        base.BuildNodeView(nodeObject, graph);
        CreateInputPort(typeof(SequentialNodeObject), "►", inputContainer, nodeObject, ref index, Port.Capacity.Multi);
        CreateOutputPort(typeof(SequentialNodeObject), "True ►", outputContainer, nodeObject, ref index, Port.Capacity.Single);
        CreateOutputPort(typeof(SequentialNodeObject), "False ►", outputContainer, nodeObject, ref index, Port.Capacity.Single);
        titleContainer.style.backgroundColor = new StyleColor(new Color(0.64f, 0.96f, 0.88f));

        PopupField<string> operationEnumField =
            new PopupField<string>(new List<string> { "=", ">", "<", "≥", "≤", "≠" },
                (int)predicateNodeObject.operation);
        operationEnumField.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
        // Debug.Log(operationEnumField.Q<TextElement>());
        operationEnumField.Q<TextElement>().style.unityTextAlign = TextAnchor.MiddleCenter;
        // Debug.LogWarning(operationEnumField.ElementAt(0));
        operationEnumField.Q<VisualElement>().Query<VisualElement>().ToList()
            .First(x => x.GetClasses().Any(c => c == "unity-base-popup-field__arrow")).RemoveFromHierarchy();
        operationEnumField.style.width = 20;
        operationEnumField.Q<TextElement>().style.width = 20;
        operationEnumField.ElementAt(0).style.width = 20;
        operationEnumField.ElementAt(0).style.minWidth = 0;
        // operationEnumField.Query<VisualElement>().ForEach(Debug.Log);
        // Debug.Log("---");
        operationEnumField.RegisterValueChangedCallback(e =>
            OperationChangeCallback(e, predicateNodeObject));

        CreateInputPort(typeof(Fact), "Fact", inputContainer, predicateNodeObject, ref index);
        CreateOutputPort(typeof(bool), "Predicate", outputContainer, predicateNodeObject, ref index);

        // Button testButton = new Button(() =>
        // {
        //     var window = EditorWindow.GetWindow<ZDialogueGraphEditorWindow>();
        //     var node = window.graphView.GetNodeByGuid(predicateNodeObject.SequenceChild.guid);
        //     node.Select(window.graphView, false);
        //     node.Focus();
        // });
        // testButton.text = "Test";
        // outputContainer.Add(testButton);



        Font font = Resources.Load<Font>("Fonts/FugazOne");

        IMGUIContainer factNameContainer = new IMGUIContainer((() =>
        {
            GUILayout.Label(predicateNodeObject.fact ? predicateNodeObject.fact.nameID : "Fact",
                new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, fontSize = 20, font = font });
        }));

        IMGUIContainer autoUpdateContainer = new IMGUIContainer(() =>
        {
            GUILayout.Label(predicateNodeObject.GetPredicate().ToString(),
                new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, fontSize = 20, font = font });
        });
        autoUpdateContainer.style.borderTopColor = new StyleColor(new Color(.25f, .25f, .25f));
        autoUpdateContainer.style.borderTopWidth = new StyleFloat(1);
        VisualElement horizontalContainer = new VisualElement
        {
            style =
            {
                paddingBottom = 5,
                paddingTop = 5,
                paddingLeft = 5,
                paddingRight = 5,
                alignItems = Align.Center,
                alignSelf = Align.Center,
                flexDirection = FlexDirection.Row
            }
        };
        horizontalContainer.Add(factNameContainer);
        horizontalContainer.Add(operationEnumField);
        GenerateValueField(horizontalContainer);
        extensionContainer.Add(horizontalContainer);
        extensionContainer.Add(autoUpdateContainer);

        title = "Predicate Node";
    }

    private void UpdatePredicateNodeValue(object value, PredicateNodeObject predicateNodeObject)
    {
        predicateNodeObject.Value = value;
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
    private void OnFactTypeChange(Fact.FactType newFactType)
    {
        this.Q("factValueField").RemoveFromHierarchy();
        GenerateValueField(this.Q("factFieldContainer"));
    }

    private void GenerateValueField(VisualElement horizontalContainer)
    {
        VisualElement valueField;
        
        if (_predicateNodeObject.fact)
        {
            switch (_predicateNodeObject.fact.factType)
            {
                case Fact.FactType.Float:
                    FloatField floatField = new FloatField();
                    floatField.SetValueWithoutNotify((float)_predicateNodeObject.Value);
                    floatField.RegisterValueChangedCallback(e =>
                        UpdatePredicateNodeValue(e.newValue, _predicateNodeObject));
                    valueField = floatField;
                    break;
                case Fact.FactType.String:
                    TextField stringField = new TextField();
                    stringField.SetValueWithoutNotify((string)_predicateNodeObject.Value);
                    stringField.RegisterValueChangedCallback(e =>
                        UpdatePredicateNodeValue(e.newValue, _predicateNodeObject));
                    valueField = stringField;
                    break;
                default: throw new NotImplementedException();
            }
        }
        else
        {
            valueField = new Label("No Fact Connected!");
        }

        valueField.name = "factValueField";
        horizontalContainer.name = "factFieldContainer";
        horizontalContainer.Add(valueField);
    }

    public override void OnConnectEdgeToInputPort(Edge edge)
    {
        edge.IsInputKey("3",
            () =>
            {
                _predicateNodeObject.fact = ((FactNodeObject)((NodeView)edge.output.node).NodeObject).fact;
                _predicateNodeObject.fact.OnFactTypeChange += OnFactTypeChange;
                OnFactTypeChange(default);
                
            });
    }

    public override void OnConnectEdgeToOutputPort(Edge edge)
    {
        edge.IsOutputKey("1", () => _predicateNodeObject.childIfTrue = ((NodeView)edge.input.node).NodeObject);
        edge.IsOutputKey("2", () => _predicateNodeObject.childIfFalse = ((NodeView)edge.input.node).NodeObject);
    }

    public override void OnDisconnectEdgeFromInputPort(Edge edge)
    {
        edge.IsInputKey("3", () =>
        {
            _predicateNodeObject.fact.OnFactTypeChange -= OnFactTypeChange;
            _predicateNodeObject.fact = null;
            OnFactTypeChange(default);
        });
    }

    public override void OnDisconnectEdgeFromOutputPort(Edge edge)
    {
        edge.IsOutputKey("1", () => _predicateNodeObject.childIfTrue = null);
        edge.IsOutputKey("2", () => _predicateNodeObject.childIfFalse = null);
    }
}