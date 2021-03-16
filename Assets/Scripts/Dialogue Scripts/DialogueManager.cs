using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;


public enum TextEffect
{
    Wavy,
    Shaky,
    None,
}

public class DialogueManager : MonoBehaviour
{
    // Story Variables
    private static Story dialogueStory;
    private static Choice choiceSelected;
    private bool dialogueHappening = false;
    private bool isTyping = false;
    public GameObject dialogueBox;
    public GameObject EventSystem;
    
    // Text Effect Variables
    private List<(char, TextEffect)> charEffects = new List<(char, TextEffect)>();
    private TextEffect activeEffect = TextEffect.None;


    // UI Variables
    public GameObject choicePanel;  // This will be part of the dialogue box which will be where the choices go
    public GameObject customButton; // This will be a prefab of an example button that will be the preset for all generated buttons
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (!dialogueHappening)
            return;

        doAnimations();

        if (Input.GetKeyDown(KeyCode.Z) && !isTyping) 
        {
            // Makes sure that it is not time for a choice
            if (dialogueStory.currentChoices.Count == 0)
                DisplayNextSentence();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        dialogueStory = new Story(dialogue.inkFile.text);
        dialogueHappening = true;
        // Sets the name variable so peeps know to whom they speak
        dialogueBox.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = dialogue.name;
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Checks 
        if (!dialogueStory.canContinue && dialogueStory.currentChoices.Count == 0) 
        {
            EndDialogue();
            return;
        }
        string sentence = dialogueStory.Continue();
        LoadEffects(sentence);  // loads the text effects for each chunk of text
        StopAllCoroutines();

        // This will update the UI to show the current sentence
        StartCoroutine(TypeOutSentence());

    }

    public void EndDialogue() 
    {
        dialogueHappening = false;
    }

    /*** Types out the sentence ***/
    /// <summary>
    ///  This function will be used to type out the sentence letter by letter and add any effects to the letters as necessary. 
    ///  After it types out the function, it checks if there are choices, and if there are, it will call the function to display them.
    /// </summary>
    /// <param name="sentence"></param>
    /// <returns>null</returns>
    IEnumerator TypeOutSentence()
    {
        // this gets the second child of the dialogue box, which should be the dialogue text component
        TMPro.TextMeshProUGUI dialogueText = dialogueBox.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();

        // sets the text to be blank
        dialogueText.text = "";
        // sets this to true to prevent the player from acidentally skipping dialogue
        isTyping = true;
        foreach ((char, TextEffect) entry in charEffects)
        {

                // enables the player to skip the writing animation
                if (Input.GetKeyDown(KeyCode.X))
                {
                    SkipThroughSentence(dialogueText);
                    break;
                }
                // types out the text
                dialogueText.text += entry.Item1;
                yield return null;
        }
        // sets it to false as dialogue has been completed
        isTyping = false;

        // Checks if it is choice time
        if (!(dialogueStory.currentChoices.Count == 0))
            StartCoroutine(ShowChoices());
        yield return null;
    }

    public void SkipThroughSentence(TMPro.TextMeshProUGUI dialogueText)
    {
        string sentence = "";
        foreach ((char, TextEffect) var in charEffects) 
        {
            sentence += var.Item1;
        }
        dialogueText.text = sentence;
    }

    // Text Effect Functions

    // textMesh.textInfo.meshInfo[i].mesh.vertices += new Vector3(random(-1,1)...) to cause shak
    /*** Tag Parser ***/
    /// <summary>
    /// Simply parses the tabs in the ink file and starts the corresponding text effect
    /// </summary>
    /// <param name="sentence"> This is the full line of text</param>
    /// <param name="c"> This is the current character of the line of text</param>
    /// <param name="cIndex"> This is the string index of c</param>
    /// <param name="inTag"> This keeps track of whether the program is in a tag</param>
    public void CheckTags(string sentence, char c, int cIndex, ref bool inTag)
    {
        if (c == '<')
        {
            // Since current char is < we have entered a tag
            inTag = true;

            char next = sentence[cIndex + 1];

            if (next != '/')
            {
                switch (next)
                {
                    case 'w': activeEffect = TextEffect.Wavy;  break;
                    case 'v': activeEffect = TextEffect.Shaky; break;
                }
            }
            else
            {
                activeEffect = TextEffect.None;
            }
        }
        else if (cIndex > 0 && sentence[cIndex - 1] == '>')
        {
            inTag = false;
        }
    }

    public void LoadEffects(string sentence)
    {
        // clears the previous sentences char effects
        charEffects.Clear();
        int cIndex = 0;
        bool inTag = false;
        activeEffect = TextEffect.None;
        foreach (char c in sentence)
        {
            // Gets the char index in the sentence and then check if the dialogue is in a tag, and if so adds the current effect to charEffects
            CheckTags(sentence, c, cIndex, ref inTag);
            if (!inTag) 
            {
                charEffects.Add((c, activeEffect));
            }
            cIndex++;
        }
        activeEffect = TextEffect.None;
    }

