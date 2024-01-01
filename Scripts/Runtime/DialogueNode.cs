using ZGraph;

namespace ZGraph.DialogueSystem
{
    public abstract class DialogueNode : ZNode
    {
        public ZDialogueGraph DialogueGraph;

        protected override void OnCreate()
        {
            base.OnCreate();

            DialogueGraph = graph as ZDialogueGraph;

            if (DialogueGraph == null) throw new NodeNotCreatedException($"variable \"graph\" is null");
        }
    }
    
    public class Flow {}
}