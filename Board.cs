using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum Piece
{
    empty,
    sunGold,
    sunSilver,
    moonGold,
    moonSilver
}

//This class keeps track of each of the 16 spots on the board and who occupies each spot
public class Board : MonoBehaviour
{
    public static Board S;  //There is only one board
    Vector3[] positions;
    Piece[] occupants;
    int numPositions = 16;
    float thresh = 0.8f;

	// Use this for initialization
	void Awake ()
    {
        S = this;
        occupants = new Piece[numPositions];
        SetPlayPositions();
	}

    //This was just to test to see if the positions were correct
    public void Test(GameObject[] go)
    {
        for(int i=0; i<go.Length; i++)
        {
            go[i].transform.position = positions[i];
        }
    }

    //Define the Vectors3s that indicate the positions where the pieces can be placed
    void SetPlayPositions()
    {
        Vector3 scale = transform.localScale;
        positions = new Vector3[numPositions];
        for (int i = 0; i < numPositions / 4; i++)  //0-4
        {
            for (int j = 0; j < numPositions / 4; j++)  //0-4
            {
                float x = (0.85f + 1.50f * j - 3.1f) * scale.x;
                float y = (-0.85f - 1.50f * i + 3.1f) * scale.y;
                positions[4 * i + j] = new Vector3(x, y, 0);

            }
        }
    }

    

    //When a piece is released, this is called to check if the piece's position is within the threshold
    //Loops through all positions and finds the first position that meets the criteria
    //If it is, then we set that position as the piece's new position and end and return true
    //If it isn't within threshold then we just return false
    public bool CheckIfValidSpot(GameObject obj)
    {
        for(int i=0; i<positions.Length; i++)
        {
            if(Mathf.Abs(obj.transform.position.x - positions[i].x) <= thresh)
            {
                if(Mathf.Abs(obj.transform.position.y - positions[i].y) <= thresh)
                {
                    //Or set positions[i] as the end Lerp position  
                    //It is within the thresh of a spot, so check for occupancy before placing
                    
                    bool occupied = CheckIfOccupied(obj, i);
                    if (!occupied)
                    {
                        //print(occupied);
                        obj.transform.position = positions[i];
                        return true;
                    }                    
                    return false;                
                }        
            }
        }
        return false;                   
    }

    //Checks if an adjacent spot next to GameObject is valid
    public bool CheckIfAdjacentValidSpot(GameObject obj)
    {
        int index = obj.GetComponent<GamePiece>().occupantIndex;
        //print("Current Index = " + index);
        int[] adjacentIndices = Utils.GetAdjacentIndices(index);
        //for (int i = 0; i < adjacentIndices.Length; i++)
        //    print("Adjacent Index = " +adjacentIndices[i]);
        for (int i = 0; i < adjacentIndices.Length; i++)
        {
            if (Mathf.Abs(obj.transform.position.x - positions[adjacentIndices[i]].x) <= thresh)
            {
                if (Mathf.Abs(obj.transform.position.y - positions[adjacentIndices[i]].y) <= thresh)
                {
                    bool occupied = CheckIfOccupied(obj, adjacentIndices[i]);
                    if (!occupied)
                    {
                        //print(occupied);
                        obj.transform.position = positions[adjacentIndices[i]];
                        return true;
                    }
                    return false;
                }
            }
        }
        return false;               
    }

    //Only checks if any index in occupants in "indices" is occupied or not.  Doesn't modify anything.
    //Returns true if any spot in "index" is empty, false otherwise.
    public bool CheckIfAvailableSpot(int[] indices)
    {
        for(int i=0; i<indices.Length; i++)
        {
            if (occupants[indices[i]] == Piece.empty)
                return true;
        }
        return false;
    }

    //Checks if indices adjacent to each index is occupied or not
    //If all adjacent indices are occupied, then return true (locked)
    public bool CheckIfLocked(int[] indices)
    {
        for (int i = 0; i < indices.Length; i++)
        {
            int[] adjacents = Utils.GetAdjacentIndices(indices[i]);
            for(int j=0; j<adjacents.Length; j++)
            {
                if (occupants[adjacents[j]] == Piece.empty)
                    return false;
            }
            
        }
        return true;
    }

    //This updates the "occupants" array.  Is called by CheckIfWithinThresh whenever a piece is within thresh
    //Returns true if spot is occupied, false if spot is empty
    public bool CheckIfOccupied(GameObject obj, int index)
    {
        if(occupants[index] == Piece.empty) //Spot is empty
        {
            //tag can only either be "Moon" or "Sun"
            UpdateOccupants(obj, index);          
            RemoveOccupant(obj);
            //Record the GamePiece's old position and new position
            if (obj.GetComponent<GamePiece>().moveType == Movement.flip)
            {
                int oldIndex = obj.GetComponent<GamePiece>().occupantIndex;
                GameController.S.RecordMove(Utils.CreateStringFromFlip(obj, oldIndex, index));
            }
            obj.GetComponent<GamePiece>().occupantIndex = index;

            return false;
        }               
        return true;
    }

    //Remove an occupant from the occupants array
    public void RemoveOccupant(GameObject obj)
    {
        int index = obj.GetComponent<GamePiece>().occupantIndex;
        if(index != -1)
        {
            occupants[index] = Piece.empty;
        }
    }

    //A helper method that just prints out the contents of the occupants array
    public void PrintOccupants()
    {
        string str = "";
        for(int i=0; i<occupants.Length; i++)
        {
            str += occupants[i] + ", ";
        }
        print(str);
    }

    public Vector3 GetPositionByIndex(int index)
    {
        return positions[index];
    }

