using System.Collections;
using UnityEngine;

public class Dice : MonoBehaviour
{

    // Array of dice sides sprites to load from Resources folder
    private Sprite[] diceSides;

    // Reference to sprite renderer to change sprites
    private SpriteRenderer rend;

    public GameObject controller;

    GameObject reference = null;

    // Use this for initialization
    private void Start()
    {

        // Assign Renderer component
        rend = GetComponent<SpriteRenderer>();

        // Load dice sides sprites to array from DiceSides subfolder of Resources folder
        diceSides = Resources.LoadAll<Sprite>("DiceSides/");

        controller = GameObject.FindGameObjectWithTag("GameController");
    }
    private void Update()
    {
        Game cs = controller.GetComponent<Game>();
        reference = cs.GetActivePiece();
        string phase = controller.GetComponent<Game>().GetCurrentPhase();
        if (rend != null)
        {
            rend.enabled = (phase == "roll" ? true : false);
            if (!rend.enabled) return;
        }
        UpdateDicePotentials();
    }

    private void UpdateDicePotentials()
    {
        if (reference == null)
        {
            return;
        }
        // Show what each dice value would produce when rerolling the active dice.
        GameObject[] dicePotentials = GameObject.FindGameObjectsWithTag("DicePotential");
        int i = 1;
        Chessman cm = reference.GetComponent<Chessman>();

        foreach (GameObject dicePotential in dicePotentials)
        {
            int modifiedRoll = cm.ApplyExperienceModifier(i);
            GameObject spriteHolder = dicePotential.transform.Find("SpriteHolder").gameObject;
            spriteHolder.GetComponent<SpriteRenderer>().sprite = cm.GetRerolledSprite(modifiedRoll);
            i++;
        }
    }

    // If you left click over the dice then RollTheDice coroutine is started
    private void OnMouseDown()
    {
        // Get the active piece when the dice is rolled and send the dice result to it.
        if (reference != null)
        {
            StartCoroutine("RollTheDice");
        }
    }


    // Coroutine that rolls the dice
    private IEnumerator RollTheDice()
    {
        // Variable to contain random dice side number.
        // It needs to be assigned. Let it be 0 initially
        int randomDiceSide = 0;

        // Final side or value that dice reads in the end of coroutine
        int finalSide = 0;

        // Loop to switch dice sides ramdomly
        // before final side appears. 20 itterations here.
        for (int i = 0; i <= 20; i++)
        {
            // Pick up random value from 0 to 5 (All inclusive)
            randomDiceSide = Random.Range(0, 6);

            // Set sprite to upper face of dice from array according to random value
            rend.sprite = diceSides[randomDiceSide];

            // Pause before next itteration
            yield return new WaitForSeconds(0.05f);
        }

        // Assigning final side so you can use this value later in your game
        // for player movement for example
        finalSide = randomDiceSide + 1;

        // Show final dice value in Console
        reference.GetComponent<Chessman>().RerollPiece(finalSide);
        // Unhighlight the piece when the phase changes.
        reference.GetComponent<Chessman>().DestroyMovePlates();
        // When the dice stops. Switch to the moving phase.
        yield return new WaitForSeconds(0.5f);
        controller.GetComponent<Game>().NextPhase();
        reference = null;
    }
}
