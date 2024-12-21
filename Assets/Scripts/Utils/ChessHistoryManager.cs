using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ChessHistoryManager : MonoBehaviour
{   
    public TextMeshProUGUI PGNText;
    public List<ChessMove> moveHistory = new List<ChessMove>();
    public TTSManager tts;
    private string PGNString;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void RecordMove(Vector2Int origin, Vector2Int destination, moveType MT, bool capturedPiece, Piece movingPiece, ChessPlayer team, bool causedCheck, bool causedCheckmate){
        ChessMove newMove = new ChessMove(origin, destination, MT, capturedPiece, movingPiece, team, causedCheck, causedCheckmate);
        moveHistory.Add(newMove);
        tts.AnnounceMove(newMove);
        PGNString = GetPGN();
        PGNText.text = PGNString;
    }

    public void RecordPromotion(Piece promotedPiece){
        ChessMove lastMove = moveHistory[moveHistory.Count - 1];
        lastMove.promotedPiece = promotedPiece;
        PGNString = GetPGN();
        PGNText.text = PGNString;
    }

    public string GetPGN(){
        string result = "";
        int moveCount = 0;
        for(int i = 0; i < moveHistory.Count; i++){
            if(i % 2 == 0){
                moveCount += 1;
                result += moveCount + ". ";
            }
            
            ChessMove move = moveHistory[i];
            result += move.GetAlgebraicNotation() + " ";
        }
        return result;
    }

}

[System.Serializable]
public class ChessMove{
    public Vector2Int origin;
    public Vector2Int destination;
    public moveType MT;
    public Piece movingPiece;
    public bool capturedPiece;
    public Piece promotedPiece;
    public ChessPlayer team;
    public bool causedCheck;
    public bool causedCheckmate;

    public ChessMove(Vector2Int origin, Vector2Int destination, moveType MT, bool capturedPiece, Piece movingPiece, ChessPlayer team, bool causedCheck, bool causedCheckmate){
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
            case "Rook": return "R";
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
    public string getSquarePhonetic(Vector2Int coords){
        string result = "";
        switch (coords.x){
            case 0: result += "eigh"; break;
            case 1: result += "b"; break;
            case 2: result += "s"; break;
            case 3: result += "d"; break;
            case 4: result += "e"; break;
            case 5: result += "e"; break;
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
        string promotionString = "";
        if(promotedPiece != null){
            promotionString += "=" + getPieceAbbr(promotedPiece);
        }
        if(capturedPiece){
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
        return originString + getPieceAbbr(movingPiece) + captureString + getSquare(destination) + promotionString + checkString;
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