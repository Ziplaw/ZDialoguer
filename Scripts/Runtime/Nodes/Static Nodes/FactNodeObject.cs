using System.Collections.Generic;
using UnityEngine;

namespace ZDialoguer
{
    public class FactNodeObject : StaticNodeObject
    {
        public Fact fact;

        public override NodeObject DeepClone()
        {
            FactNodeObject instance = (FactNodeObject)graph.GetOrCreateNodeInstance(this);
            instance.fact = graph.GetOrCreateFactInstance(fact);
            return instance;
        }

        public override bool Init(Vector2 position, ZDialogueGraph graph)
        {
            base.Init(position, graph);
            if (graph.facts.Count > 0)
            {
                fact = graph.facts[0];
                return true;
            }
            Debug.LogWarning("There's no Facts in the current graph!");
            return false;
        }
    }
}