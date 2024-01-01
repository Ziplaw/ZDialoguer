using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;
using ZDialoguer.Localization;
using ZDialoguer.Localization.Editor;

namespace ZGraph.DialogueSystem
{
    public class DialogueZNodeView : SequentialZNodeView
    {
        public override void BuildNodeView(ZNode Node, ZGraph graph)
        {
            base.BuildNodeView(Node, graph);
            int index = 0;
            var dialogueNodeObject = Node as DialogueNodeObject;
            CreateInputPort(typeof(SequentialDialogueNodeObject), "►", inputContainer, Node, ref index,
                Port.Capacity.Multi);
            title = "Dialogue Node";
            CreateOutputPort(typeof(SequentialDialogueNodeObject), "►", outputContainer, Node, ref index,
                Port.Capacity.Single);
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.58f, 0.96f, 0.68f));

            dialogueNodeObject.text.csvFile = ((ZDialogueGraph)graph).dialogueText;
            dialogueNodeObject.text.csvFileFullAssetPath =
                LocalisedString.GetFullAssetPath(dialogueNodeObject.text.csvFile);

            var propertyDrawer =
                new LocalisedStringPropertyDrawer
                    { ZNodeView = this, _container = extensionContainer, _containerPosition = 0 }.CreatePropertyGUI(
                    new SerializedObject(Node).FindProperty("text"));

            extensionContainer.Add(propertyDrawer);

            ForceCollapsable();
        }

        // public override void OnConnectEdgeToOutputPort(Edge edge)
        // {
        //     throw new NotImplementedException();
        //     edge.IsInputKey(0,
        //         () => (ZNode as DialogueNodeObject).connectedChild =
        //             (edge.input.node as ZNodeView).ZNode as SequentialDialogueNodeObject);
        // }
        //
        // public override void OnDisconnectEdgeFromOutputPort(Edge edge)
        // {
        //     throw new NotImplementedException();
        //     edge.IsInputKey(0, () => (ZNode as DialogueNodeObject).connectedChild = null);
        // }
        //
        // public override void OnConnectEdgeToInputPort(Edge edge)
        // {
        // }
        //
        // public override void OnDisconnectEdgeFromInputPort(Edge edge)
        // {
        // }
    }
}