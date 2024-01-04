using System.Collections;
using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ZGraph.DialogueSystem;

public class ChoiceSpawner : MonoBehaviour
{
    public GameObject buttonPrefab;
    public UnityEvent<ChoiceDialogueNodeObject> OnSubmitChoice;
    [FormerlySerializedAs("currentChoiceNodeObject")] public ChoiceDialogueNodeObject currentChoiceDialogueNodeObject;
    public void SpawnChoices(ChoiceDialogueNodeObject choiceDialogueNodeObject)
    {
        currentChoiceDialogueNodeObject = choiceDialogueNodeObject;
        for (var i = 0; i < choiceDialogueNodeObject.choices.Count; i++)
        {
            var button = Instantiate(buttonPrefab, transform.position, Quaternion.identity, transform).GetComponent<Button>();
            button.GetComponentInChildren<TextMeshProUGUI>().text = choiceDialogueNodeObject.choices[i].choiceText;

            // if (!choiceDialogueNodeObject.choices[i].Enabled)
            {
                button.interactable = false;
                if (choiceDialogueNodeObject.choices[i].visibility == Choice.DisabledVisibility.Hidden)
                {
                    button.gameObject.SetActive(false);
                }
                continue;
            }
            int index = i;
            button.onClick.AddListener(() =>
            {
                currentChoiceDialogueNodeObject.ConfirmChoice(choiceDialogueNodeObject.choices[index]);
                OnSubmitChoice.Invoke(choiceDialogueNodeObject);
                foreach (Transform tf in transform)
                {
                    Destroy(tf.gameObject);
                }
            });
        }
    }
}
