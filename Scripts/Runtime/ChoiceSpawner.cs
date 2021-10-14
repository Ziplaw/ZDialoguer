using System.Collections;
using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ZDialoguer;

public class ChoiceSpawner : MonoBehaviour
{
    public UnityEvent<ChoiceNodeObject> OnSubmitChoice;
    private ChoiceNodeObject currentChoiceNodeObject;
    public void SpawnChoices(ChoiceNodeObject choiceNodeObject)
    {
        gameObject.SetActive(true);
        Debug.Log("Spawning Choices for " + choiceNodeObject);
        currentChoiceNodeObject = choiceNodeObject;

        var buttons = GetComponentsInChildren<Button>();
        for (var i = 0; i < buttons.Length; i++)
        {
            int index = i;
            var button = buttons[i];
            button.onClick.AddListener(() =>
            {
                currentChoiceNodeObject.ConfirmChoice(choiceNodeObject.choices[index]);
                OnSubmitChoice.Invoke(choiceNodeObject);
                gameObject.SetActive(false);
            });
        }
    }
}
