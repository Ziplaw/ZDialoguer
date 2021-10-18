using System;
using System.Linq;
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


    public void EndDialogue()
    {
        
        OnEndDialogue.Invoke();
    }

    private void SetupGraph(ZDialogueGraph graph)
    {
        if (graph)
        {
            foreach (ChoiceNodeObject nodeObject in graph.nodes.Where(n => n is ChoiceNodeObject))
            {
                nodeObject.OnExecuteExternal = OnGetChoice.Invoke;
            }
        }
    }

    public void RequestDialogue(ZDialogueGraph graph, params FactData[] factDatas)
    {
        currentGraph = ProcessGraph(graph);
        currentGraph.SetupGraph();
        currentGraph.InitializeFacts(factDatas);
        
        OnRequestDialogue?.Invoke();
        GetNextText();
    }

    private ZDialogueGraph ProcessGraph(ZDialogueGraph graph)
    {
        var cloneGraph = Instantiate(graph);
        cloneGraph.nodes = cloneGraph.nodes.Select(n => Instantiate(n)).ToList();
        cloneGraph.facts = cloneGraph.facts.Select(f => Instantiate(f)).ToList();
        return cloneGraph;
    }

    public void GetNextText()
    {
        string parsedText = currentGraph.GetEntryNode().GetNextText(currentGraph);
        if (!string.IsNullOrEmpty(parsedText))
            ComposeText(parsedText);
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