using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZDialoguer.Localization;

namespace ZGraph.DialogueSystem
{


    public class DialogueNodeObject : SequentialDialogueNodeObject
    {
        public SequentialDialogueNodeObject connectedChild;
        public LocalisedString text;

        protected override void OnCreate()
        {
            base.OnCreate();
            
            text = new LocalisedString(true);
            if (DialogueGraph.dialogueText) return;
            throw new NodeNotCreatedException("Dialogue text asset in graph is null");
        }
    }
}