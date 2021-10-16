using System;
using System.Collections.Generic;
using UnityEngine;
using ZDialoguer.Localization;

namespace ZDialoguer
{
    public class ChoiceNodeObject : SequentialNodeObject, IEventNodeObject
    {
        public override SequentialNodeObject SequenceChild
        {
            get
            {
                var child = _sequenceChild;
                _sequenceChild = null;
                return child;
            }
        }

        [SerializeField] internal SequentialNodeObject _sequenceChild;
        public Action<ChoiceNodeObject> OnExecuteExternal;

        public List<Choice> choices = new List<Choice>();

        public void Execute()
        {
            Debug.Log("Displaying Choices");
            OnExecuteExternal?.Invoke(this);
        }

        public void ConfirmChoice(Choice choice)
        {
            Debug.Log("Choice Confirmed");
            _sequenceChild = choice.output;
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
        public bool enabled = true;

        [SerializeField] internal PredicateNodeObject overriddenNode;
        public DisabledVisibility visibility;
    }
}