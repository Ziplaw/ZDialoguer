using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZDialoguer
{
    public class GraphStartNodeObject : SequencialNodeObject
    {
        public override SequencialNodeObject SequenceChild => childNodeObject;
        public SequencialNodeObject childNodeObject;
        [SerializeField] SequencialNodeObject _next;//
        
        public SequencialNodeObject Next
        {
            get
            {
                _next = _next.SequenceChild;
                return _next;
            }
            internal set { _next = value; }
        }
    }
}