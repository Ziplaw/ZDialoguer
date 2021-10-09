using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;
using ZDialoguerEditor;

public class ZDialoguerGraphView : GraphView
{
    public new class UxmlFactory : UxmlFactory<ZDialoguerGraphView, UxmlTraits>
    {
    }

    public Action<NodeView> OnNodeSelected;
    public Action<Fact> OnBlackboardFactSelected;
    internal ZDialogueGraph graph;
    internal DialogueBlackboard _blackBoard;
    internal NodeSearchWindow _nodeSearchWindow;
    internal ZDialogueGraphEditorWindow _editorWindow;

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

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList()
            .Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node &&
                              startPort.portType == endPort.portType).ToList();
    }

    public void PopulateView(ZDialogueGraph graph)
    {
        this.graph = graph;
        graphViewChanged -= OnGraphViewChanged;
        graph.Init();
        DeleteElements(graphElements.ToList());
        graphViewChanged += OnGraphViewChanged;

        AddSearchWindow();
        GenerateBlackBoard();
        graph.nodes.ForEach(n => CreateNodeView(n));
        RestoreConnections();
    }

    private void AddSearchWindow()
    {
        _nodeSearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _nodeSearchWindow.graphView = this;
        nodeCreationRequest = context =>
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _nodeSearchWindow);
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

        var bb = new DialogueBlackboard(this);
        
        Add(bb);
        _blackBoard = bb;

    }

    internal void InitDialogueTextBlackboard(BlackboardSection section)
    {
        section.Q<TemplateContainer>().style.flexGrow = 1;
        section.Q<TemplateContainer>().Q<VisualElement>("rowsContainer").style.flexDirection = FlexDirection.Row;
        
        var textObjectField = new ObjectField() { objectType = typeof(TextAsset), style = { flexGrow = 1, flexShrink = 1}};
        textObjectField.SetValueWithoutNotify(graph.dialogueText);
        textObjectField.RegisterValueChangedCallback(e => graph.dialogueText = e.newValue as TextAsset);
        var editButton = new Button(() =>
        {
            var editWindow = ScriptableObject.CreateInstance<LocalizationCSVEditorWindow>();
            Vector2 mouse = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            var currentResolution = Screen.currentResolution;
            Rect r = new Rect(currentResolution.width*.5f - 1024*.5f, currentResolution.height*.5f - 512*.5f, 10, 10);
            editWindow.ShowAsDropDown(r, new Vector2(1024, 512));
            
            
            // LocalizationSearchWindow window = CreateInstance<LocalizationSearchWindow>();
            // window.titleContent = new GUIContent("Localisation Search");
            // _property = property;
            // _localisedTextBox = localisedTextBox;
            // Vector2 mouse = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            // Rect r = new Rect(mouse.x - 450, mouse.y + 10, 10, 10);
            // window.ShowAsDropDown(r, new Vector2(500, 300));
            
            editWindow.rootVisualElement.Q<ObjectField>().SetValueWithoutNotify(textObjectField.value);
            
            editWindow.csvFileAssetPath = editWindow.GetTextAssetFullPath(textObjectField.value as TextAsset);

            editWindow.GenerateTableMenu(editWindow.csvFileAssetPath, editWindow.rootVisualElement.Q<ScrollView>());
        });

        editButton.style.backgroundImage = Resources.Load<Texture2D>("Icons/edit");
        editButton.style.width = 24;
        editButton.style.maxWidth = 24;
        editButton.style.height = 24;
        editButton.style.flexGrow = 1;
        editButton.style.flexShrink = 1;
        section.style.flexDirection = FlexDirection.Row;
        section.Add(textObjectField);
        section.Add(editButton);
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

    internal Vector2 TransformMousePosition(Vector2 eventLocalMousePosition)
    {
        return viewTransform.matrix.inverse.MultiplyPoint(eventLocalMousePosition);
    }

    internal NodeObject CreateNode<T>(Vector2 position, bool generateView = true) where T : NodeObject
    {
        return CreateNode(typeof(T), position, generateView);
    }
    
    internal NodeObject CreateNode(Type type, Vector2 position, bool generateView = true)
    {
        var node = ScriptableObject.CreateInstance(type) as NodeObject;
        node.Init(position, graph);
        if(generateView) CreateNodeView(node);
        return node;
    }

    NodeView CreateNodeView(NodeObject nodeObject)
    {
        NodeView nodeView = NodeView.CreateNodeView(nodeObject, this);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
        return nodeView;
    }
}