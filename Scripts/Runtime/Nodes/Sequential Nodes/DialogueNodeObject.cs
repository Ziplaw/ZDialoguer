using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZDialoguer;

public class DialogueNodeObject : SequencialNodeObject
{
    public override NodeObject SequenceChild { get; }

    public NodeObject connectedChild;
    public string text; //Temporary until Localization System is implemented

    public new void Init(Vector2 position, ZDialogueGraph graph)
    {
        base.Init(position,graph);
    }
}
