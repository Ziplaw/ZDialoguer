using System;
using System.Collections.Generic;
using UnityEngine;

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

        [SerializeField] SequentialNodeObject _sequenceChild;
        public Action<ChoiceNodeObject> OnExecuteExternal;

        public List<Choice> choices;

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
        public enum DisabledVisibility {Visible, Hidden}
        
        
        public string choiceText = "Choice";
        public SequentialNodeObject output;
        public bool enabled = true;
        public DisabledVisibility visibility;
    }
}