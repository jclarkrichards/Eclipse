using UnityEngine;
using System.Collections;

public enum Side
{
    gold,
    silver
}

public enum Movement
{
    normal, 
    flip
}

//This is attached to each Moon and Sun piece as a component
//Each piece is a parent to two children called "Gold" and "Silver"
public class GamePiece : MonoBehaviour
{
    [HideInInspector]
    public Transform[] children;
    
    //Private variables.  Only GamePiece knows about them
    Side _side = Side.gold;
    int _occupantIndex = -1; //For Board's occupant array
    Vector3 homePosition;
    Vector3 followOffset;
    bool followMouse;
    bool locked;
    Movement _moveType = Movement.normal;


    void Awake()
    {
        children = new Transform[2];
        children[0] = transform.FindChild("Gold");
        children[1] = transform.FindChild("Silver");
        SetFaceUpSide(side);

    }
	
	// Update is called once per frame
	void Update ()
    {
        if(followMouse)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //print(mousePosition);
            mousePosition.z = 0.0f;
            transform.position = followOffset + mousePosition;
        }
	
	}

    //This is only called once, on first instance of this piece being selected
    //Change layer to "Selected" so it appears on top of all other pieces
    //Determine the "followOffset" vector so the piece is dragged from where it is touched
    //Need to remove it from the Board's occupants array if it is in that array
    //Let the GameController know that this is the active piece.
    void OnMouseDown()  
    {
        //print(transform.name);
        children[0].gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Selected";
        children[1].gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Selected";
        followOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        followMouse = true;
        GameController.S.SetActivePiece(gameObject);
        
    }

    //Return the children to their original sorting layers of "Gold" and "Silver"
    //No longer following the mouse
    //Check if released position is within threshold of allowed positions on Board
    //If not, then return piece to home position, if it is then set that spot as the 
    //pieces new home position.
    void OnMouseUp()
    {
        
        children[0].gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Gold";
        children[1].gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Silver";
        followMouse = false;

        //Needs to check if within threshold of a valid position
        if (moveType == Movement.normal)
        {
            //print("can move to any spot");
            bool validSpot = Board.S.CheckIfValidSpot(gameObject);
            if (!validSpot)
            {
                //print("Not a valid spot");
                transform.position = homePosition;
            }
            else
            {
                //print("Is a valid spot");
                homePosition = transform.position;
                GameController.S.ReportPiecePlacedOnBoard(gameObject);
                
            }
        }
        else if(moveType == Movement.flip)
        {
            //print("Can only move to adjacent spot");
            //valid spots are those that are adjacent to this piece
            bool validSpot = Board.S.CheckIfAdjacentValidSpot(gameObject);
            if(!validSpot)
            {
                transform.position = homePosition;
            }
            else
            {
                //print("Is a valid spot");
                homePosition = transform.position;
                Flip();
                GameController.S.ReportPieceFlipped(gameObject);
            }
        }
    }

    //Move this piece to a postion on the board given an index
    public void MoveToPositionByIndex(int index)
    {
        Vector3 position = Board.S.GetPositionByIndex(index);
        transform.position = position;
        homePosition = position;
        bool validSpot = Board.S.CheckIfAdjacentValidSpot(gameObject);

        Flip();
        Utils.EnablePiece(gameObject);
    }

    //This sets the home position for this piece based on the position of the tray
    //Defines a position that is within a certain range of the tray
    public void SetHomePosition(GameObject tray)
    {
        transform.position = tray.transform.position;
        homePosition = tray.transform.position;
        //Vector3 traySize = tray.GetComponent<SpriteRenderer>().bounds.size;
        //float mpx = Random.Range(-traySize.x / 4.0f, traySize.x / 4.0f);
        //float mpy = Random.Range(-traySize.y / 4.0f, traySize.y / 4.0f);
        //transform.position = tray.transform.position + new Vector3(mpx, mpy, 0);
        //homePosition = tray.transform.position + new Vector3(mpx, mpy, 0);
    }

    //Changes the color of this piece from gold to silver or silver to gold
    public void Flip()
    {
        if (side == Side.gold) { side = Side.silver; }
        else { side = Side.gold; }     
        Board.S.UpdateOccupants(gameObject, occupantIndex);
    }

    public void ChangeMovement()
    {
        moveType = Movement.flip;
    }

    //Set the active side of the piece (the face-up side) either Gold or Silver
    public void SetFaceUpSide(Side side)
    {
        if(side == Side.gold)
        {
            children[0].gameObject.SetActive(true);
            children[1].gameObject.SetActive(false);
            side = Side.gold;
        }
        else
        {
            children[0].gameObject.SetActive(false);
            children[1].gameObject.SetActive(true);
            side = Side.silver;
        }
    }

    public Side side
    {
        set
        {
            _side = value;
            SetFaceUpSide(_side);
        }
        get { return _side; }
    }

    public int occupantIndex
    {
        set { _occupantIndex = value; }
        get { return _occupantIndex; }
    }

    public Movement moveType
    {
        set { _moveType = value; }
        get { return _moveType; }
    }

    
}
