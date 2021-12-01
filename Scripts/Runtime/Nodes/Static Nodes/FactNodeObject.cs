using System.Collections.Generic;
using UnityEngine;

namespace ZDialoguer
{
    public class FactNodeObject : StaticNodeObject
    {
        public int factIndex;

        public override NodeObject DeepClone()
        {
            FactNodeObject instance = (FactNodeObject)graph.GetOrCreateNodeInstance(this);
            instance.factIndex = factIndex;
            return instance;
        }

        public override bool Init(Vector2 position, ZDialogueGraph graph)
        {
            base.Init(position, graph);
            if (graph.facts.Count > 0)
            {
                factIndex = graph.facts[0];
                return true;
            }
            Debug.LogWarning("There's no Facts in the current graph!");
            return false;
        }
    }
}