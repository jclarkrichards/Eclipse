using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum Player
{
    sun,
    moon,
}

public enum Phase
{
    flip,
    movePiece,
    changeSide
}

public class GameController : MonoBehaviour
{
    public static GameController S;
    public GameObject sunPrefab;
    public GameObject moonPrefab;
    public GameObject sunTray;
    public GameObject moonTray;

    //Buttons
    public Button sunFlipRedo;
    public Button sunEndTurn;
    public Button sunChangeColor;
    public Button sunForfeit;
    public Button moonFlipRedo;
    public Button moonEndTurn;
    public Button moonChangeColor;
    public Button moonForfeit;

    //Table
    public GameObject sunTable;
    public GameObject moonTable;

    //public bool divider _______________________;
    //The Moon and Sun GameObjects are stored in these two arrays
    GameObject[] sunPieces;
    GameObject[] moonPieces;

    int numPieces = 8;

    //Only one piece can be the active piece
    GameObject activePiece;
    GameObject flippedPiece;  //Holds the piece just flipped
    Player activePlayer = Player.moon;

    //For recording the moves
    Stack<string> record;

    void Awake()
    {
        S = this;
    }

    // Use this for initialization
    void Start ()
    {
        CreatePieces();
        //Board.S.Test(sunPieces);
        InitializePositions();
        SwitchPlayers();
        
        record = new Stack<string>();

        //record.Push("hello");
        //record.Push("there");
        //print(record.Peek());
        //string s = record.Pop();
        //print(s);
        //print(record.Peek());

    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    //Instantiate all of the Moon and Sun pieces
    void CreatePieces()
    {      
        sunPieces = new GameObject[8];
        moonPieces = new GameObject[8];
        for (int i = 0; i < numPieces; i++)
        {
            GameObject sunPiece = Instantiate(sunPrefab) as GameObject;
            GameObject moonPiece = Instantiate(moonPrefab) as GameObject;
            sunPiece.name = sunPrefab.name + i.ToString();
            moonPiece.name = moonPrefab.name + i.ToString();
            sunPieces[i] = sunPiece;
            moonPieces[i] = moonPiece;

        }
    }

    //Place pieces loosely on their respective trays
    void InitializePositions()
    {
        if(sunPieces.Length > 0 && moonPieces.Length > 0)
        {
            for(int i=0; i<numPieces; i++)
            {
                moonPieces[i].GetComponent<GamePiece>().SetHomePosition(moonTray);
                sunPieces[i].GetComponent<GamePiece>().SetHomePosition(sunTray);
            }
        }
        
    }

    //Set the active piece so we can do other stuff to only this piece
    //Will need to apply other checks later on
    public void SetActivePiece(GameObject go)
    {
        activePiece = go;
        sunFlipRedo.interactable = false;
        moonFlipRedo.interactable = false;
    }

    public void ReportPieceChosen(GameObject obj)
    {

    }

    public void ReportPiecePlacedOnBoard(GameObject obj)
    {
        if(activePlayer == Player.sun)
        {
            sunEndTurn.interactable = true;
            sunChangeColor.interactable = true;
            sunFlipRedo.interactable = false;
            Utils.DisablePieces(sunPieces);
            Utils.EnablePiece(obj);
        }
        else
        {
            moonEndTurn.interactable = true;
            moonChangeColor.interactable = true;
            moonFlipRedo.interactable = false;
            Utils.DisablePieces(moonPieces);
            Utils.EnablePiece(obj);
        }
    }

    public void ReportPieceFlipped(GameObject obj)
    {
        
        if(activePlayer == Player.sun)
        {
            sunFlipRedo.interactable = true;
            Utils.DisablePieces(moonPieces);
            GameObject[] unPlayedPieces = Utils.GetUnplayedPieces(sunPieces);
            Utils.EnablePieces(unPlayedPieces);
        }
        else
        {
            moonFlipRedo.interactable = true;
            Utils.DisablePieces(sunPieces);
            GameObject[] unPlayedPieces = Utils.GetUnplayedPieces(moonPieces);
            Utils.EnablePieces(unPlayedPieces);
        }
        //Record the flip
        //string s = Utils.CreateStringFromFlip(obj);
        //record.Push(s);
    }

    public void RecordMove(string str)
    {
        record.Push(str);
    }

    //Deactivate all of the buttons for both players
    public void DeactivateButtons()
    {
        sunFlipRedo.interactable = false;
        sunEndTurn.interactable = false;
        sunChangeColor.interactable = false;
        sunForfeit.interactable = false;
        moonFlipRedo.interactable = false;
        moonEndTurn.interactable = false;
        moonChangeColor.interactable = false;
        //moonForfeit.interactable = false;
}


    //======================================================================
    //Button Presses========================================================
    //======================================================================
    //When called, this flips the active piece from gold to silver or silver to gold
    //Can only flip own piece though
    public void FlipSideOfActivePiece()
    {
        if (activePiece != null)
        {
            if((activePlayer == Player.sun && activePiece.tag == "Sun") ||
                (activePlayer == Player.moon && activePiece.tag == "Moon"))
            {
                //print(activePlayer + "  " + activePiece.name);
                activePiece.GetComponent<GamePiece>().Flip();

                //Side side = activePiece.GetComponent<GamePiece>().side;
                //if (side == Side.gold)
                //    activePiece.GetComponent<GamePiece>().side = Side.silver;
                //else
                //    activePiece.GetComponent<GamePiece>().side = Side.gold;
            }
            
        }
    }

    //Switches the active player when the "End" button is pressed
    public void SwitchPlayers()
    {
        bool winner = Board.S.CheckForWinner();
        if (!winner)
        {
            DeactivateButtons();
            //print(activePiece.name);
            if (activePiece != null)
            {
                RecordMove(Utils.CreateStringFromPlacement(activePiece));
                activePiece.GetComponent<GamePiece>().ChangeMovement();
            }

            if (activePlayer == Player.sun)
            {   //Make moon the active player           
                Utils.DimObject(sunTable);
                Utils.UndimObject(moonTable);
                activePlayer = Player.moon;
                Utils.DisablePieces(sunPieces);
                Utils.DisablePieces(moonPieces);
                GameObject[] flippablePieces = Utils.GetFlippablePieces(sunPieces);
                if (flippablePieces.Length == 0) //No flippable pieces
                {
                    GameObject[] unPlayedPieces = Utils.GetUnplayedPieces(moonPieces);
                    Utils.EnablePieces(unPlayedPieces);
                }
                else
                {
                    Utils.EnablePieces(flippablePieces);
                }
            }
            else
            {   //Make Sun the active player
                Utils.DimObject(moonTable);
                Utils.UndimObject(sunTable);
                activePlayer = Player.sun;
                Utils.DisablePieces(moonPieces);
                Utils.DisablePieces(sunPieces);
                GameObject[] flippablePieces = Utils.GetFlippablePieces(moonPieces);
                if (flippablePieces.Length == 0) //No flippable pieces
                {
                    GameObject[] unPlayedPieces = Utils.GetUnplayedPieces(sunPieces);
                    Utils.EnablePieces(unPlayedPieces);
                }
                else
                {
                    Utils.EnablePieces(flippablePieces);
                }
            }
        }
        else //If a winner is detected
        {
            //Display who won and lock all of the pieces.  
            //Display Play again or Quit or Replay
            DeactivateButtons();
            Utils.DisablePieces(moonPieces);
            Utils.DisablePieces(sunPieces);
            //Display LOSER text to the loser facing the loser and WINNER text to winner
            //This text comes in from the left hand side
            //Then in the middle fade in the end-game options
        }

    }

    public void PrintOutRecord()
    {
        Board.S.PrintOccupants();
        //for(int i=0; i<record.Count; i++)
        //while(record.Count > 0)
        //{
        //    print(record.Pop());
        //}
    }

    //Undo a Flip, place last piece flipped to where it was before the flip
    //This is only active right after a flip took place so top entry in
    //record will be a "Flip" move
    public void UndoFlip()
    {
        if(activePlayer == Player.sun)
        {         
            sunFlipRedo.interactable = false;
            GameObject[] flippables = Utils.GetFlippablePieces(moonPieces);
            Utils.EnablePieces(flippables);
            GameObject[] unPlayedPieces = Utils.GetUnplayedPieces(sunPieces);
            Utils.DisablePieces(unPlayedPieces);
        }
        else
        {
            moonFlipRedo.interactable = false;
            GameObject[] flippables = Utils.GetFlippablePieces(sunPieces);
            Utils.EnablePieces(flippables);
            GameObject[] unPlayedPieces = Utils.GetUnplayedPieces(moonPieces);
            Utils.DisablePieces(unPlayedPieces);
        }
        string flipInfo = record.Pop();
        Dictionary<string, string> parsedInfo = Utils.ParseRecord(flipInfo);
        int index;
        int.TryParse(parsedInfo["fromIndex"], out index);      
        activePiece.GetComponent<GamePiece>().MoveToPositionByIndex(index);
        record.Pop(); //need to pop again since moving a piece back to original spot records the move automatically
       
    }

    



    
}
