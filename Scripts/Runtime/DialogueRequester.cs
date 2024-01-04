using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZGraph.DialogueSystem;

public class DialogueRequester : MonoBehaviour
{
    public DialogueGraph graph;
    public string playerName;
    
    public void RequestDialogue()
    {
        FindObjectOfType<DialogueDirector>().RequestDialogue(graph, new FactData("playerName", playerName));
    }
}
