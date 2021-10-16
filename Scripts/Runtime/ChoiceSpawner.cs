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
    private ChoiceNodeObject currentChoiceNodeObject;
    public void SpawnChoices(ChoiceNodeObject choiceNodeObject)
    {
        // gameObject.SetActive(true);
        Debug.Log("Spawning Choices for " + choiceNodeObject);
        currentChoiceNodeObject = choiceNodeObject;
        for (var i = 0; i < choiceNodeObject.choices.Count; i++)
        {
            var button = Instantiate(buttonPrefab, transform.position, Quaternion.identity, transform).GetComponent<Button>();
            button.GetComponentInChildren<TextMeshProUGUI>().text = choiceNodeObject.choices[i].choiceText;
            int index = i;
            button.onClick.AddListener(() =>
            {
                currentChoiceNodeObject.ConfirmChoice(choiceNodeObject.choices[index]);
                OnSubmitChoice.Invoke(choiceNodeObject);
                foreach (Transform tf in transform)
                {
                    Destroy(tf);
                }
                // gameObject.SetActive(false);
            });
        }
    }
}
