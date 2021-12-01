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
    private Fact fact;
    private Fact secondFact;

    public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
    {
        int index = 0;
        var predicateNodeObject = nodeObject as PredicateNodeObject;
        base.BuildNodeView(nodeObject, graph);
        CreateInputPort(typeof(SequentialNodeObject), "►", inputContainer, nodeObject, ref index, Port.Capacity.Multi);
        CreateOutputPort(typeof(SequentialNodeObject), "True ►", outputContainer, nodeObject, ref index,
            Port.Capacity.Single);
        CreateOutputPort(typeof(SequentialNodeObject), "False ►", outputContainer, nodeObject, ref index,
            Port.Capacity.Single);
        titleContainer.style.backgroundColor = new StyleColor(new Color(0.64f, 0.96f, 0.88f));

        fact = predicateNodeObject.factIndex == -1 ? new Fact() : GlobalData.Instance.facts[predicateNodeObject.factIndex];
        secondFact = predicateNodeObject.secondFactIndex == -1 ? new Fact() : GlobalData.Instance.facts[predicateNodeObject.secondFactIndex];

        List<string> stringList = !fact.initialized || fact.initialized && fact.factType == Fact.FactType.Float
            ? new List<string> { "=", ">", "<", "≥", "≤", "≠" }
            : new List<string> { "=", "≠" };

        PopupField<string> operationEnumField =
            new PopupField<string>(stringList,
                (int)predicateNodeObject.operation);
        operationEnumField.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
        operationEnumField.Q<TextElement>().style.unityTextAlign = TextAnchor.MiddleCenter;
        operationEnumField.Q<VisualElement>().Query<VisualElement>().ToList()
            .First(x => x.GetClasses().Any(c => c == "unity-base-popup-field__arrow")).RemoveFromHierarchy();
        operationEnumField.style.width = 20;
        operationEnumField.Q<TextElement>().style.width = 20;
        operationEnumField.ElementAt(0).style.width = 20;
        operationEnumField.ElementAt(0).style.minWidth = 0;
        operationEnumField.RegisterValueChangedCallback(e =>
            OperationChangeCallback(e, predicateNodeObject));

        CreateInputPort(typeof(Fact), "Fact", inputContainer, predicateNodeObject, ref index);
        CreateOutputPort(typeof(bool), "Predicate", outputContainer, predicateNodeObject, ref index);
        if (fact.initialized)
            CreateInputPort(typeof(Fact), "Fact", inputContainer, predicateNodeObject, ref index);


        Font font = Resources.Load<Font>("Fonts/FugazOne");
        Label factNameLabel = new Label(fact.initialized ? fact.nameID : "Fact")
            { style = { unityTextAlign = TextAnchor.MiddleCenter, fontSize = 20, unityFont = font } };

        // IMGUIContainer factNameContainer = new IMGUIContainer((() =>
        // {
        //     GUILayout.Label(predicateNodeObject.factNodeObject ? predicateNodeObject.factNodeObject.nameID : "Fact",
        //         new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, fontSize = 20, font = font });
        // }));

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
        horizontalContainer.Add(factNameLabel);
        // horizontalContainer.Add(factNameContainer);
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
            new Dictionary<string, PredicateNodeObject.Operation>
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


    private void GenerateValueField(VisualElement horizontalContainer)
    {
        VisualElement valueField;

        if (fact.initialized)
        {
            if (secondFact.initialized)
            {
                Font font = Resources.Load<Font>("Fonts/FugazOne");
                IMGUIContainer factNameContainer = new IMGUIContainer(() =>
                {
                    GUILayout.Label(secondFact.nameID,
                        new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, fontSize = 20, font = font });
                });
                valueField = factNameContainer;
            }
            else
            {
                switch (fact.factType)
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
                    default: throw new ArgumentOutOfRangeException();
                }
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
        edge.IsInputKey(3,
            () =>
            {
                _predicateNodeObject.factIndex = ((FactNodeObject)((NodeView)edge.output.node).NodeObject).factIndex;
                // _predicateNodeObject.fact.OnFactTypeChange += OnFactTypeChange;
                // OnFactTypeChange(default);
            });
        edge.IsInputKey(5,
            () =>
            {
                _predicateNodeObject.secondFactIndex = ((FactNodeObject)((NodeView)edge.output.node).NodeObject).factIndex;
                // _predicateNodeObject.secondFact.OnFactTypeChange += OnFactTypeChange;
                // OnFactTypeChange(default);
            });
    }

    public override void OnConnectEdgeToOutputPort(Edge edge)
    {
        edge.IsOutputKey(1, () => _predicateNodeObject.childIfTrue = ((NodeView)edge.input.node).NodeObject);
        edge.IsOutputKey(2, () => _predicateNodeObject.childIfFalse = ((NodeView)edge.input.node).NodeObject);
    }

    public override void OnDisconnectEdgeFromInputPort(Edge edge)
    {
        edge.IsInputKey(3, () =>
        {
            // _predicateNodeObject.fact.OnFactTypeChange -= OnFactTypeChange;
            _predicateNodeObject.factIndex = -1;
            // OnFactTypeChange(default);
        });
        edge.IsInputKey(5, () =>
        {
            // _predicateNodeObject.secondFact.OnFactTypeChange -= OnFactTypeChange;
            _predicateNodeObject.secondFactIndex = -1;
            // OnFactTypeChange(default);
        });
    }

    public override void OnDisconnectEdgeFromOutputPort(Edge edge)
    {
        edge.IsOutputKey(1, () => _predicateNodeObject.childIfTrue = null);
        edge.IsOutputKey(2, () => _predicateNodeObject.childIfFalse = null);
    }
}