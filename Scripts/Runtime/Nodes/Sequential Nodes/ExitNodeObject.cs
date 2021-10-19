using System;
using UnityEngine;
using ZDialoguer.Localization;

namespace ZDialoguer
{
    public class ExitNodeObject : SequentialNodeObject
    {
        public override SequentialNodeObject SequenceChild { get; }
        public Action OnExitDialogue;
        public override (LocalisedString, SequentialNodeObject) OnRetrieve()
        {
            OnExitDialogue.Invoke();
            return (null, null);
        }
    }
}