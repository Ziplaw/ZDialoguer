using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using ZDialoguer;
using TMPro;

public class DialogueDirector : MonoBehaviour
{
    // public List<ZDialogueGraph> graphs;
    public TextMeshProUGUI textComponent;
    public ZDialogueGraph currentGraph;


    public UnityEvent OnDialogueStart;

    public void GetNextText()
    {
        string parsedText = currentGraph.GetEntryNode().GetNextText(currentGraph);
        if (!string.IsNullOrEmpty(parsedText))
            ComposeText(parsedText);
    }

    async void ComposeText(string parsedText)
    {
        textComponent.text = "";
        while (textComponent.text.Length < parsedText.Length)
        {
            textComponent.text += parsedText[textComponent.text.Length];
            await Task.Yield();
        }
    }
}