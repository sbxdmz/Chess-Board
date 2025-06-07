using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ChessHistoryManager : MonoBehaviour
{   
    public TextMeshProUGUI PGNText;
    public List<ChessMove> moveHistory = new List<ChessMove>();
    public TTSManager tts;
    private string PGNString;
    private string FENString;
    [SerializeField] private TMP_InputField FENInput;
    public ChessGameController gameController;
    public GameObject undoButton;
    private bool canUndo = true;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z)){
            UndoMove();
        }
    }

    public void SetUndo(bool enabled){
        undoButton.SetActive(enabled);
        canUndo = enabled;
    }
    public void displayFEN(Piece[,] board, ChessPlayer nextPlayer)
    {
        FENString = GetFEN(board, nextPlayer);
        FENInput.text = FENString;
    }

    public void RecordMove(Vector2Int origin, Vector2Int destination, moveType MT, string capturedPiece, Piece movingPiece, ChessPlayer team, bool causedCheck, bool causedCheckmate, Piece[,] board, ChessPlayer nextPlayer, bool ambiguousRow, bool ambiguousCol){
        ChessMove newMove = new ChessMove(origin, destination, MT, capturedPiece, movingPiece, team, causedCheck, causedCheckmate, GetFEN(board, nextPlayer), ambiguousRow, ambiguousCol);
        moveHistory.Add(newMove);
        tts.AnnounceMove(newMove);
        PGNString = GetPGN();
        PGNText.text = PGNString;
        FENString = GetFEN(board, nextPlayer);
        FENInput.text = FENString;

    }
    public void RecordPromotion(Piece promotedPiece){
        ChessMove lastMove = moveHistory[moveHistory.Count - 1];
        lastMove.promotedPiece = promotedPiece;
        PGNString = GetPGN();
        PGNText.text = PGNString;
    }

    public void UndoMove(){
        Debug.Log(moveHistory.Count);
        if(canUndo == false){
            return;
        }
        if(moveHistory.Count <= 1)
        {
            return;
        }
        ChessMove previousMove = moveHistory[moveHistory.Count - 2];
        gameController.StartFromFEN(previousMove.FEN);
        moveHistory.Remove(moveHistory[moveHistory.Count-1]);
        PGNString = GetPGN();
        PGNText.text = PGNString;
        FENString = previousMove.FEN;
        FENInput.text = FENString;
    }

    public string GetPGN(){
        string result = "";
        int moveCount = 0;
        int startingValue = 1;
        if(moveHistory[0].team.team == TeamColor.Black){
            startingValue = 0;
            moveCount += 1;
        }
        for(int i = 1; i < moveHistory.Count; i++){
            if(i % 2 == startingValue){
                moveCount += 1;
                result += moveCount + ". ";
            }
            
            ChessMove move = moveHistory[i];
            result += move.GetAlgebraicNotation() + " ";
        }
        return result;
    }
    
    public void ResetPGN(bool isBlack){
        PGNString = "";
        PGNText.text = "";
        moveHistory.Clear();
        if(isBlack){
        //     moveCount = 1;
        // }
        // else{
        //     moveHistory.count = 0;
        }
    }
    public string GetFEN(Piece[,] board, ChessPlayer toMove){
        string result = "";
        King whiteKing = null;
        King blackKing = null;
        int counter = 0;
        string enPassantString = "-";
        for(int r = 7; r >= 0; r--){
            for(int c = 0; c < 8; c++){
                if(board[c,r] == null){
                    counter++;
                    continue;
                }
                else if(board[c,r].occupiedSquare != new Vector2Int(c,r)){
                    enPassantString = MyUtils.getSquare(new Vector2Int(c,r));
                    counter++;
                    continue;
                }
                if(counter>0){
                    result += counter;
                    counter = 0;
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
            if(counter>0){
                    result += counter;
                    counter = 0;
            }
            if(r > 0){
                result += "/";
            }
        }
        int fullMove = gameController.fullMoveClock;
        int halfMove = gameController.halfMoveClock;
        result += (toMove.team == TeamColor.White?" w ":" b ") + whiteKing.GetCastlingRights() + blackKing.GetCastlingRights() + " " + enPassantString + " " + halfMove + " " + fullMove;
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
    public string FEN;
    public bool ambiguousRow;
    public bool ambiguousCol;

    public ChessMove(Vector2Int origin, Vector2Int destination, moveType MT, string capturedPiece, Piece movingPiece, ChessPlayer team, bool causedCheck, bool causedCheckmate, string FEN, bool ambiguousRow, bool ambiguousCol){
        this.origin = origin;
        this.destination = destination;
        this.MT = MT;
        this.capturedPiece = capturedPiece;
        this.movingPiece = movingPiece;
        this.team = team;
        this.causedCheck = causedCheck;
        this.causedCheckmate = causedCheckmate;
        this.FEN = FEN;
        this.ambiguousRow = ambiguousRow;
        this.ambiguousCol = ambiguousCol;
    }

    public ChessMove(string FEN, ChessPlayer team){
        this.FEN = FEN;
        this.team = team;
    }
    public string GetAlgebraicNotation(){
        string captureString = "";
        string checkString = "";
        string originString = "";
        string originSquare = MyUtils.getSquare(origin);
        string promotionString = "";
        string disambiguationString = "";

        if(promotedPiece != null){
            promotionString += "=" + MyUtils.getPieceAbbr(promotedPiece);
        }
        if(capturedPiece != ""){
            if(movingPiece.GetType().Name == "Pawn"){
                originString = originSquare.Substring(0,1);
            }
            captureString = "x";

        }
        if(ambiguousCol){
            string o = MyUtils.getSquare(origin);
            disambiguationString += o.Substring(0,1);
        }
        if(ambiguousRow){
            string o = MyUtils.getSquare(origin);
            disambiguationString += o.Substring(1,1);
        }
        if(movingPiece.GetType().Name == "Pawn"){
            disambiguationString = "";
        }
        if(causedCheckmate){
            checkString = "#";
        }
        else if(causedCheck){
            checkString = "+";
        }

        if(MT == moveType.normal){
            return originString + MyUtils.getPieceAbbr(movingPiece) + disambiguationString + captureString + MyUtils.getSquare(destination) + promotionString + checkString;
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