    private void doAnimations() 
    {
        // This gets the second child of the dialogue box, which should be the dialogue text component and stores it.
        // It does the same for text info
        TMPro.TextMeshProUGUI dialogueText = dialogueBox.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TMP_TextInfo textInfo = dialogueText.textInfo;

        // Forces the Mesh to update, makes sure the text is up to date
        dialogueText.ForceMeshUpdate();

        // Creates a copy of the vertices in the text mesh
        Vector3[][] copyOfVertices = new Vector3[0][];
        // Makes sure that the length of the copy is less that the length of textinfo before initialising it to the
        // length of the mesh, i.e. the number of letters 
        if (copyOfVertices.Length < textInfo.meshInfo.Length)
            copyOfVertices = new Vector3[textInfo.meshInfo.Length][];
        // Loops through the vertices and initilialises the arrays to the correct length
        for (int x = 0; x < textInfo.meshInfo.Length; x++)
        {
            copyOfVertices[x] = new Vector3[textInfo.meshInfo[x].vertices.Length];
        }

        // Caches the mesh data into a variable
        TMPro.TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
        // Starts the loop through the charEffects List
        for (int i = 0; i < charEffects.Count; i++)
        {
            // Checks if there is no associated effect and/or the text is visible
            if (charEffects[i].Item2 == TextEffect.None || !textInfo.characterInfo[i].isVisible)
                continue;
            // Tbh I have no clue what this does
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;

            for (int x = 0; x < textInfo.meshInfo.Length; x++)
            {
                copyOfVertices[materialIndex] = textInfo.meshInfo[x].mesh.vertices;
            }

            // resetting the Vector modifier to be either shaky or wavy
            float modX = 0f;
            float modY = 0f;
            if (charEffects[i].Item2 == TextEffect.Shaky) {
                // shaky text code
                int shakeAmount = 1; // change this to modify amount of shake
                Vector2 rand = Random.insideUnitCircle * shakeAmount;
                modX = rand.x;
                modY = rand.y;
            }
            else if (charEffects[i].Item2 == TextEffect.Wavy)
            {
                // wavy text code
                int amplitude = 1;
                modX = 0f;
                modY = Mathf.Sin(copyOfVertices[materialIndex][vertexIndex+0].x * 0.1f + 10 * Time.time) * amplitude;
            }
            Vector3 modifier = new Vector3(modX, modY, 0f);

            // These are the verteces of the bounding box, so this bit of code updates the position of all of these
            copyOfVertices[materialIndex][vertexIndex + 0] = sourceVertices[vertexIndex + 0] + modifier;
            copyOfVertices[materialIndex][vertexIndex + 1] = sourceVertices[vertexIndex + 1] + modifier;
            copyOfVertices[materialIndex][vertexIndex + 2] = sourceVertices[vertexIndex + 2] + modifier;
            copyOfVertices[materialIndex][vertexIndex + 3] = sourceVertices[vertexIndex + 3] + modifier;

            // Updates the mesh with the new positions
            for (int x = 0; x < textInfo.meshInfo.Length; x++)
            {
                textInfo.meshInfo[x].mesh.vertices = copyOfVertices[x];
                dialogueText.UpdateGeometry(textInfo.meshInfo[x].mesh, x);
            }
        }
    }

    // Choice functions

    /*** Dynamically adds the choice buttons to the Dialogue Box***/
    /// <summary>
    /// This will attempt to add the choice buttons to the dialogue box which will be given as input to the Dialogue Manager
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowChoices()
    {

        isTyping = true;
        List<Choice> _choices = dialogueStory.currentChoices;

        for (int i = 0; i < _choices.Count; i++)
        {
            GameObject temp = Instantiate(customButton, choicePanel.transform);
            float spacing = customButton.GetComponent<RectTransform>().sizeDelta.x;
            Vector3 localPosition = new Vector3();
            localPosition.x = i * 2 * spacing - spacing;
            temp.GetComponent<Transform>().localPosition = localPosition;
            temp.transform.GetComponent<Text>().text = _choices[i].text;
            temp.AddComponent<Selectable>();
            temp.GetComponent<Selectable>().element = _choices[i];
            temp.GetComponent<Button>().navigation = new Navigation() { mode = Navigation.Mode.Horizontal };
            temp.GetComponent<Button>().onClick.AddListener(() => { temp.GetComponent<Selectable>().Decide(); });

        }
        EventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(choicePanel.transform.GetChild(0).gameObject);
        choicePanel.SetActive(true);
        yield return new WaitUntil(() => { return choiceSelected != null;  });
        isTyping = false;
        AdvanceAfterChoice();
    }

    /*** This updates the story to use the choice inputted as a parameter ***/
    /// <summary>
    /// This uses the param element, being the element that has the choice and sets the choice selected in the story
    /// </summary>
    /// <param name="element"></param>
    public static void SetDecision(object element)
    {
        choiceSelected = (Choice)element;
        dialogueStory.ChooseChoiceIndex(choiceSelected.index);
    }

    /*** This advance the dialogue after a choice has been made ***/
    /// <summary>
    /// This deactivates the choicePanel and removes all of its children, then displays the next sentence
    /// </summary>
    public void AdvanceAfterChoice() 
    {
        choicePanel.SetActive(false);
        for (int i = 0; i < choicePanel.transform.childCount; i++) 
        {
            Destroy(choicePanel.transform.GetChild(i).gameObject);
        }
        choiceSelected = null;
        DisplayNextSentence();
    }

}

