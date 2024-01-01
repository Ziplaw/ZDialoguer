using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ZDialoguer.Localization;

namespace ZGraph.DialogueSystem
{
    public class ChoiceDialogueNodeObject : SequentialDialogueNodeObject, IEventNodeObject
    {
        [SerializeField] internal SequentialDialogueNodeObject _sequenceChild;
        public Action<ChoiceDialogueNodeObject> OnExecuteExternal;
        public LocalisedString dialogueText;

        public List<Choice> choices = new List<Choice>();

        public void Execute()
        {
            OnExecuteExternal.Invoke(this);
        }

        public void ConfirmChoice(Choice choice)
        {
            throw new NotImplementedException();
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
        public SequentialDialogueNodeObject output;
        internal bool Enabled => overriddenDialogueNode ? overriddenDialogueNode.GetPredicate() : true;

        [FormerlySerializedAs("overriddenNode")] [SerializeField] internal PredicateDialogueNodeObject overriddenDialogueNode;
        public DisabledVisibility visibility;
    }
}