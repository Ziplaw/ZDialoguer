using System.Collections;
using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ZDialoguer;

public class ChoiceSpawner : MonoBehaviour
{
    public GameObject buttonPrefab;
    public UnityEvent<ChoiceNodeObject> OnSubmitChoice;
    public ChoiceNodeObject currentChoiceNodeObject;
    public void SpawnChoices(ChoiceNodeObject choiceNodeObject)
    {
        currentChoiceNodeObject = choiceNodeObject;
        for (var i = 0; i < choiceNodeObject.choices.Count; i++)
        {
            var button = Instantiate(buttonPrefab, transform.position, Quaternion.identity, transform).GetComponent<Button>();
            button.GetComponentInChildren<TextMeshProUGUI>().text = choiceNodeObject.choices[i].choiceText;

            if (!choiceNodeObject.choices[i].Enabled)
            {
                button.interactable = false;
                if (choiceNodeObject.choices[i].visibility == Choice.DisabledVisibility.Hidden)
                {
                    button.gameObject.SetActive(false);
                }
                continue;
            }
            int index = i;
            button.onClick.AddListener(() =>
            {
                currentChoiceNodeObject.ConfirmChoice(choiceNodeObject.choices[index]);
                OnSubmitChoice.Invoke(choiceNodeObject);
                foreach (Transform tf in transform)
                {
                    Destroy(tf.gameObject);
                }
            });
        }
    }
}
