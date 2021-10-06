using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;

public class ZDialoguerGraphView : GraphView
{
    public new class UxmlFactory : UxmlFactory<ZDialoguerGraphView, UxmlTraits>
    {
    }

    public Action<NodeView> OnNodeSelected;
    public Action<Fact> OnBlackboardFactSelected;
    private ZDialogueGraph graph;
    private Blackboard _blackBoard;

    public ZDialoguerGraphView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
        RegisterCallback<DragPerformEvent>(OnDragPerformEvent);

        var styleSheet =
            AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/com.Ziplaw.ZDialoguer/Graph/Editor/ZDialogueGraphEditorWindow.uss");
        styleSheets.Add(styleSheet);
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        var localMousePos = evt.localMousePosition;
        //base.BuildContextualMenu(evt);
        var types = TypeCache.GetTypesDerivedFrom<NodeObject>().Where(t => t != typeof(GraphStartNodeObject) && t.BaseType != typeof(NodeObject));
        foreach (var type in types)
        {
            if (type == typeof(FactNodeObject) && graph.facts.Count < 1) continue;
            evt.menu.AppendAction($"{type.Name}", a =>
            {
                if (graph)
                {
                    CreateNode(type, TransformMousePosition(localMousePos));
                }
                else
                {
                    Debug.Log("No Graph Selected");
                }
            });
        }
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList()
            .Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node &&
                              startPort.portType == endPort.portType).ToList();
    }

    NodeView FindNodeView(NodeObject nodeObject)
    {
        return GetNodeByGuid(nodeObject.guid) as NodeView;
    }

    public void PopulateView(ZDialogueGraph graph)
    {
        this.graph = graph;
        graphViewChanged -= OnGraphViewChanged;
        graph.Init();
        DeleteElements(graphElements.ToList());
        graphViewChanged += OnGraphViewChanged;


        graph.nodes.ForEach(n => CreateNodeView(n));
        RestoreConnections();
        GenerateBlackBoard();
    }

    private void RestoreConnections()
    {
        List<Edge> edgesToCreate = new List<Edge>();
        foreach (var graphEdgeData in graph.edgeDatas)
        {
            Port input = GetPortByGuid(graphEdgeData.inputPortViewDataKey);
            Port output = GetPortByGuid(graphEdgeData.outputPortViewDataKey);
            
            Edge edge = output.ConnectTo(input);
            AddElement(edge);
            edgesToCreate.Add(edge);
        }

        graphViewChanged.Invoke(new GraphViewChange() { edgesToCreate = edgesToCreate });
    }

    void GenerateBlackBoard()
    {
        if (_blackBoard != null)
        {
            Remove(_blackBoard);
        }


        var bb = new Blackboard(this);
        bb.Add(new BlackboardSection() { title = "Facts" });
        bb.addItemRequested += AddFactToBlackBoard;
        bb.editTextRequested += EditFactText;

        bb.RegisterCallback<GeometryChangedEvent>(e => GeometryChangedCallback(bb));


        Add(bb);
        _blackBoard = bb;

        PopulateBlackboardWithFacts();
    }

    private void GeometryChangedCallback(Blackboard blackboard)
    {
        blackboard.UnregisterCallback<GeometryChangedEvent>(evt1 => GeometryChangedCallback(blackboard));
        blackboard.SetPosition(new Rect(new Vector2(resolvedStyle.width - 300, 0), new Vector2(300, 300)));

    }

    private void EditFactText(Blackboard bb, VisualElement field, string value)
    {
        string newName = FixNewFactName(value);
        var _field = (FactBlackboardField) field;
        _field.fact.nameID = newName;
        _field.fact.name = newName;
        _field.text = newName;
        _field.name = newName;
        //Implement this into the Fact Field itself
        SaveChangesToGraph(graph);
    }

    void PopulateBlackboardWithFacts()
    {
        graph.facts.ForEach(f => { _blackBoard.Add(GenerateFactContainer(f)); });
    }

    string FixNewFactName(string newName)
    {
        int appender = 1;
        if (graph.facts.Count != 0)
        {
            while (graph.facts.Any(f => f.nameID == newName))
            {
                if (newName.Contains($"({appender - 1})"))
                {
                    newName = newName.Replace($"({appender - 1})", $"({appender})");
                }
                else
                {
                    newName += $" ({appender})";
                }

                appender++;
            }
        }

        return newName;
    }

    private void AddFactToBlackBoard(Blackboard blackboard)
    {
        var newFact = graph.CreateFact(FixNewFactName("New Fact"), 0);
        blackboard.Add(GenerateFactContainer(newFact));

        SaveChangesToGraph(graph);
    }

    VisualElement GenerateFactContainer(Fact fact)
    {
        var container = new VisualElement();
        var bbField = new FactBlackboardField(fact)
            { text = fact.nameID, typeText = "float", OnBlackboardFactSelected = OnBlackboardFactSelected };

        container.Add(bbField);

        return container;
    }


    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {

        if (graphViewChange.elementsToRemove != null)
        {
            graphViewChange.elementsToRemove.ForEach(e =>
            {
                NodeView nV = e as NodeView;
                if (nV != null)
                {
                    graph.DeleteNode(nV.NodeObject);
                }

                Edge edge = e as Edge;
                if (edge != null)
                {
                    (edge.output.node as NodeView).OnDisconnectEdgeFromOutputPort(edge); 
                    (edge.input.node as NodeView).OnDisconnectEdgeFromInputPort(edge); 
                    graph.edgeDatas.RemoveEdge(edge);
                }

                FactBlackboardField factBlackboardField = e as FactBlackboardField;
                if (factBlackboardField != null)
                {
                    graph.DeleteFact(factBlackboardField.fact);
                }
            });
        }

        if (graphViewChange.edgesToCreate != null)
        {
            graphViewChange.edgesToCreate.ForEach(e =>
            {
                (e.output.node as NodeView).OnConnectEdgeToOutputPort(e); 
                (e.input.node as NodeView).OnConnectEdgeToInputPort(e); 
                graph.edgeDatas.AddEdge(e);
            });
        }

        SaveChangesToGraph(graph);

        return graphViewChange;
    }

    public static void SaveChangesToGraph(ZDialogueGraph graph)
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
        var selection = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
        IEnumerable<FactBlackboardField> fields = selection.OfType<FactBlackboardField>();
        foreach (FactBlackboardField field in fields)
        {
            var node = CreateNode<FactNodeObject>(TransformMousePosition(e.localMousePosition), false); 
            (node as FactNodeObject).fact = field.fact;
            CreateNodeView(node); // creating node here because we gotta assign fact first hehe;
        }
        
        SaveChangesToGraph(graph);
    }

    Vector2 TransformMousePosition(Vector2 eventLocalMousePosition)
    {
        return viewTransform.matrix.inverse.MultiplyPoint(eventLocalMousePosition);
    }

    NodeObject CreateNode<T>(Vector2 position, bool generateView = true) where T : NodeObject
    {
        return CreateNode(typeof(T), position, generateView);
    }
    
    NodeObject CreateNode(Type type, Vector2 position, bool generateView = true)
    {
        var node = ScriptableObject.CreateInstance(type) as NodeObject;
        node.Init(position, graph);
        if(generateView) CreateNodeView(node);
        return node;
    }

    NodeView CreateNodeView(NodeObject nodeObject)
    {
        NodeView nodeView = NodeView.CreateNodeView(nodeObject, graph);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
        return nodeView;
    }
}