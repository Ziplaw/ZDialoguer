using System;
using System.Collections.Generic;
using UnityEngine;
using ZDialoguer.Localization;

namespace ZGraph.DialogueSystem
{
    [ZNodeDisplay("#9991F5","Exit")]
    public class ExitDialogueNodeObject : UnnamedNode
    {
        [Input(PortOptions.Single)] int Input;
        public override void Execute()
        {
            var connectedNode = GetConnectedNode<UnnamedNode>(nameof(Input));
            connectedNode.Execute();
            Input = connectedNode.GetValueConnectedTo<int>(this, nameof(Input));
        }
    }
}