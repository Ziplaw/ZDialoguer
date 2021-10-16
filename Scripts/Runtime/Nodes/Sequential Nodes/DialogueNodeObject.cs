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
        return (text, SequenceChild);
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