    public void UpdateOccupants(GameObject obj, int index)
    {
        //int index = obj.GetComponent<GamePiece>().occupantIndex;
        if (obj.tag == "Moon")
        {
            if (obj.GetComponent<GamePiece>().side == Side.gold)
            {
                occupants[index] = Piece.moonGold;
            }
            else
            {
                occupants[index] = Piece.moonSilver;
            }
        }
        else
        {
            if (obj.GetComponent<GamePiece>().side == Side.gold)
            {
                occupants[index] = Piece.sunGold;
            }
            else
            {
                occupants[index] = Piece.sunSilver;
            }
        }
    }

    //Check the occupants array to see if there is a winner
    //A winner is when there are 3 same values in a row either horizontally, vertically, or diagonally
    //These 3 values also have to be "locked", meaning that there aren't any empty values adjacent to 
    //any of them
    public string CheckForWinner()
    {
        //Get 3x3 subsets of the 4x4 occupants array, these contain the indices to the occupants array
        List<Piece> winners = new List<Piece>();
        List<string> uniqueWinners = new List<string>();
        int[] A = new int[9] { 0, 1, 2, 4, 5, 6, 8, 9, 10 };
        winners = CheckHorizontally(A, winners);
        winners = CheckVertically(A, winners);
        winners = CheckDiagonally(A, winners);
        A = new int[9] { 1, 2, 3, 5, 6, 7, 9, 10, 11 };
        winners = CheckHorizontally(A, winners);
        winners = CheckVertically(A, winners);
        winners = CheckDiagonally(A, winners);
        A = new int[9] { 4, 5, 6, 8, 9, 10, 12, 13, 14 };
        winners = CheckHorizontally(A, winners);
        winners = CheckVertically(A, winners);
        winners = CheckDiagonally(A, winners);
        A = new int[9] { 5, 6, 7, 9, 10, 11, 13, 14, 15 };
        winners = CheckHorizontally(A, winners);
        winners = CheckVertically(A, winners);
        winners = CheckDiagonally(A, winners);

        for(int i=0; i<winners.Count; i++)
        {
            switch(winners[i])
            {
                case Piece.moonGold:
                case Piece.moonSilver:
                    if (!uniqueWinners.Contains("Moon"))
                    {
                        uniqueWinners.Add("Moon");
                    }
                    break;
                case Piece.sunGold:
                case Piece.sunSilver:
                    if (!uniqueWinners.Contains("Sun"))
                    {
                        uniqueWinners.Add("Sun");
                    }
                    break;
            }
        }

        if (uniqueWinners.Count == 0)
        {
            print("No winners yet");
            return "";
        }
        else if(uniqueWinners.Count == 1)
        {
            for (int i = 0; i < winners.Count; i++)
            {
                print("Winner = " + winners[i]);
            }
            return uniqueWinners[0];
        }
        else
        {
            print("game is tied");
            return "Tied";
        }
    }

    //Check for horizontal wins
    List<Piece> CheckHorizontally(int[] A, List<Piece> winners)
    {
        
        int[] pair = new int[3];
        for (int i = 0; i < A.Length; i+=3)
        {
            
            Piece a1 = occupants[A[i]];
            Piece a2 = occupants[A[i + 1]];
            Piece a3 = occupants[A[i + 2]];
            if((a1 != 0) && ((a1-a2)==0) && ((a1-a3)==0))
            {
                print("Found three of a kind horizontally");
                pair = new int[3] { A[i], A[i + 1], A[i + 2] };
                bool locked = CheckIfLocked(pair);
                if(locked)
                {
                    //Add value of occupants[A[i]] to a winners list
                    winners.Add(occupants[A[i]]);
                }             
            }
        }
        return winners;
    }

    //Check for vertical wins
    List<Piece> CheckVertically(int[] A, List<Piece> winners)
    {     
        int[] pair = new int[3];
        for (int i = 0; i < 3; i++)
        {
            Piece a1 = occupants[A[i]];
            Piece a2 = occupants[A[i + 3]];
            Piece a3 = occupants[A[i + 6]];
            if ((a1 != 0) && ((a1 - a2) == 0) && ((a1 - a3) == 0))
            {
                print("Found three of a kind vertically");
                pair = new int[3] { A[i], A[i + 3], A[i + 6] };
                bool locked = CheckIfLocked(pair);
                if (locked)
                {
                    //Add value of occupants[A[i]] to a winners list
                    winners.Add(occupants[A[i]]);
                }
            }
        }
        return winners;
    }

    //Check for diagonal wins
    List<Piece> CheckDiagonally(int[] A, List<Piece> winners)
    {
        int[] pair = new int[3];
        Piece a1 = occupants[A[0]];
        Piece a2 = occupants[A[4]];
        Piece a3 = occupants[A[8]];
        if ((a1 != 0) && ((a1 - a2) == 0) && ((a1 - a3) == 0))
        {
            print("Found three of a kind diagonally");
            pair = new int[3] { A[0], A[4], A[8] };
            bool locked = CheckIfLocked(pair);
            if (locked)
            {
                //Add value of occupants[A[i]] to a winners list
                winners.Add(occupants[A[0]]);
            }
        }

        a1 = occupants[A[2]];
        a2 = occupants[A[4]];
        a3 = occupants[A[6]];
        if ((a1 != 0) && ((a1 - a2) == 0) && ((a1 - a3) == 0))
        {
            print("Found three of a kind diagonally");
            pair = new int[3] { A[2], A[4], A[6] };
            bool locked = CheckIfLocked(pair);
            if (locked)
            {
                //Add value of occupants[A[i]] to a winners list
                winners.Add(occupants[A[2]]);
            }
        }

        return winners;
    }
}
