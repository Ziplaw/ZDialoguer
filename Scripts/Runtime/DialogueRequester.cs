using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZDialoguer;

public class DialogueRequester : MonoBehaviour
{
    public ZDialogueGraph graph;
    public string playerName;
    
    public void RequestDialogue()
    {
        FindObjectOfType<DialogueDirector>().RequestDialogue(graph, new FactData("playerName", playerName));
    }
}
