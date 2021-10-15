using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using ZDialoguer.Localization;
using ZDialoguer.Localization.Editor;

namespace ZDialoguer
{
    public class ChoiceNodeView : SequentialNodeView
    {
        ChoiceNodeObject _choiceNodeObject;
        private int choiceStartPos = 2;
        public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
        {
            _choiceNodeObject = nodeObject as ChoiceNodeObject;
            base.BuildNodeView(nodeObject, graph);
            int index = 0;
            titleContainer.style.backgroundColor = new Color(0.96f, 0.9f, 0.56f);
            title = "Choice Node";
            
            
            CreateInputPort(typeof(SequentialNodeObject), "►", inputContainer, NodeObject, ref index);
            CreateOutputPort(typeof(SequentialNodeObject), "►", outputContainer, NodeObject, ref index);
            Button addButton = new Button (() =>
            {
                _choiceNodeObject.choices.Add(new Choice(new LocalisedString(true)));
            }){ text = "+", style = { width = 24, height = 24, fontSize = 24 } };
            
            for (int i = 0; i < _choiceNodeObject.choices.Count; i++)
            {
                AddChoiceContainer(i);
            }
            
            
            titleContainer.Add(addButton);
        }

        public void AddChoiceContainer(int index)
        {
            int portPosition = index + choiceStartPos;
            
            Port port = CreateOutputPort(typeof(SequentialNodeObject), "", outputContainer, _choiceNodeObject,
                ref portPosition);
            port.Q<Label>().RemoveFromHierarchy();
            var container = GeneratePortContainer(index);
            port.contentContainer.Add(container);
        }

        VisualElement GeneratePortContainer(int choicePosition)
        {
            // this.Q("choiceContentContainer")?.RemoveFromHierarchy();
            
            var container = new VisualElement{name = "choiceContentContainer", style={flexDirection = FlexDirection.Row, alignItems = Align.Center}};
            var colorList = new List<Color> { new Color(40f/255,40f/255,40f/255), new Color(0.25f, 0.25f, 0.25f)/*titleContainer.style.backgroundColor.value*/ };
            var iconList = new List<Texture2D>
            {
                Resources.Load<Texture2D>("Icons/hidden"),
                Resources.Load<Texture2D>("Icons/visible"),
            };
            
            Debug.Log(choicePosition);
            
            var visibilityButton = new TwoStateToggle<Choice.DisabledVisibility>(_choiceNodeObject.choices[choicePosition].visibility, button => _choiceNodeObject.choices[choicePosition].visibility = button.state, colorList, iconList){name = "visibilityButton"};
            
            _choiceNodeObject.choices[choicePosition].choiceText.csvFile = currentGraphView.graph.dialogueText;
            _choiceNodeObject.choices[choicePosition].choiceText.csvFileFullAssetPath = LocalisedString.GetFullAssetPath(_choiceNodeObject.choices[choicePosition].choiceText.csvFile);

            var propertyDrawer =
                new LocalisedStringPropertyDrawer{indexPosition = choicePosition, oneLine = true}.CreatePropertyGUI(
                    new SerializedObject(_choiceNodeObject).FindProperty("choices").GetArrayElementAtIndex(choicePosition).FindPropertyRelative("choiceText"));

            int _choicePos = choicePosition;

            var minusButton = new Button(() =>
            {
                _choiceNodeObject.choices.RemoveAt(_choicePos);
                RepopulateGraph(); 
            }) {text = "-", style = { width = 24, height = 24, fontSize = 24 }};
            
            container.Add(visibilityButton);
            container.Add(propertyDrawer);
            container.Add(minusButton);
            // var textField
            // var button = new Button();
            // var t = button as ITransitionAnimations;
            // button.clickable.clicked += () => t.Position(new Vector3(-10, 0, 0), 100);
            // container.Add(button);
            return container;
        }

        public override void OnConnectEdgeToInputPort(Edge edge)
        {
        }

        public override void OnConnectEdgeToOutputPort(Edge edge)
        {
            edge.IsOutputKey("1", () =>
            {
                _choiceNodeObject._sequenceChild = (edge.input.node as NodeView).NodeObject as SequentialNodeObject;
            });
        }

        public override void OnDisconnectEdgeFromInputPort(Edge edge)
        {
        }

        public override void OnDisconnectEdgeFromOutputPort(Edge edge)
        {
            edge.IsOutputKey("1", () =>
            {
                _choiceNodeObject._sequenceChild = null;
            });
        }
    }
}