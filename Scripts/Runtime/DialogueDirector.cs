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

    public ZDialogueGraph CurrentGraph
    {
        get => currentGraph;
        set
        {
            SetupGraph(value);
            currentGraph = value;
        }
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

    private void Start()
    {
        SetupGraph(currentGraph);
    }

    public void SetNextTo(SequentialNodeObject nodeObject)
    {
        currentGraph.GetEntryNode().Next = nodeObject;
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