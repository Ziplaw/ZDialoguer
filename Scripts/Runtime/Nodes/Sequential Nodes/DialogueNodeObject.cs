using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZDialoguer;
using ZDialoguer.Localization;

public class DialogueNodeObject : SequentialNodeObject
{
    public override SequentialNodeObject SequenceChild => connectedChild;

    public SequentialNodeObject connectedChild;
    public LocalisedString text;

    public override (LocalisedString, SequentialNodeObject) OnRetrieve()
    {
        text.GenerateOutput();
        return (text, SequenceChild);
    }

    public override NodeObject DeepClone()
    {
        DialogueNodeObject instance = (DialogueNodeObject)graph.GetOrCreateNodeInstance(this);
        instance.connectedChild = (SequentialNodeObject)graph.GetOrCreateNodeInstance(connectedChild);
        return instance;
    }

    public override bool Init(Vector2 position, ZDialogueGraph graph)
    {
        base.Init(position,graph);
        text = new LocalisedString(true);
        if (graph.dialogueText) return true;
        Debug.LogWarning("No Localization Text Asset Selected");
        return false;
    }
}
