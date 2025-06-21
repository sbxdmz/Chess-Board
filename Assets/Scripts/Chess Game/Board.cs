using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using static UnityEngine.Networking.UnityWebRequest;

[RequireComponent(typeof(SquareSelectorCreator))]
public class Board : MonoBehaviour
{

    public const int BOARD_SIZE = 8;

    [SerializeField] private Transform bottomLeftSquareTransform;
    [SerializeField] private float squareSize;
    [SerializeField] private GameObject promotionScreen;
    [SerializeField] private GameObject buttonParent;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip captureClip;

    public Piece[,] grid;
    public Piece[] tempPieces;
    private Piece selectedPiece;
    private ChessGameController chessController;
    private SquareSelectorCreator squareSelector;
    private ChessHistoryManager historyManager;

    private void Awake()
    {
        squareSelector = GetComponent<SquareSelectorCreator>();
        CreateGrid();
    }
    private void Start(){
        tempPieces = new Piece[64];
    }
    private void Update(){
        if(Input.GetKeyDown(KeyCode.Space)){
            Debug.Log(historyManager.GetFEN(grid, chessController.activePlayer));
        }
        if(Input.GetKeyDown(KeyCode.M)){
            string result = "";
            for(int r = 7; r >= 0; r--){
                for(int c = 0; c < 8; c++){
                    result += grid[c , r] + " " + c + " " + r;
                }
            }
            Debug.Log(result);

        }
        int counter = 0;
        for (int r = 7; r >= 0; r--)
        {
            for (int c = 0; c < 8; c++)
            {
                tempPieces[counter] = grid[c, r];
                counter++;
            }
        }
    }
    public void SetDependencies(ChessGameController chessController, ChessHistoryManager historyManager)
    {
        this.chessController = chessController;
        this.historyManager = historyManager;
    }



    private void CreateGrid()
    {
        grid = new Piece[BOARD_SIZE, BOARD_SIZE];
    }
    public void ResetBoard()
    {
        chessController.whitePlayer.ClearPieces();
        chessController.blackPlayer.ClearPieces();
        foreach(Piece p in grid)
        {
            if(p != null)
            {
                Destroy(p.gameObject);
            }
        }
        grid = new Piece[BOARD_SIZE, BOARD_SIZE];
    }

    public Vector3 CalculatePositionFromCoords(Vector2Int coords)
    {
        return bottomLeftSquareTransform.position + new Vector3(coords.x * squareSize, 0f, coords.y * squareSize);
    }

