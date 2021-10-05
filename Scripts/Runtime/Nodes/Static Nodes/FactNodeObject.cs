using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZDialoguer
{
    public class FactNodeObject : StaticNodeObject
    {
        public Fact fact;
        
        #if UNITY_EDITOR
        
        public void Init(Fact fact, Vector2 position, ZDialogueGraph graph)
        {
            Init(position, graph);
            this.fact = fact;
        }
        
        #endif
        
    }
}
