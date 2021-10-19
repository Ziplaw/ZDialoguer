using System;
using System.Collections.Generic;
using UnityEngine;
using ZDialoguer.Localization;

namespace ZDialoguer
{
    public class ChoiceNodeObject : SequentialNodeObject, IEventNodeObject
    {
        public override SequentialNodeObject SequenceChild => _sequenceChild;

            [SerializeField] internal SequentialNodeObject _sequenceChild;
        public Action<ChoiceNodeObject> OnExecuteExternal;
        public LocalisedString dialogueText;

        public List<Choice> choices = new List<Choice>();

        public void Execute()
        {
            OnExecuteExternal.Invoke(this);
        }

        public void ConfirmChoice(Choice choice)
        {
            graph.GetEntryNode().Next = choice.output;
        }

        public override (LocalisedString, SequentialNodeObject) OnRetrieve()
        {
            Execute();
            return (dialogueText, SequenceChild);
        }
    }

    [Serializable]
    public class Choice
    {
        public Choice(LocalisedString choiceText)
        {
            this.choiceText = choiceText;
            visibility = DisabledVisibility.Visible;
        }

        public enum DisabledVisibility {Hidden, Visible}
        
        
        public LocalisedString choiceText;
        public SequentialNodeObject output;
        internal bool Enabled => overriddenNode ? overriddenNode.GetPredicate() : true;

        [SerializeField] internal PredicateNodeObject overriddenNode;
        public DisabledVisibility visibility;
    }
}