    private Vector2Int CalculateCoordsFromPosition(Vector3 inputPosition)
    {
        int x = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).x / squareSize) + BOARD_SIZE / 2;
        int y = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).z / squareSize) + BOARD_SIZE / 2;
        return new Vector2Int(x, y);
    }

    public selectedStatus OnSquareSelected(Vector3 inputPosition)
    {
        Vector2Int coords = CalculateCoordsFromPosition(inputPosition);
        return OnSquareSelected(coords);
    }

    public selectedStatus OnSquareSelected(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        if (selectedPiece)
        {
            if (piece != null && selectedPiece == piece){
                DeselectPiece();
                return selectedStatus.deselect;
            }
            else if (piece != null && selectedPiece != piece && chessController.IsTeamTurnActive(piece.team)){
                SelectPiece(piece);
                return selectedStatus.select;
            }
            else if (selectedPiece.CanMoveTo(coords)){
                OnSelectedPieceMoved(coords, selectedPiece);
                return selectedStatus.move;
            }
        }
        else
        {
            if (piece != null && chessController.IsTeamTurnActive(piece.team)){
                SelectPiece(piece);
                return selectedStatus.select;
            }
        }
        return selectedStatus.invalid;
    }

    public void DeselectAll(){
        DeselectPiece();
    }


    private void SelectPiece(Piece piece)
    {
        //chessController.RemoveMovesEnablingAttackOnPieceOfType<King>(piece);
        selectedPiece = piece;
        List<Vector2Int> selection = selectedPiece.avaliableMoves;
        ShowSelectionSquares(selection);
    }

    private void ShowSelectionSquares(List<Vector2Int> selection)
    {
        Dictionary<Vector3, bool> squaresData = new Dictionary<Vector3, bool>();
        for (int i = 0; i < selection.Count; i++)
        {
            Vector3 position = CalculatePositionFromCoords(selection[i]);
            Piece piece = GetPieceOnSquare(selection[i]);
            bool isSquareFree = true;
            if (piece && !selectedPiece.IsFromSameTeam(piece))
            {
                if(piece.occupiedSquare == selection[i]){
                    isSquareFree = false;
                }
                else if(selectedPiece.GetType() == typeof(Pawn)){
                    isSquareFree = false;
                }
            }
            
            squaresData.Add(position, isSquareFree);
        }
        squareSelector.ShowSelection(squaresData);
    }

    private void DeselectPiece()
    {
        selectedPiece = null;
        squareSelector.ClearSelection();
    }
    private void OnSelectedPieceMoved(Vector2Int coords, Piece piece)
    {   
        if(chessController.activePlayer.team == TeamColor.Black){
            chessController.fullMoveClock += 1;
        }
        chessController.halfMoveClock += 1;
        bool ambiguousCol = false;
        bool ambiguousRow = false;
        List<Piece> piecesAttackingSameSquare = chessController.activePlayer.GetAllTargeting(coords);
        foreach(Piece thisPiece in piecesAttackingSameSquare){
            if(thisPiece == piece){
                continue;
            }
            if(thisPiece.GetType().Equals(piece.GetType())){
                if(thisPiece.occupiedSquare.x == piece.occupiedSquare.x){
                    ambiguousRow = true;
                }
                if(thisPiece.occupiedSquare.y == piece.occupiedSquare.y){
                    ambiguousCol = true;
                }
            }
        }
        Vector2Int origin = piece.occupiedSquare;
        Piece takenPiece = TryToTakeOppositePiece(coords);
        if(takenPiece != null || piece.GetType() == typeof(Pawn)){
            chessController.halfMoveClock = 0;
        }
        Piece movingPiece = piece;
        UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece, null);
        moveType MT = selectedPiece.MovePiece(coords);
        bool isInCheck = chessController.GetOpponentCheckStatus();
        bool isInCheckmate = chessController.CheckIfGameIsFinished() == ChessGameState.GameWon;
        string capturedPieceString = "";
        if (takenPiece != null){
            capturedPieceString = takenPiece.GetType().Name;
            audioSource.clip = captureClip;
        }
        else{
            audioSource.clip = moveClip;
        }
        audioSource.Play();
        historyManager.RecordMove(origin, coords, MT, capturedPieceString, movingPiece, chessController.activePlayer, isInCheck, isInCheckmate, grid, chessController.GetOppositeOfActive(), ambiguousRow, ambiguousCol);
        DeselectPiece();
        EndTurn();
    }

    private void EndTurn()
    {
        chessController.EndTurn();
    }

    public void UpdateBoardOnPieceMove(Vector2Int newCoords, Vector2Int oldCoords, Piece newPiece, Piece oldPiece)
    {
        grid[oldCoords.x, oldCoords.y] = oldPiece;
        grid[newCoords.x, newCoords.y] = newPiece;
    }

    public void ClearBoardOfPassant(ChessPlayer player)
    {
        for (int row = 0; row < grid.GetLength(0); row++)
        {
            for (int col = 0; col < grid.GetLength(1); col++)
            {
                if (grid[row, col] == null) { continue; }
                if (grid[row, col].team != player.team) { continue; }

                Vector2Int pos = new Vector2Int(row, col);

                if (pos == grid[row, col].occupiedSquare) { continue; }

                UpdateBoardOnPieceMove(pos, pos, null, null);
            }
        }
    }

    public Piece GetPieceOnSquare(Vector2Int coords)
    {
        if (CheckIfCoordinatesAreOnBoard(coords))
            return grid[coords.x, coords.y];
        return null;
    }

    public bool CheckIfCoordinatesAreOnBoard(Vector2Int coords)
    {
        if (coords.x < 0 || coords.y < 0 || coords.x >= BOARD_SIZE || coords.y >= BOARD_SIZE)
            return false;
        return true;
    }

    public bool HasPiece(Piece piece)
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (grid[i, j] == piece)
                    return true;
            }
        }
        return false;
    }

    public void SetPieceOnBoard(Vector2Int coords, Piece piece)
    {
        if (CheckIfCoordinatesAreOnBoard(coords))
            grid[coords.x, coords.y] = piece;
    }

    private Piece TryToTakeOppositePiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        if (piece && !selectedPiece.IsFromSameTeam(piece))
        {
            if(piece.occupiedSquare == coords){
                TakePiece(piece);
                return piece;
            }
            else if(selectedPiece.GetType() == typeof(Pawn)){
                TakePiece(piece);
                return piece;
            }
        }
        return null;
    }

    private void TakePiece(Piece piece)
    {
        if (piece)
        {
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
            chessController.OnPieceRemoved(piece);
            Destroy(piece.gameObject);
        }
    }

    private string chosenPiece = "";
    public void PromotePiece(Piece piece)
    {   
        historyManager.SetUndo(false);
        promotionScreen.SetActive(true);
        buttonParent.transform.position = CalculatePositionFromCoords(piece.occupiedSquare);
        buttonParent.GetComponent<ButtonTeam>().SwitchToTeam(piece.team);
        StartCoroutine(waitForButtonSelect());
        IEnumerator waitForButtonSelect(){
            chosenPiece = "";
            yield return new WaitUntil(()=>chosenPiece != "");
            historyManager.SetUndo(true);
            TakePiece(piece);
            Piece newPiece = null;
            switch(chosenPiece){
                case "Queen": newPiece = chessController.CreatePieceAndInitialize(piece.occupiedSquare, piece.team, typeof(Queen)); break;
                case "Knight": newPiece = chessController.CreatePieceAndInitialize(piece.occupiedSquare, piece.team, typeof(Knight)); break;
                case "Rook": newPiece = chessController.CreatePieceAndInitialize(piece.occupiedSquare, piece.team, typeof(Rook)); break;
                case "Bishop": newPiece = chessController.CreatePieceAndInitialize(piece.occupiedSquare, piece.team, typeof(Bishop)); break;
                default: historyManager.UndoMove(); break;
           }   
            chosenPiece = "";
            promotionScreen.SetActive(false);
            if(newPiece != null){

                historyManager.RecordPromotion(newPiece);
                chessController.ChangeActiveTeam();
                chessController.EndTurn();
            }

        } 
        
        
    }

    public void ChoosePromotionPiece(String pieceType){
        chosenPiece = pieceType;
    }

    internal void OnGameRestarted()
    {
        selectedPiece = null;
        CreateGrid();
    }

}

public enum selectedStatus{
        deselect, select, move, invalid
    }
