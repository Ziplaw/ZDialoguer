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

    public void RequestDialogue(ZDialogueGraph graph, params FactData[] factDatas)
    {
        currentGraph = ProcessGraph(graph);
        SetupGraphExternal(currentGraph);
        currentGraph.Setup();
        currentGraph.InitializeFacts(factDatas);

        OnRequestDialogue?.Invoke();
        GetNextText();
    }

    private ZDialogueGraph ProcessGraph(ZDialogueGraph graph)
    {
        var cloneGraph = Instantiate(graph);

        var nodeMap = cloneGraph.nodes.ToDictionary(n => n, n => Instantiate(n));
        var factMap = cloneGraph.facts.ToDictionary(fact => fact, fact => Instantiate(fact));
        
        cloneGraph.nodes = nodeMap.Select(n => n.Value).ToList();
        cloneGraph.facts = factMap.Select(f => f.Value).ToList();
        
        foreach (var o in cloneGraph.nodes.Where(n => n is ChoiceNodeObject))
        {
            var choiceNodeObject = (ChoiceNodeObject)o;
            foreach (var choice in choiceNodeObject.choices)
            {
                choice.overriddenNode = (PredicateNodeObject)(choice.overriddenNode ? nodeMap[choice.overriddenNode] : null);
                choice.output = (SequentialNodeObject)(choice.output ? nodeMap[choice.output] : null);
            }
        }

        DeepCloneReferences(ref cloneGraph.nodes, nodeMap);
        DeepCloneReferences(ref cloneGraph.facts, factMap);

        return cloneGraph;
    }

    void DeepCloneReferences<T>(ref List<T> list, Dictionary<T, T> map) where T : ScriptableObject
    {
        foreach (var clonedMember in list)
        {
            foreach (var fieldInfo in clonedMember.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => typeof(T).IsAssignableFrom(f.FieldType)))
            {
                var memberReference = fieldInfo.GetValue(clonedMember);
                if (memberReference != null)
                {
                    if (map.ContainsKey((T)memberReference))
                        fieldInfo.SetValue(clonedMember, map[(T)memberReference]);
                    else if(map.ContainsValue((T)memberReference))
                        fieldInfo.SetValue(clonedMember, (T)memberReference);
                }
            }
        }
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