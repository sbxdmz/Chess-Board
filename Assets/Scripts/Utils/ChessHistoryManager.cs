using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessHistoryManager : MonoBehaviour
{
    public List<ChessMove> moveHistory = new List<ChessMove>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void RecordMove(Vector2Int origin, Vector2Int destination, moveType MT, Piece capturedPiece, Piece movingPiece, ChessPlayer team, bool causedCheck, bool causedCheckmate){
        ChessMove newMove = new ChessMove(origin, destination, MT, capturedPiece, movingPiece, team, causedCheck, causedCheckmate);
        moveHistory.Add(newMove);
        Debug.Log(newMove.GetAlgebraicNotation());
    }


    
}
[System.Serializable]
public class ChessMove{
    Vector2Int origin;
    Vector2Int destination;
    moveType MT;
    Piece movingPiece;
    Piece capturedPiece;
    ChessPlayer team;
    bool causedCheck;
    bool causedCheckmate;
    public ChessMove(Vector2Int origin, Vector2Int destination, moveType MT, Piece capturedPiece, Piece movingPiece, ChessPlayer team, bool causedCheck, bool causedCheckmate){
        this.origin = origin;
        this.destination = destination;
        this.MT = MT;
        this.capturedPiece = capturedPiece;
        this.movingPiece = movingPiece;
        this.team = team;
        this.causedCheck = causedCheck;
        this.causedCheckmate = causedCheckmate;
    }
    public string getPieceAbbr(Piece piece){
        switch (piece.GetType().Name){
            case "Pawn": return ""; 
            case "Knight": return "N";
            case "King": return "K";
            case "Bishop": return "B";
            case "Queen": return "Q"; 
        }
        return null;
    }
    public string getSquare(Vector2Int coords){
        string result = "";
        switch (coords.x){
            case 0: result += "a"; break;
            case 1: result += "b"; break;
            case 2: result += "c"; break;
            case 3: result += "d"; break;
            case 4: result += "e"; break;
            case 5: result += "f"; break;
            case 6: result += "g"; break;
            case 7: result += "h"; break;
        }
        result += (coords.y + 1);

        return result;
    }
    public string GetAlgebraicNotation(){
        string captureString = "";
        string checkString = "";
        string originString = "";
        string originSquare = getSquare(origin);

        if(capturedPiece != null){
            if(movingPiece.GetType().Name == "Pawn"){
                originString = originSquare.Substring(0,1);
            }
            captureString = "x";
        }
        if(causedCheckmate){
            checkString = "#";
        }
        else if(causedCheck){
            checkString = "+";
        }

        if(MT == moveType.normal){
        return originString + getPieceAbbr(movingPiece) + captureString + getSquare(destination) + checkString;
        }
        else if(MT == moveType.shortCastle){
            return "O-O" + checkString;
        }
        else if(MT == moveType.longCastle){
            return "O-O-O" + checkString;
        }
        return null;
    }
    public string GetSentence(){
        return null;
    }

}