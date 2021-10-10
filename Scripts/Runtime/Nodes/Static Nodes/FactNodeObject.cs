using UnityEngine;

namespace ZDialoguer
{
    public class FactNodeObject : StaticNodeObject
    {
        public Fact fact;

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