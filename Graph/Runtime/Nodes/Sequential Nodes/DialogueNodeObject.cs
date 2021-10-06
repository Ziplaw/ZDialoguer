using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZDialoguer;
using ZDialoguer.Localization;

public class DialogueNodeObject : SequencialNodeObject
{
    public override SequencialNodeObject SequenceChild => connectedChild;

    public SequencialNodeObject connectedChild;
    public LocalisedString text;

    public override void Init(Vector2 position, ZDialogueGraph graph)
    {
        base.Init(position,graph);
        text = new LocalisedString(true);
    }
}
