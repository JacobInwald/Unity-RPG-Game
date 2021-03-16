using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{

    public Dialogue dialogue;

    public void TriggerDialogue() {
        // Stops any dialogue that's running
        FindObjectOfType<DialogueManager>().EndDialogue();
        // Starts the dialogue
        FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
    }

}
