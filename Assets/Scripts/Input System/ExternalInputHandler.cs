using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ExternalInputHandler : MonoBehaviour
{
    public Board board;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(wait());
        IEnumerator wait(){
            
            yield return new WaitForSeconds(1);
            MakeMove("e2e4");
            MakeMove("e7e5");
            MakeMove("g1f3");
            MakeMove("b8c6");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool MakeMove(string move){
        board.DeselectAll();
        string firstSquareString = move.Substring(0, 2);
        string secondSquareString = move.Substring(2, 2);
        Vector2Int firstSquare = GetSquareFromString(firstSquareString);
        Vector2Int secondSquare = GetSquareFromString(secondSquareString);
        selectedStatus status = board.OnSquareSelected(firstSquare);
        if(status == selectedStatus.invalid || status == selectedStatus.deselect){
            // Debug.Log("Invalid");
            board.DeselectAll();
            return false;
        } 
        status = board.OnSquareSelected(secondSquare);
        if(status != selectedStatus.move){
            // Debug.Log("Invalid");
            board.DeselectAll();
            return false;
        } 
        // Debug.Log("Made Move");
        return true;        
    }

    public Vector2Int GetSquareFromString(string squareString){
        char firstChar = squareString[0];
        char secondChar = squareString[1];

        int x = 0;
        switch(firstChar){
            case 'a': x = 0; break;
            case 'b': x = 1; break;
            case 'c': x = 2; break;
            case 'd': x = 3; break;
            case 'e': x = 4; break;
            case 'f': x = 5; break;
            case 'g': x = 6; break;
            case 'h': x = 7; break;
        }

        int y = (int) Char.GetNumericValue(secondChar) - 1;

        Vector2Int result = new Vector2Int(x, y);
        // Debug.Log(result);
        return result;
    }
}
