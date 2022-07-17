using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessman : MonoBehaviour
{
    //References to objects in our Unity Scene
    public GameObject controller;
    public GameObject movePlate;

    public GameObject experienceIndicator;

    //Position for this Chesspiece on the Board
    //The correct position will be set later
    private int xBoard = -1;
    private int yBoard = -1;

    //Variable for keeping track of the player it belongs to "black" or "white"
    private string player;

    private bool firstMove = true;
    // Experience is used to determine how likely a piece is to reroll into something good.
    private int experience = 0;
    private int numRolls = 0;

    private GameObject[] rollIndicators = new GameObject[3];

    //References to all the possible Sprites that this Chesspiece could be
    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    public void Activate()
    {
        //Get the game controller
        controller = GameObject.FindGameObjectWithTag("GameController");

        //Take the instantiated location and adjust transform
        SetCoords();

        //Choose correct sprite based on piece's name
        switch (this.name)
        {
            case "black_queen": this.GetComponent<SpriteRenderer>().sprite = black_queen; player = "black"; experience = 3; break;
            case "black_knight": this.GetComponent<SpriteRenderer>().sprite = black_knight; player = "black"; experience = 1; break;
            case "black_bishop": this.GetComponent<SpriteRenderer>().sprite = black_bishop; player = "black"; experience = 1; break;
            case "black_king": this.GetComponent<SpriteRenderer>().sprite = black_king; player = "black"; break;
            case "black_rook": this.GetComponent<SpriteRenderer>().sprite = black_rook; player = "black"; experience = 2; break;
            case "black_pawn": this.GetComponent<SpriteRenderer>().sprite = black_pawn; player = "black"; experience = 0; break;
            case "white_queen": this.GetComponent<SpriteRenderer>().sprite = white_queen; player = "white"; experience = 3; break;
            case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; player = "white"; experience = 1; break;
            case "white_bishop": this.GetComponent<SpriteRenderer>().sprite = white_bishop; player = "white"; experience = 1; break;
            case "white_king": this.GetComponent<SpriteRenderer>().sprite = white_king; player = "white"; break;
            case "white_rook": this.GetComponent<SpriteRenderer>().sprite = white_rook; player = "white"; experience = 2; break;
            case "white_pawn": this.GetComponent<SpriteRenderer>().sprite = white_pawn; player = "white"; experience = 0; break;
        }
        UpdateExperienceIndicators(true);
    }

    void UpdateExperienceIndicators(bool first)
    {
        if (first)
        {

            for (int i = 0; i < experience; i++)
            {
                GameObject ec = Instantiate(Resources.Load("Experience Circle") as GameObject, new Vector3(this.transform.position.x + 0.25f, this.transform.position.y - 0.25f + 0.1f * i, -4f), Quaternion.identity);
                ec.transform.parent = this.transform;
            }
        }
        else
        {

            GameObject ec = Instantiate(Resources.Load("Experience Circle") as GameObject, new Vector3(this.transform.position.x + 0.25f, this.transform.position.y - 0.25f + 0.1f * (experience - 1), -4f), Quaternion.identity);
            ec.transform.parent = this.transform;
        }
    }

    public void SetCoords()
    {
        //Get the board value in order to convert to xy coords
        float x = xBoard;
        float y = yBoard;

        //Adjust by variable offset
        x *= 0.66f;
        y *= 0.66f;

        //Add constants (pos 0,0)
        x += -2.3f;
        y += -2.3f;

        //Set actual unity values
        this.transform.position = new Vector3(x, y, -1.0f);
    }

    public int GetXBoard()
    {
        return xBoard;
    }

    public int GetYBoard()
    {
        return yBoard;
    }

    public void SetXBoard(int x)
    {
        xBoard = x;
    }

    public void SetYBoard(int y)
    {
        yBoard = y;
    }

    private void OnMouseUp()
    {

        Game sc = controller.GetComponent<Game>();
        // The game isn't over and the clicked on piece belongs to the current player.
        if (!sc.IsGameOver() && controller.GetComponent<Game>().GetCurrentPlayer() == player)
        {
            // Highlight active piece
            if (sc.GetCurrentPhase() == "roll")
            {
                // Pieces can only be rerolled three times
                if (numRolls >= 3)
                {
                    return;
                }
                DestroyMovePlates();
                MovePlateSelfSpawn(xBoard, yBoard);
            }
            else
            {
                //Remove all moveplates relating to previously selected piece
                DestroyMovePlates();

                //Create new MovePlates
                InitiateMovePlates();
            }
            sc.SetActivePiece(gameObject);
        }
    }

    public void DestroyMovePlates()
    {
        //Destroy old MovePlates
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]); //Be careful with this function "Destroy" it is asynchronous
        }
    }

    public void InitiateMovePlates()
    {
        MovePlateSelfSpawn(xBoard, yBoard);
        switch (this.name)
        {
            case "black_queen":
            case "white_queen":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(1, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                LineMovePlate(-1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(1, -1);
                break;
            case "black_knight":
            case "white_knight":
                LMovePlate();
                break;
            case "black_bishop":
            case "white_bishop":
                LineMovePlate(1, 1);
                LineMovePlate(1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(-1, -1);
                break;
            case "black_king":
            case "white_king":
                SurroundMovePlate();
                break;
            case "black_rook":
            case "white_rook":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                break;
            case "black_pawn":
                PawnMovePlate(xBoard, yBoard - 1);
                break;
            case "white_pawn":
                PawnMovePlate(xBoard, yBoard + 1);
                break;
        }
    }

    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        Game sc = controller.GetComponent<Game>();

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);
            x += xIncrement;
            y += yIncrement;
        }

        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y).GetComponent<Chessman>().player != player)
        {
            MovePlateAttackSpawn(x, y);
        }
    }

    public void LMovePlate()
    {
        PointMovePlate(xBoard + 1, yBoard + 2);
        PointMovePlate(xBoard - 1, yBoard + 2);
        PointMovePlate(xBoard + 2, yBoard + 1);
        PointMovePlate(xBoard + 2, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard - 2);
        PointMovePlate(xBoard - 1, yBoard - 2);
        PointMovePlate(xBoard - 2, yBoard + 1);
        PointMovePlate(xBoard - 2, yBoard - 1);
    }

    public void SurroundMovePlate()
    {
        PointMovePlate(xBoard, yBoard + 1);
        PointMovePlate(xBoard, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard + 0);
        PointMovePlate(xBoard - 1, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard + 1);
        PointMovePlate(xBoard + 1, yBoard + 0);
        PointMovePlate(xBoard + 1, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard + 1);
    }

    public void PointMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.PositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);

            if (cp == null)
            {
                MovePlateSpawn(x, y);
            }
            else if (cp.GetComponent<Chessman>().player != player)
            {
                MovePlateAttackSpawn(x, y);
            }
        }
    }

    public void PawnMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        // Jump moves for pawns on first move
        if (firstMove)
        {
            PointMovePlate(x, this.name == "white_pawn" ? y + 1 : y - 1);
        }
        if (sc.PositionOnBoard(x, y))
        {
            if (sc.GetPosition(x, y) == null)
            {
                MovePlateSpawn(x, y);
            }

            if (sc.PositionOnBoard(x + 1, y) && sc.GetPosition(x + 1, y) != null && sc.GetPosition(x + 1, y).GetComponent<Chessman>().player != player)
            {
                MovePlateAttackSpawn(x + 1, y);
            }

            if (sc.PositionOnBoard(x - 1, y) && sc.GetPosition(x - 1, y) != null && sc.GetPosition(x - 1, y).GetComponent<Chessman>().player != player)
            {
                MovePlateAttackSpawn(x - 1, y);
            }
        }
    }

    public void MovePlateSpawn(int matrixX, int matrixY)
    {
        //Get the board value in order to convert to xy coords
        float x = matrixX;
        float y = matrixY;

        //Adjust by variable offset
        x *= 0.66f;
        y *= 0.66f;

        //Add constants (pos 0,0)
        x += -2.3f;
        y += -2.3f;

        //Set actual unity values
        GameObject mp = Instantiate(movePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }

    public void MovePlateAttackSpawn(int matrixX, int matrixY)
    {
        //Get the board value in order to convert to xy coords
        float x = matrixX;
        float y = matrixY;

        //Adjust by variable offset
        x *= 0.66f;
        y *= 0.66f;

        //Add constants (pos 0,0)
        x += -2.3f;
        y += -2.3f;

        //Set actual unity values
        GameObject mp = Instantiate(movePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.plateType = MovePlate.PlateType.attack;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }

    // This is horrible code duplication. Should  be converted to a function that takes a plate type.
    public void MovePlateSelfSpawn(int matrixX, int matrixY)
    {
        //Get the board value in order to convert to xy coords
        float x = matrixX;
        float y = matrixY;

        //Adjust by variable offset
        x *= 0.66f;
        y *= 0.66f;

        //Add constants (pos 0,0)
        x += -2.3f;
        y += -2.3f;

        //Set actual unity values
        GameObject mp = Instantiate(movePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.plateType = MovePlate.PlateType.self;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }

    // Gets a value from 1 to 6 and converts the piece to something new depending on the experience.
    public void RerollPiece(int diceValue)
    {
        Game gm = controller.GetComponent<Game>();
        int pieceIndex = ApplyExperienceModifier(diceValue);
        numRolls++;
        // Make sure kings can always reroll to prevent soft locks.
        if (this.name == "white_king" || this.name == "black_king")
        {
            string activePlayer = gm.GetCurrentPlayer();
            int kingCount = gm.GetKingCount(activePlayer);
            if (kingCount <= 1)
            {
                gm.Winner(activePlayer == "white" ? "black" : "white");
            }
            // Kings should always reroll into pawns.
            pieceIndex = 1;
        }

        switch (pieceIndex)
        {
            case 1:
                this.name = player + "_pawn";
                break;
            case 2:
                this.name = player + "_knight";
                break;
            case 3:
                this.name = player + "_bishop";
                break;
            case 4:
                this.name = player + "_rook";
                break;
            case 5:
                this.name = player + "_queen";
                break;
            case 6:
                this.name = player + "_king";
                // If a piece is rerolled into a king then add one to the king count.
                string activePlayer = gm.GetCurrentPlayer();
                gm.SetKingCount(activePlayer, gm.GetKingCount(activePlayer) + 1);
                // Must prevent soft locking by being unable to lose or reroll.
                numRolls = Mathf.Min(numRolls, 2);
                break;
        }
        this.GetComponent<SpriteRenderer>().sprite = GetRerolledSprite(pieceIndex);
        string stringRolls = numRolls == 1 ? "One" : numRolls == 2 ? "Two" : "Three";
        GameObject ind = Instantiate(Resources.Load(stringRolls + "_reroll") as GameObject, new Vector3(this.transform.position.x + 0.025f, this.transform.position.y, -0.01f), Quaternion.identity);
        ind.transform.parent = this.transform;
        rollIndicators[numRolls - 1] = ind;
    }

    public Sprite GetRerolledSprite(int diceValue)
    {
        bool white = player == "white";
        switch (diceValue)
        {
            case 1:
                return white ? white_pawn : black_pawn;
            case 2:
                return white ? white_knight : black_knight;
            case 3:
                return white ? white_bishop : black_bishop;
            case 4:
                return white ? white_rook : black_rook;
            case 5:
                return white ? white_queen : black_queen;
            case 6:
                return white ? white_king : black_king;
        }
        Debug.Log("SHOULD NOT GET HERE");
        return white_king;
    }

    public int ApplyExperienceModifier(int diceRoll)
    {
        switch (experience)
        {
            case 0:
                return Mathf.Max(1, diceRoll - 3);
            case 1:
                return Mathf.Max(diceRoll - 1, 0) % 4 + 1;
            case 2:
                return Mathf.Max(diceRoll - 1, 0) % 3 + 3;
            case 3:
                return Mathf.Max(diceRoll - 1, 0) % 2 + 5;
            default:
                Debug.Log("SHOULD NOT GET HERE");
                return 1;
        }
    }

    public void AddExperience()
    {
        if (experience < 3)
        {
            experience++;
            UpdateExperienceIndicators(false);
        }
        if (numRolls > 0)
        {
            numRolls--;
            Destroy(rollIndicators[numRolls]);
        }
    }
    public int GetExperience()
    {
        return experience;
    }

    public void UpdateFirstMove()
    {
        firstMove = false;
    }
}
