using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZDialoguer;
using ZDialoguer.Localization;
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
    private bool repopulating;

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
            Resources.Load<StyleSheet>(
                "UXML/ZDialogueGraphEditorWindow");
        styleSheets.Add(styleSheet);
    }
    
    public void PopulateView(ZDialogueGraph graph)
    {
        repopulating = true;
        this.graph = graph;
        graphViewChanged -= OnGraphViewChanged;
        _editorWindow.rootVisualElement.schedule
            .Execute(() =>
            {
                if (!graph.initialized)
                {
                    graph.Init(new Vector2(resolvedStyle.width * .25f, resolvedStyle.height * .25f));
                    PopulateView(graph);
                }
            })
            .StartingIn(0);

        DeleteElements(graphElements.ToList());
        graphViewChanged += OnGraphViewChanged;

        AddSearchWindow();
        GenerateBlackBoard();
        graph.nodes.ForEach(n => CreateNodeView(n));
        RestoreConnections();
        repopulating = false;
    }
    
    private void RestoreConnections()
    {
        List<Edge> edgesToCreate = new List<Edge>();
        foreach (var graphEdgeData in graph.edgeDatas)
        {
            Port input = GetPortByGuid(graphEdgeData.inputPortViewDataKey);
            Port output = GetPortByGuid(graphEdgeData.outputPortViewDataKey);


            if (output == null || input == null) continue;
            Edge edge = output.ConnectTo(input);
            AddElement(edge);
            edgesToCreate.Add(edge);
        }

        graphViewChanged.Invoke(new GraphViewChange { edgesToCreate = edgesToCreate });
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
        _nodeSearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _nodeSearchWindow.graphView = this;
        nodeCreationRequest = context =>
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _nodeSearchWindow);
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

        var textObjectField = new ObjectField()
            { objectType = typeof(TextAsset), style = { flexGrow = 1, flexShrink = 1 } };
        textObjectField.SetValueWithoutNotify(graph.dialogueText);
        textObjectField.RegisterValueChangedCallback(e =>
        {
            if (e.newValue == null && graph.nodes.Any(n =>
                n is DialogueNodeObject dialogueNodeObject && dialogueNodeObject.text.csvFile == e.previousValue))
            {
                Debug.LogWarning(
                    "You can't remove the Dialogue Text from the graph when there are still Dialogue Nodes referencing it!");
                textObjectField.SetValueWithoutNotify(e.previousValue);
                return;
            }

            graph.dialogueText = e.newValue as TextAsset;
            foreach (var o in graph.nodes.Where(n => n is DialogueNodeObject))
            {
                var dialogueNodeObject = (DialogueNodeObject)o;
                dialogueNodeObject.text.csvFile = e.newValue as TextAsset;
                dialogueNodeObject.text.csvFileFullAssetPath =
                    LocalisedString.GetFullAssetPath(dialogueNodeObject.text.csvFile);
                dialogueNodeObject.text.Reset();
            }

            EditorWindow.GetWindow<ZDialogueGraphEditorWindow>().graphView.PopulateView(graph);
        });
        var editButton = new Button(() =>
        {
            var editWindow = ScriptableObject.CreateInstance<LocalizationCSVEditorWindow>();
            var currentResolution = Screen.currentResolution;
            Rect r = new Rect(currentResolution.width * .5f - 1024 * .5f, currentResolution.height * .5f - 512 * .5f,
                10, 10);
            editWindow.ShowAsDropDown(r, new Vector2(1024, 512));


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
            for (var i = 0; i < graphViewChange.elementsToRemove.Count; i++)
            {
                var e = graphViewChange.elementsToRemove[i];

                switch (e)
                {
                    case NodeView nV:
                        graph.DeleteNode(nV.NodeObject);
                        break;
                    case Edge edge:
                        (edge.output.node as NodeView).OnDisconnectEdgeFromOutputPort(edge);
                        (edge.input.node as NodeView).OnDisconnectEdgeFromInputPort(edge);
                        graph.edgeDatas.RemoveEdge(edge);
                        break;
                    case FactBlackboardField factBlackboardField:
                        if (graph.nodes.Any(n =>
                            n is FactNodeObject factNodeObject && factNodeObject.fact == factBlackboardField.fact))
                        {
                            Debug.LogWarning(
                                "You can't delete this fact, as a Fact Node referencing it is present on the graph");
                            graphViewChange.elementsToRemove.Remove(factBlackboardField);
                            i--;
                        }
                        else
                        {
                            graph.DeleteFact(factBlackboardField.fact);
                        }

                        break;
                }
            }

            if (!repopulating)
            {
                schedule.Execute(() => PopulateView(graph)).StartingIn(0);
            }
        }

        if (graphViewChange.edgesToCreate != null)
        {
            foreach (var edge in graphViewChange.edgesToCreate)
            {
                NodeView outputNodeView = edge.output.node as NodeView;
                NodeView inputNodeView = edge.input.node as NodeView;
                outputNodeView.OnConnectEdgeToOutputPort(edge);
                inputNodeView.OnConnectEdgeToInputPort(edge);
                graph.edgeDatas.AddEdge(edge);
            }

            if (!repopulating)
            {
                schedule.Execute(() => PopulateView(graph)).StartingIn(0);
            }
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
        if (node.Init(position, graph))
        {
            if (generateView) CreateNodeView(node);
            return node;
        }

        graph.DeleteNode(node);
        return null;
    }

    NodeView CreateNodeView(NodeObject nodeObject)
    {
        NodeView nodeView = NodeView.CreateNodeView(nodeObject, this);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
        return nodeView;
    }

    
}