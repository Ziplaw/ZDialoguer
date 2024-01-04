using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZGraph;
using ZDialoguer.Localization;
using ZDialoguerEditor;
using Node = ZGraph.Node;

public class ZGraphView : GraphView
{
    public new class UxmlFactory : UxmlFactory<ZGraphView, UxmlTraits>
    {
    }

    public Action<NodeView> OnNodeSelected;
    public Action<NodeView> OnNodeDeselected;
    
    internal ZGraph.Graph graph;
    internal ZNodeSearchWindow _nodeSearchWindow;
    internal ZDialogueGraphEditorWindow _editorWindow;

    public ZGraphView()
    {
        Insert(0, new GridBackground());

        var zoomer = new ContentZoomer();
        zoomer.maxScale = 2;
        
        this.AddManipulator(zoomer);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
        RegisterCallback<DragPerformEvent>(OnDragPerformEvent);

        var styleSheet =
            Resources.Load<StyleSheet>(
                "UXML/ZDialogueGraphEditorWindow");
        styleSheets.Add(styleSheet);
    }

    public void PopulateView(ZGraph.Graph graph)
    {
        foreach (var node in nodes)
        {
            node?.parent?.Clear();
        }
        foreach (var edge in edges)
        {
            edge?.parent?.Clear();
        }
        
        this.graph = graph;

        graph.OnPopulate(new Vector2(resolvedStyle.width, resolvedStyle.height));
        AddSearchWindow();
        // GenerateBlackBoard();
        graph.Nodes.ForEach(n => CreateNodeView(n));
        RestoreConnections();
    }

    private void RestoreConnections()
    {
        List<Edge> edgesToCreate = new List<Edge>();
        foreach (var graphEdgeData in graph.edgeDatas)
        {
            Port input = GetPortByGuid(graphEdgeData.inputPortID);
            Port output = GetPortByGuid(graphEdgeData.outputPortID);

            if (output == null || input == null) continue;
            Edge edge = output.ConnectTo(input);
            AddElement(edge);
            edgesToCreate.Add(edge);
        }
        
        graphViewChanged = (change) => OnGraphViewChanged(change,false);
        OnGraphViewChanged(new GraphViewChange { edgesToCreate = edgesToCreate },true);
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);
        evt.menu.AppendAction("Refresh", (a) => PopulateView(graph));
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList()
            .Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node &&
                              startPort.portType == endPort.portType).ToList();
    }

    private void AddSearchWindow()
    {
        _nodeSearchWindow = ScriptableObject.CreateInstance<ZNodeSearchWindow>();
        _nodeSearchWindow.graphView = this;
        nodeCreationRequest = context =>
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _nodeSearchWindow);
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange, bool populating)
    {
        if (graphViewChange.elementsToRemove != null)
        {
            for (var i = 0; i < graphViewChange.elementsToRemove.Count; i++)
            {
                var e = graphViewChange.elementsToRemove[i];

                switch (e)
                {
                    case NodeView nV:
                        DeleteNode(nV.Node);
                        edges.Where(edge => edge.input.node == nV || edge.output.node == nV).ToList().ForEach(edge =>
                        {
                            graph.RemoveEdge(edge.input.viewDataKey, edge.output.viewDataKey);
                            (edge.output.node as NodeView).OnDisconnectEdgeFromOutputPort(edge, populating);
                            (edge.input.node as NodeView).OnDisconnectEdgeFromInputPort(edge, populating);
                        });
                        break;
                    case Edge edge:
                        graph.RemoveEdge(edge.input.viewDataKey, edge.output.viewDataKey);
                        (edge.output.node as NodeView).OnDisconnectEdgeFromOutputPort(edge, populating);
                        (edge.input.node as NodeView).OnDisconnectEdgeFromInputPort(edge, populating);
                        break;
                }
            }
        }

        if (graphViewChange.edgesToCreate != null)
        {
            foreach (var edge in graphViewChange.edgesToCreate)
            {
                NodeView outputNodeView = edge.output.node as NodeView;
                NodeView inputNodeView = edge.input.node as NodeView;
                graph.AddEdge(edge.input.viewDataKey, edge.output.viewDataKey);
                outputNodeView.OnConnectEdgeToOutputPort(edge, populating);
                inputNodeView.OnConnectEdgeToInputPort(edge, populating);
            }
        }

        SaveChangesToObject(graph);

        return graphViewChange;
    }

    public static void SaveChangesToObject(ScriptableObject graph)
    {
        EditorUtility.SetDirty(graph);
        AssetDatabase.SaveAssets();
    }

    void OnDragUpdatedEvent(DragUpdatedEvent e)
    {
        if (DragAndDrop.GetGenericData("DragSelection") is List<ISelectable> selection &&
            (selection.OfType<BlackboardField>().Count() >= 0))
        {
            DragAndDrop.visualMode = e.actionKey ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Move;
        }
    }

    void OnDragPerformEvent(DragPerformEvent e)
    {
        throw new NotImplementedException();
        var selection = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
        // IEnumerable<FactBlackboardField> fields = selection.OfType<FactBlackboardField>();
        // foreach (FactBlackboardField field in fields)
        // {
        //     var node = CreateNode<FactDialogueNode>(TransformMousePosition(e.localMousePosition), false);
        //     (node as FactDialogueNode).factIndex = field.factIndex;
        //     CreateNodeView(node); // creating node here because we gotta assign factNodeObject first hehe;
        // }

        SaveChangesToObject(graph);
    }

    internal Vector2 TransformMousePosition(Vector2 eventLocalMousePosition)
    {
        return viewTransform.matrix.inverse.MultiplyPoint(eventLocalMousePosition);
    }

    internal Node CreateNode(Type type, Vector2 position, bool generateView = true)
    {
        var node = ScriptableObject.CreateInstance(type) as Node;
        if (node.OnCreate(position, graph))
        {
            if (generateView) CreateNodeView(node);
            return node;
        }
        return null;
    }
    
    public void DeleteNode(Node nodeObject)
    {
        graph.Nodes.Remove(nodeObject);
        AssetDatabase.RemoveObjectFromAsset(nodeObject);
        AssetDatabase.SaveAssets();
    }

    NodeView CreateNodeView(Node node)
    {
        NodeView nodeView = NodeView.CreateNodeView(node, this);
        AddElement(nodeView);
        return nodeView;
    }
}