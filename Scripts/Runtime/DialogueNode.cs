using ZGraph;

namespace ZGraph.DialogueSystem
{
    public abstract class DialogueNode : Node
    {
        public DialogueGraph DialogueGraph;

        protected override void OnCreate()
        {
            base.OnCreate();

            DialogueGraph = graph as DialogueGraph;

            if (DialogueGraph == null) throw new NodeNotCreatedException($"variable \"graph\" is null");
        }
    }

    public abstract class UnnamedNode : DialogueNode
    {
        public abstract void Execute();
    }
}