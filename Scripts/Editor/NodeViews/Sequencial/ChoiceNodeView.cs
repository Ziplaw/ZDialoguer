using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using ZDialoguer.Localization;
using ZDialoguer.Localization.Editor;

namespace ZGraph.DialogueSystem
{
    public class ChoiceZNodeView : SequentialZNodeView
    {
        ChoiceDialogueNodeObject _choiceDialogueNodeObject;
        private int choiceStartPos = 2;

        public override void BuildNodeView(ZNode Node, ZGraph graph)
        {
            _choiceDialogueNodeObject = Node as ChoiceDialogueNodeObject;
            base.BuildNodeView(Node, graph);
            int index = 0;
            titleContainer.style.backgroundColor = new Color(0.96f, 0.9f, 0.56f);
            title = "Choice Node";


            CreateInputPort(typeof(SequentialDialogueNodeObject), "►", inputContainer, ((ZNodeView)this).Node, ref index, Port.Capacity.Multi);
            CreateOutputPort(typeof(SequentialDialogueNodeObject), "►", outputContainer, ((ZNodeView)this).Node, ref index);
            Button addButton = new Button(() =>
            {
                _choiceDialogueNodeObject.choices.Add(new Choice(new LocalisedString(true)));
                RepopulateGraph();
            }) { text = "+", style = { width = 24, height = 24, fontSize = 24 } };

            for (int i = 0; i < _choiceDialogueNodeObject.choices.Count; i++)
            {
                AddChoiceContainer(i);
            }


            titleContainer.Add(addButton);
            
            _choiceDialogueNodeObject.dialogueText.csvFile = ((ZDialogueGraph)currentGraphView.graph).dialogueText;
            _choiceDialogueNodeObject.dialogueText.csvFileFullAssetPath =
                LocalisedString.GetFullAssetPath(_choiceDialogueNodeObject.dialogueText.csvFile);

            var propertyDrawer =
                new LocalisedStringPropertyDrawer{indexPosition = -1, stretch = true, ZNodeView = this, _container = extensionContainer, _containerPosition = 0}.CreatePropertyGUI(
                    new SerializedObject(_choiceDialogueNodeObject).FindProperty("dialogueText"));
            
            extensionContainer.Add(propertyDrawer);
        }

        public void AddChoiceContainer(int index)
        {
            int outputPortPos = index + choiceStartPos;
            int inputPortPos = index + choiceStartPos;

            Port outputPort = CreateOutputPort(typeof(SequentialDialogueNodeObject), $"Choice", outputContainer,
                _choiceDialogueNodeObject,
                ref outputPortPos);
            CreateInputPort(typeof(bool), $"Predicate {index + 1}", inputContainer, _choiceDialogueNodeObject,
                ref inputPortPos);
            // outputPort.Q<Label>().RemoveFromHierarchy();
            var container = GeneratePortContainer(index);
            outputPort.contentContainer.Add(container);
        }

        VisualElement GeneratePortContainer(int choicePosition)
        {
            this.Q($"choiceContentContainer{choicePosition}")?.RemoveFromHierarchy();

            var container = new VisualElement
            {
                name = $"choiceContentContainer{choicePosition}",
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
            };
            var colorList = new List<Color>
            {
                new Color(40f / 255, 40f / 255, 40f / 255),
                new Color(0.25f, 0.25f, 0.25f) /*titleContainer.style.backgroundColor.value*/
            };
            var iconList = new List<Texture2D>
            {
                Resources.Load<Texture2D>("Icons/hidden"),
                Resources.Load<Texture2D>("Icons/visible"),
            };

            var visibilityButton =
                new TwoStateToggle<Choice.DisabledVisibility>(_choiceDialogueNodeObject.choices[choicePosition].visibility,
                        button => _choiceDialogueNodeObject.choices[choicePosition].visibility = button.state, colorList,
                        iconList)
                    { name = "visibilityButton" };

            _choiceDialogueNodeObject.choices[choicePosition].choiceText.csvFile = ((ZDialogueGraph)currentGraphView.graph).dialogueText;
            _choiceDialogueNodeObject.choices[choicePosition].choiceText.csvFileFullAssetPath =
                LocalisedString.GetFullAssetPath(_choiceDialogueNodeObject.choices[choicePosition].choiceText.csvFile);

            var propertyDrawer =
                new LocalisedStringPropertyDrawer { indexPosition = choicePosition, oneLine = true, ZNodeView = this, _container = container, _containerPosition = _choiceDialogueNodeObject.choices[choicePosition].overriddenDialogueNode ? 1 : 0}.CreatePropertyGUI(
                    new SerializedObject(_choiceDialogueNodeObject).FindProperty("choices")
                        .GetArrayElementAtIndex(choicePosition).FindPropertyRelative("choiceText"));

            int _choicePos = choicePosition;

            var minusButton = new Button(() =>
            {
                _choiceDialogueNodeObject.choices.RemoveAt(_choicePos);
                RepopulateGraph();
            }) { text = "-", style = { width = 24, height = 24, fontSize = 24 } };

            if (_choiceDialogueNodeObject.choices[choicePosition].overriddenDialogueNode) container.Add(visibilityButton);
            container.Add(propertyDrawer);
            container.Add(minusButton);


            // var textField
            // var button = new Button();
            // var t = button as ITransitionAnimations;
            // button.clickable.clicked += () => t.Position(new Vector3(-10, 0, 0), 100);
            // container.Add(button);
            return container;
        }

        // public override void OnConnectEdgeToInputPort(Edge edge)
        // {
        //     if (!edge.IsInputKey(0))
        //         _choiceDialogueNodeObject.choices[edge.input.GetID(Direction.Input) - choiceStartPos]
        //             .overriddenDialogueNode = (edge.output.node as ZNodeView).ZNode as PredicateDialogueNodeObject;
        // }
        //
        // public override void OnConnectEdgeToOutputPort(Edge edge)
        // {
        //     edge.IsOutputKey(1,
        //         () =>
        //         {
        //             _choiceDialogueNodeObject._sequenceChild = (edge.input.node as ZNodeView).ZNode as SequentialDialogueNodeObject;
        //         });
        //     if (!edge.IsOutputKey(1))
        //     {
        //         _choiceDialogueNodeObject.choices[edge.output.GetID(Direction.Output) - choiceStartPos].output = (edge.input.node as ZNodeView).ZNode as SequentialDialogueNodeObject;
        //     }
        // }
        //
        // public override void OnDisconnectEdgeFromInputPort(Edge edge)
        // {
        //     if (!edge.IsInputKey(0))
        //         _choiceDialogueNodeObject.choices[edge.input.GetID(Direction.Input) - choiceStartPos]
        //             .overriddenDialogueNode = null;
        // }
        //
        // public override void OnDisconnectEdgeFromOutputPort(Edge edge)
        // {
        //     edge.IsOutputKey(1, () => { _choiceDialogueNodeObject._sequenceChild = null; });
        //     
        //     if (!edge.IsOutputKey(1))
        //     {
        //         _choiceDialogueNodeObject.choices[edge.output.GetID(Direction.Output) - choiceStartPos].output = null;
        //     }
        // }
    }
}