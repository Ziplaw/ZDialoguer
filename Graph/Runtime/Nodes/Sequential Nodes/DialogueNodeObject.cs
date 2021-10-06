using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZDialoguer;
using ZDialoguer.Localization;

public class DialogueNodeObject : SequencialNodeObject
{
    public override SequencialNodeObject SequenceChild => connectedChild;

    public SequencialNodeObject connectedChild;
    public LocalisedString text; //Temporary until Localization System is implemented

    public new void Init(Vector2 position, ZDialogueGraph graph)
    {
        base.Init(position,graph);
    }
}
