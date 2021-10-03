using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguerEditor;

namespace ZDialoguer
{
    public class NodeView : Node
    {
        public Action<NodeView> OnNodeSelected;
        public NodeObject NodeObject;
        public Port input;
        public Port output;

        public NodeView(NodeObject nodeObject, ZDialogueGraph graph)
        {
            NodeObject = nodeObject;

            BuildNodeView(graph);

            viewDataKey = nodeObject.guid;

            style.left = nodeObject.position.x;
            style.top = nodeObject.position.y;
        }

        public void BuildNodeView(ZDialogueGraph graph)
        {
            int index = 0;
            mainContainer.style.backgroundColor = new StyleColor(new Color(0.17f, 0.17f, 0.17f));
            inputContainer.Clear();
            outputContainer.Clear();

            switch (NodeObject.GetType().BaseType.Name)
            {
                case "SequencialNodeObject":

                    CreateInputPort(typeof(NodeObject), "►", inputContainer, NodeObject, ref index);
                    CreateOutputPort(typeof(NodeObject), "True ►", NodeObject, ref index);
                    CreateOutputPort(typeof(NodeObject), "False ►", NodeObject, ref index);

                    switch (NodeObject.GetType().Name)
                    {
                        case "PredicateNodeObject":
                            BuildPredicateNodeView(NodeObject as PredicateNodeObject, ref index);

                            break;
                    }

                    break;
                case "StaticNodeObject":
                    switch (NodeObject.GetType().Name)
                    {
                        case "FactNodeObject":
                            BuildFactNodeView(NodeObject as FactNodeObject, graph, ref index);
                            break;
                    }

                    break;
            }

            RefreshPorts();
        }

        public void BuildFactNodeView(FactNodeObject nodeObject, ZDialogueGraph graph, ref int index)
        {
            titleContainer.style.backgroundColor = new StyleColor(colorMap[typeof(Fact)]);
            FactNodeObject factNodeObject = nodeObject;

            PopupField<Fact> factEnumField = new PopupField<Fact>(graph.facts, factNodeObject.fact);
            factEnumField.RegisterValueChangedCallback(e => FactEnumChangeCallback(e, factNodeObject));
            inputContainer.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            inputContainer.Add(factEnumField);
            inputContainer.Add(new IMGUIContainer(() =>
                GUILayout.Label($"({factNodeObject.fact.value})")));
            inputContainer.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            CreateOutputPort(typeof(Fact), "Fact", nodeObject, ref index);
            title = "Fact Node";
        }

        public void BuildPredicateNodeView(PredicateNodeObject predicateNodeObject, ref int index)
        {
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
        }

        private void UpdatePredicateNodeValue(ChangeEvent<float> evt, PredicateNodeObject predicateNodeObject)
        {
            predicateNodeObject.value = evt.newValue;
            EditorUtility.SetDirty(NodeObject);
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
            EditorUtility.SetDirty(NodeObject);
            AssetDatabase.SaveAssets();
        }

        private void FactEnumChangeCallback(ChangeEvent<Fact> evt, FactNodeObject nodeObject)
        {
            nodeObject.fact = evt.newValue;
            EditorUtility.SetDirty(nodeObject);
            AssetDatabase.SaveAssets();
        }

        void CreateInputPort(Type type, string portName, VisualElement container, NodeObject nodeObject, ref int index,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            input = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, type);
            input.portName = portName;
            input.portColor = colorMap[type];
            input.viewDataKey = nodeObject.guid + " " + index;
            index++;
            container.Add(input);
        }

        private Dictionary<Type, Color> colorMap = new Dictionary<Type, Color>()
        {
            { typeof(Fact), new Color(1f, 0.65f, 0f) },
            { typeof(NodeObject), new Color(0.55f, 0.42f, 1f) }
        };

        void CreateOutputPort(Type type, string portName, NodeObject nodeObject, ref int index)
        {
            output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, type);
            output.portName = portName;
            output.portColor = colorMap[type];
            output.viewDataKey = nodeObject.guid + " " + index;
            index++;
            outputContainer.Add(output);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            NodeObject.position.x = newPos.xMin;
            NodeObject.position.y = newPos.yMin;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }
    }
}