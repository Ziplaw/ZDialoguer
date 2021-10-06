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
        
        public override void Init(Vector2 position, ZDialogueGraph graph)
        {
            base.Init(position, graph);
            fact = graph.facts[0];
        }
        
        #endif
        
    }
}
