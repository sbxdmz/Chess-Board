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
    
    public void RecordMove(Vector2Int origin, Vector2Int destination, moveType MT, string capturedPiece, Piece movingPiece, ChessPlayer team, bool causedCheck, bool causedCheckmate){
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
//
    public string GetFEN(Piece[,] board, ChessPlayer toMove){
        string result = "";
        King whiteKing = null;
        King blackKing = null;
        for(int r = 7; r >= 0; r--){
            for(int c = 0; c < 8; c++){
                if(board[c,r] == null){
                    result += "S";
                    continue;
                }
                else if(board[c,r].occupiedSquare != new Vector2Int(c,r)){
                    result += "E";
                    continue;
                }
                if(board[c,r].GetType() == typeof(King)){
                    if(board[c,r].team == TeamColor.Black){
                        blackKing = (King) board[c,r];
                    }
                    else{
                        whiteKing = (King) board[c,r];
                    }
                }
                string currentPiece = MyUtils.getPieceAbbrFEN(board[c,r]);    
                if(board[c,r].team == TeamColor.Black){
                    currentPiece = currentPiece.ToLower();
                }
                result += currentPiece;
            }
            result += "/";
        }
        result += (toMove.team == TeamColor.White?"w ":"b ") + whiteKing.GetCastlingRights() + blackKing.GetCastlingRights();
        return result;
    }

}

[System.Serializable]
public class ChessMove{
    public Vector2Int origin;
    public Vector2Int destination;
    public moveType MT;
    public Piece movingPiece;
    public string capturedPiece;
    public Piece promotedPiece;
    public ChessPlayer team;
    public bool causedCheck;
    public bool causedCheckmate;

    public ChessMove(Vector2Int origin, Vector2Int destination, moveType MT, string capturedPiece, Piece movingPiece, ChessPlayer team, bool causedCheck, bool causedCheckmate){
        this.origin = origin;
        this.destination = destination;
        this.MT = MT;
        this.capturedPiece = capturedPiece;
        this.movingPiece = movingPiece;
        this.team = team;
        this.causedCheck = causedCheck;
        this.causedCheckmate = causedCheckmate;
    }
    public string GetAlgebraicNotation(){
        string captureString = "";
        string checkString = "";
        string originString = "";
        string originSquare = MyUtils.getSquare(origin);
        string promotionString = "";
        if(promotedPiece != null){
            promotionString += "=" + MyUtils.getPieceAbbr(promotedPiece);
        }
        if(capturedPiece != ""){
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
        return originString + MyUtils.getPieceAbbr(movingPiece) + captureString + MyUtils.getSquare(destination) + promotionString + checkString;
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