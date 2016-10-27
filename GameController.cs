using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum Player
{
    sun,
    moon,
    sunNPC,
    moonNPC
}

public enum Phase
{
    flip,
    movePiece,
    changeSide
}

public class ZoomTextTemplate
{
    public GameObject obj { get; set; }
    public Vector3 start { get; set; }
    public Vector3 mid { get; set; }
    public Vector3 end { get; set; }
    public bool show { get; set; }
    public bool reachedMidPoint { get; set; }
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

    //Text that comes on and off of the screen
    public GameObject sunTurnText;
    public GameObject moonTurnText;
    public GameObject winnerTextPrefab;
    public GameObject loserTextPrefab;

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

    //Zoom zoom objects
    ZoomTextTemplate sunZoom;
    ZoomTextTemplate moonZoom;
    ZoomTextTemplate winnerZoom;
    ZoomTextTemplate loserZoom;

    void Awake()
    {
        S = this;
        sunZoom = new ZoomTextTemplate { obj = sunTurnText, start = new Vector3(6, 1.6f, 0), mid = new Vector3(0, 1.6f, 0), end = new Vector3(-6, 1.6f, 0), show = false, reachedMidPoint = false };
        moonZoom = new ZoomTextTemplate { obj = moonTurnText, start = new Vector3(-6, -1.6f, 0), mid = new Vector3(0, -1.6f, 0), end = new Vector3(6, -1.6f, 0), show = false, reachedMidPoint = false };
        winnerZoom = new ZoomTextTemplate { show = false, reachedMidPoint = false };
        loserZoom = new ZoomTextTemplate { show = false, reachedMidPoint = false };
    }

    // Use this for initialization
    void Start ()
    {
        CreatePieces();
        //Board.S.Test(sunPieces);
        InitializePositions();
        SwitchPlayers();       
        record = new Stack<string>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (sunZoom.show)
        {
            StartCoroutine(ZoomText(sunZoom));
        }
        if (moonZoom.show)
        {
            StartCoroutine(ZoomText(moonZoom));
        }
        if(winnerZoom.show)
        {
            winnerZoom.show = !MoveToLocation(winnerZoom.obj, winnerZoom.end);
            //StartCoroutine(ZoomText(winnerZoom));
        }
        if (loserZoom.show)
        {
            loserZoom.show = !MoveToLocation(loserZoom.obj, loserZoom.end);
            //StartCoroutine(ZoomText(loserZoom));
        }
    }

    //Zoom text to a position, wait for 1 second, then zoom to another position
    IEnumerator ZoomText(ZoomTextTemplate zoomObj)
    {
        //print(sunZoom.reachedMidPoint);
        while (zoomObj.reachedMidPoint == false)
        {
            zoomObj.reachedMidPoint = MoveToLocation(zoomObj.obj, zoomObj.mid);
            yield return null;
        }
        yield return new WaitForSeconds(1);
        zoomObj.show = !MoveToLocation(zoomObj.obj, zoomObj.end);
    }

    //Lerp GameObject from current position to some end position
    //Returns true when reaches position, false otherwise
    bool MoveToLocation(GameObject obj, Vector3 end)
    {
        bool atEnd = Utils.MoveLerp(obj, obj.transform.position, end);
        if (atEnd) { return true; }       
        return false;
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
                activePiece.GetComponent<GamePiece>().Flip();
            }
            
        }
    }

    //Switches the active player when the "End" button is pressed
    public void SwitchPlayers()
    {
        string winner = Board.S.CheckForWinner();
        if (winner == "")//what if all pieces have been used and still no winners?
        {
            DeactivateButtons();
            //print(activePiece.name);
            //Record the last move the previous player played to the board
            if (activePiece != null)
            {
                RecordMove(Utils.CreateStringFromPlacement(activePiece));
                activePiece.GetComponent<GamePiece>().ChangeMovement();
            }



            if (activePlayer == Player.sun)
            {   //Make moon the active player 
                moonZoom.obj.transform.position = moonZoom.start;
                moonZoom.show = true;
                moonZoom.reachedMidPoint = false;
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
                sunZoom.obj.transform.position = sunZoom.start;
                sunZoom.show = true;
                sunZoom.reachedMidPoint = false;
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
            if(winner == "Sun")
            {
                GameObject winnerText = Instantiate(winnerTextPrefab) as GameObject;
                GameObject loserText = Instantiate(loserTextPrefab) as GameObject;
                Vector3 scale = winnerText.transform.localScale;
                scale.x *= -1;
                scale.y *= -1;
                winnerText.transform.localScale = scale;
                winnerText.transform.position = new Vector3(6, 1.6f, 0);
                loserText.transform.position = new Vector3(-6, -1.6f, 0);
                winnerZoom = new ZoomTextTemplate { obj = winnerText, start = new Vector3(6, 1.6f, 0), mid = new Vector3(0, 1.6f, 0), end = new Vector3(0, 1.6f, 0), show = true, reachedMidPoint = false };
                loserZoom = new ZoomTextTemplate { obj = loserText, start = new Vector3(-6, -1.6f, 0), mid = new Vector3(0, -1.6f, 0), end = new Vector3(0, -1.6f, 0), show = true, reachedMidPoint = false };
            }
            else if (winner == "Moon")
            {
                GameObject winnerText = Instantiate(winnerTextPrefab) as GameObject;
                GameObject loserText = Instantiate(loserTextPrefab) as GameObject;
                Vector3 scale = loserText.transform.localScale;
                scale.x *= -1;
                scale.y *= -1;
                loserText.transform.localScale = scale;
                winnerText.transform.position = new Vector3(-6, -1.6f, 0);
                loserText.transform.position = new Vector3(6, 1.6f, 0);
                loserZoom = new ZoomTextTemplate { obj = loserText, start = new Vector3(6, 1.6f, 0), mid = new Vector3(0, 1.6f, 0), end = new Vector3(0, 1.6f, 0), show = true, reachedMidPoint = false };
                winnerZoom = new ZoomTextTemplate { obj = winnerText, start = new Vector3(-6, -1.6f, 0), mid = new Vector3(0, -1.6f, 0), end = new Vector3(0, -1.6f, 0), show = true, reachedMidPoint = false };
            }
            else
            {

            }

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
