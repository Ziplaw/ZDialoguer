using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using ZDialoguer;
using TMPro;
using UnityEditor.Graphs;

public class DialogueDirector : MonoBehaviour
{
    // public List<ZDialogueGraph> graphs;
    public TextMeshProUGUI textComponent;
    [SerializeField] ZDialogueGraph currentGraph;
    public UnityEvent<ChoiceNodeObject> OnGetChoice;
    public UnityEvent OnRequestDialogue;
    public UnityEvent OnEndDialogue;

    private void Start()
    {
        StupidCheck();
    }

    private void StupidCheck()
    {
        foreach (var type in typeof(ZDialogueGraph).GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => typeof(ScriptableObject).IsAssignableFrom(f.FieldType.GenericTypeArguments?.FirstOrDefault()))
            .Select(f => f.FieldType.GenericTypeArguments?.FirstOrDefault()))
        {
            if (type != typeof(Fact) && type != typeof(NodeObject))
                Debug.LogWarning($"Missing {type} for deep cloning!");
        }
    }

    public void EndDialogue()
    {
        OnEndDialogue.Invoke();
    }

    private void SetupGraphExternal(ZDialogueGraph graph)
    {
        foreach (var o in graph.nodes)
        {
            switch (o)
            {
                case ChoiceNodeObject choiceNodeObject:
                    choiceNodeObject.OnExecuteExternal = OnGetChoice.Invoke;
                    break;
                case ExitNodeObject exitNodeObject:
                    exitNodeObject.OnExitDialogue = EndDialogue;
                    break;
            }
        }
    }

    public void RequestDialogue(ZDialogueGraph graph, params FactData[] context)
    {
        currentGraph = ProcessGraph(graph);
        currentGraph.Setup();
        SetupGraphExternal(currentGraph);
        currentGraph.InitializeFacts(context);

        OnRequestDialogue?.Invoke();
        GetNextText();
    }

    private ZDialogueGraph ProcessGraph(ZDialogueGraph graph)
    {
        graph.factInstanceMap.Clear();
        graph.nodeObjectMap.Clear();
        
        var cloneGraph = Instantiate(graph);
        
        cloneGraph.nodes = cloneGraph.nodes.Select(n => n.DeepClone()).ToList();
        cloneGraph.facts = cloneGraph.facts.Select(f => cloneGraph.GetOrCreateFactInstance(f)).ToList();

        return cloneGraph;
    }

    public void GetNextText()
    {
        string parsedText = currentGraph.GetEntryNode().GetNextText();
        if (!string.IsNullOrEmpty(parsedText)) ComposeText(parsedText);
    }

    async void ComposeText(string parsedText)
    {
        textComponent.text = "";
        while (textComponent.text.Length < parsedText.Length)
        {
            textComponent.text += parsedText[textComponent.text.Length];
            await Task.Yield();
        }
    }
}

public class FactData
{
    public string nameID;
    public object value;

    public FactData(string nameID, object value)
    {
        this.nameID = nameID;
        this.value = value;
    }
}