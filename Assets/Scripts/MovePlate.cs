using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    //Some functions will need reference to the controller
    public GameObject controller;

    //The Chesspiece that was tapped to create this MovePlate
    GameObject reference = null;

    //Location on the board
    int matrixX;
    int matrixY;

    public enum PlateType
    {
        attack = 0,
        moving = 1,
        self = 2,
    }
    //false: movement, true: attacking
    public PlateType plateType = PlateType.moving;

    public void Start()
    {
        if (plateType == PlateType.attack)
        {
            //Set to red
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
        else if (plateType == PlateType.self)
        {
            //Set to green
            gameObject.GetComponent<SpriteRenderer>().color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        }
    }

    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        Chessman chessman = reference.GetComponent<Chessman>();
        Game gm = controller.GetComponent<Game>();

        //Destroy the victim Chesspiece
        if (plateType == PlateType.attack)
        {
            GameObject cp = gm.GetPosition(matrixX, matrixY);
            string activePlayer = gm.GetCurrentPlayer();
            string otherPlayer = activePlayer == "white" ? "black" : "white";
            if (cp.name == otherPlayer + "_king")
            {
                int kings = gm.GetKingCount(otherPlayer);
                if (kings <= 1)
                {
                    gm.Winner(activePlayer);
                }
                else
                {
                    gm.SetKingCount(otherPlayer, kings - 1);
                }
            }

            Destroy(cp);
            chessman.AddExperience();
        }
        // Ignore clicks on self
        else if (plateType == PlateType.self)
        {
            return;
        }

        //Set the Chesspiece's original location to be empty
        controller.GetComponent<Game>().SetPositionEmpty(chessman.GetXBoard(),
            chessman.GetYBoard());

        //Move reference chess piece to this position
        chessman.SetXBoard(matrixX);
        chessman.SetYBoard(matrixY);
        chessman.SetCoords();
        chessman.UpdateFirstMove();

        //Update the matrix
        controller.GetComponent<Game>().SetPosition(reference);

        //Switch Current Player
        controller.GetComponent<Game>().NextTurn();

        //Switch to roll phase of that player's turn
        controller.GetComponent<Game>().NextPhase();

        //Destroy the move plates including self
        reference.GetComponent<Chessman>().DestroyMovePlates();
    }

    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    public GameObject GetReference()
    {
        return reference;
    }
}
