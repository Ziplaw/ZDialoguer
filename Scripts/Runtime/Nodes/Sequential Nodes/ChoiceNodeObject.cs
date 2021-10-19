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
            graph.GetEntryNode().Current = choice.output;
        }

        public override (LocalisedString, SequentialNodeObject) OnRetrieve()
        {
            Execute();
            return (dialogueText, SequenceChild);
        }

        public override NodeObject DeepClone()
        {
            ChoiceNodeObject instance = (ChoiceNodeObject)graph.GetOrCreateNodeInstance(this);
            instance._sequenceChild = (SequentialNodeObject)graph.GetOrCreateNodeInstance(instance._sequenceChild);
            instance.choices.ForEach(c =>
            {
                c.overriddenNode = (PredicateNodeObject)graph.GetOrCreateNodeInstance(c.overriddenNode);
                c.output = (SequentialNodeObject)graph.GetOrCreateNodeInstance(c.output);
            });
            return instance;
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