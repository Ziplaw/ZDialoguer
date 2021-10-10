using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;
using ZDialoguerEditor;

public class GraphStartNodeView : SequentialNodeView
{
    public override void BuildNodeView(NodeObject nodeObject, ZDialogueGraph graph)
    {
        int index = 0;
        GraphStartNodeObject startNodeObject = nodeObject as GraphStartNodeObject;
        capabilities ^= Capabilities.Deletable;
        base.BuildNodeView(nodeObject, graph);
        title = "Entry";
        titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.57f, 0.96f));
        mainContainer.style.alignItems = Align.Center;

        Button testButton = new Button(() =>
        {
            var current = startNodeObject.Next;
            var graphView = EditorWindow.GetWindow<ZDialogueGraphEditorWindow>().graphView;
            while (current != null)
            {
                while (!(current as DialogueNodeObject))
                {
                    current = startNodeObject.Next;
                }

                graphView.AddToSelection(graphView.GetNodeByGuid(current.guid));
                Debug.Log((current as DialogueNodeObject).text.ParseFacts(graph));
                current = startNodeObject.Next;
            }

            startNodeObject.Next = startNodeObject;
        }) { text = "Test Graph" };

        mainContainer.Add(testButton);
        CreateOutputPort(typeof(SequentialNodeObject), "", mainContainer, nodeObject, ref index, Port.Capacity.Single, Orientation.Vertical);
        startNodeObject.Next = startNodeObject;
    }

    public override void OnConnectEdgeToInputPort(Edge edge)
    {
    }

    public override void OnConnectEdgeToOutputPort(Edge edge)
    {
        (NodeObject as GraphStartNodeObject).childNodeObject =
            (edge.input.node as NodeView).NodeObject as SequentialNodeObject;
    }

    public override void OnDisconnectEdgeFromInputPort(Edge edge)
    {
    }

    public override void OnDisconnectEdgeFromOutputPort(Edge edge)
    {
        (NodeObject as GraphStartNodeObject).childNodeObject = null;
    }
}