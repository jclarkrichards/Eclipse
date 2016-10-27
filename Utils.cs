using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Utils : MonoBehaviour
{

    //Input an index in a 16-element array and return all indices that are adjacent to it
    //Uses an adjacency matrix 
    public static int[] GetAdjacentIndices(int index)
    {
        List<int> adjacentIndices = new List<int>();
        int[,] matrix = new int[16,16] { { 0,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0 },
                                         { 1,0,1,0,0,1,0,0,0,0,0,0,0,0,0,0 },
                                         { 0,1,0,1,0,0,1,0,0,0,0,0,0,0,0,0 },
                                         { 0,0,1,0,0,0,0,1,0,0,0,0,0,0,0,0 },
                                         { 1,0,0,0,0,1,0,0,1,0,0,0,0,0,0,0 },
                                         { 0,1,0,0,1,0,1,0,0,1,0,0,0,0,0,0 },
                                         { 0,0,1,0,0,1,0,1,0,0,1,0,0,0,0,0 },
                                         { 0,0,0,1,0,0,1,0,0,0,0,1,0,0,0,0 },
                                         { 0,0,0,0,1,0,0,0,0,1,0,0,1,0,0,0 },
                                         { 0,0,0,0,0,1,0,0,1,0,1,0,0,1,0,0 },
                                         { 0,0,0,0,0,0,1,0,0,1,0,1,0,0,1,0 },
                                         { 0,0,0,0,0,0,0,1,0,0,1,0,0,0,0,1 },
                                         { 0,0,0,0,0,0,0,0,1,0,0,0,0,1,0,0 },
                                         { 0,0,0,0,0,0,0,0,0,1,0,0,1,0,1,0 },
                                         { 0,0,0,0,0,0,0,0,0,0,1,0,0,1,0,1 },
                                         { 0,0,0,0,0,0,0,0,0,0,0,1,0,0,1,0 } };

        for(int i=0; i<16; i++)
        {
            if(matrix[index, i] == 1)
            {
                adjacentIndices.Add(i);
            }
        }
        return adjacentIndices.ToArray();

    }

    //Disables the collider on the Moon and/or Sun pieces so they aren't clickable
    //Can include any subset of the moon and/or sun pieces
    public static void DisablePieces(GameObject[] pieces)
    {
        for(int i=0; i<pieces.Length; i++)
        {
            DisablePiece(pieces[i]);
            //pieces[i].GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    //Enables the collider on the Moon and/or Sun pieces so they are clickable
    //Can include any subset of the moon and/or sun pieces
    public static void EnablePieces(GameObject[] pieces)
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            EnablePiece(pieces[i]);
            //pieces[i].GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    //Enable a single piece
    public static void EnablePiece(GameObject piece)
    {    
        piece.GetComponent<BoxCollider2D>().enabled = true;      
    }

    public static void DisablePiece(GameObject piece)
    {
        piece.GetComponent<BoxCollider2D>().enabled = false;
    }

    //Returns a subset of pieces that haven't been played
    public static GameObject[] GetUnplayedPieces(GameObject[] pieces)
    {
        List<GameObject> result = new List<GameObject>();
        for(int i=0; i<pieces.Length; i++)
        {
            if(pieces[i].GetComponent<GamePiece>().occupantIndex == -1)
            {
                result.Add(pieces[i]);
            }
        }
        return result.ToArray();
    }

    //Returns a subset of pieces that have been played on the Board
    public static GameObject[] GetPlayedPieces(GameObject[] pieces)
    {
        List<GameObject> result = new List<GameObject>();
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].GetComponent<GamePiece>().occupantIndex != -1)
            {
                result.Add(pieces[i]);
            }
        }
        return result.ToArray();
    }

    //Return a subset of pieces that are flippable
    //Input all 8 pieces of either sun or moon
    //Find the pieces that have been played
    //From the played pieces, find the indices that are adjacent to piece
    //From adjacent indices check if any are empty
    //If empty, then add that piece to the list of flippables
    public static GameObject[] GetFlippablePieces(GameObject[] go)
    {
        List<GameObject> flippables = new List<GameObject>();
        GameObject[] playedPieces = GetPlayedPieces(go);
        for(int i=0; i<playedPieces.Length; i++)
        {
            int index = playedPieces[i].GetComponent<GamePiece>().occupantIndex;
            int[] adjacentIndices = GetAdjacentIndices(index);

            if (Board.S.CheckIfAvailableSpot(adjacentIndices))
            {
                flippables.Add(playedPieces[i]);
            }
        }
        return flippables.ToArray();
    }

    //public static bool CheckIfFlippablePiece(GameObject go)
    //{

    //}

    //This is for recording the Flip, just put the useful information in a string
    //put in form:  "Flip,Moon4,10,6" or Moon4 was flipped to space 6 from space 10
    public static string CreateStringFromFlip(GameObject obj, int oldIndex, int newIndex)
    {
        string s = "Flip,"+obj.name + "," + oldIndex.ToString() + "," + newIndex.ToString();
        return s;
    }

    //This is for recording the Placement of a piece to the board
    //Put in form:  "Place,Sun2,silver,3" or place Sun2 silver side up on square 3
    public static string CreateStringFromPlacement(GameObject obj)
    {
        string s = "Place,"+obj.name + "," + obj.GetComponent<GamePiece>().side + "," +
                   obj.GetComponent<GamePiece>().occupantIndex.ToString();
        return s;
    }

    //Parse the record string into a dictionary
    //For "Flip":  record[0]=="Flip", record[1]==name, record[2]==FlippedFromIndex, record[3]==FlippedToIndex
    //For "Place": record[0]=="Place", record[1]==name, record[2]==side, record[3]==BoardIndex
    public static Dictionary<string, string> ParseRecord(string record)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        Dictionary<int, string> blah = new Dictionary<int, string>();
        string[] recordArray = record.Split(',');
        result.Add("name", recordArray[1]);
        result.Add("toIndex", recordArray[3]);

        if (recordArray[0] == "Flip")
        {
            result.Add("side", "null");
            result.Add("fromIndex", recordArray[2]);
            
        }
        else if(recordArray[0] == "Place")
        {
            result.Add("side", recordArray[2]);
            result.Add("fromIndex", "null");
        }
        return result;

    }

    public static void DimObject(GameObject obj)
    {
        //print("Dimming the table:" +obj.name);
        Color color = obj.GetComponent<SpriteRenderer>().color;
        color.a = 0.2f;
        obj.GetComponent<SpriteRenderer>().color = color;
        //return obj;
    }

    public static void UndimObject(GameObject obj)
    {
        Color color = obj.GetComponent<SpriteRenderer>().color;
        color.a = 1;
        obj.GetComponent<SpriteRenderer>().color = color;
        //return obj;
    }

    public static Vector3 Interpolate(Vector3 p0, Vector3 p1)
    {   //Linearly interpolate between two positions
        float u = 0.1f;
        return (1 - u) * p0 + u * p1;
    }

    public static bool MoveLerp(GameObject obj, Vector3 start, Vector3 end)
    {   //Used to move obj from start position to end position
        Vector3 pos = Interpolate(start, end);
        obj.transform.position = pos;
        return ReachedEndPosition(start, end);
    }

    public static bool ReachedEndPosition(Vector3 start, Vector3 end)
    {
        float thresh = 0.05f;
        Vector3 diffVec = end - start;
        if (Mathf.Abs(diffVec.x) < thresh && Mathf.Abs(diffVec.y) < thresh && Mathf.Abs(diffVec.z) < thresh)
        {
            return true;
        }
        return false;
    }

}
