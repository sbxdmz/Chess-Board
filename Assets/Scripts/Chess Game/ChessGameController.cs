using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PiecesCreator))]
public class ChessGameController : MonoBehaviour
{
    private enum GameState
    {
        Init, Play, Finished
    }

    [SerializeField] private BoardLayout startingBoardLayout;
    [SerializeField] private Board board;
    [SerializeField] private ChessUIManager UIManager;
    [SerializeField] private ChessHistoryManager historyManager;

    private PiecesCreator pieceCreator;
    private ChessPlayer whitePlayer;
    private ChessPlayer blackPlayer;
    public ChessPlayer activePlayer;

    private GameState state;
    

    public List<Vector2Int> test1;
    public List<Vector2Int> test2;
    private void Awake()
    {
        SetDependencies();
        CreatePlayers();
    }

    private void SetDependencies()
    {
        pieceCreator = GetComponent<PiecesCreator>();
    }

    private void CreatePlayers()
    {
        whitePlayer = new ChessPlayer(TeamColor.White, board);
        blackPlayer = new ChessPlayer(TeamColor.Black, board);
    }

    private void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        SetGameState(GameState.Init);
        UIManager.HideUI();
        board.SetDependencies(this, historyManager);
        CreatePiecesFromLayout(startingBoardLayout);
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer);
        SetGameState(GameState.Play);
    }
    private void SetGameState(GameState state)
    {
        this.state = state;
    }

    internal bool IsGameInProgress()
    {
        return state == GameState.Play;
    }



    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        for (int i = 0; i < layout.GetPiecesCount(); i++)
        {
            Vector2Int squareCoords = layout.GetSquareCoordsAtIndex(i);
            TeamColor team = layout.GetSquareTeamColorAtIndex(i);
            string typeName = layout.GetSquarePieceNameAtIndex(i);

            Type type = Type.GetType(typeName);
            CreatePieceAndInitialize(squareCoords, team, type);
        }
    }



    public void CreatePieceAndInitialize(Vector2Int squareCoords, TeamColor team, Type type)
    {
        Piece newPiece = pieceCreator.CreatePiece(type).GetComponent<Piece>();
        newPiece.SetData(squareCoords, team, board);

        Material teamMaterial = pieceCreator.GetTeamMaterial(team);
        newPiece.SetMaterial(teamMaterial);

        Sprite teamSprite = newPiece.GetSprite(team);
        newPiece.SetSprite(teamSprite);
        board.SetPieceOnBoard(squareCoords, newPiece);

        ChessPlayer currentPlayer = team == TeamColor.White ? whitePlayer : blackPlayer;
        currentPlayer.AddPiece(newPiece);
    }

    private void GenerateAllPossiblePlayerMoves(ChessPlayer player)
    {
        player.GenerateAllPossibleMoves();
    }

    public bool IsTeamTurnActive(TeamColor team)
    {
        return activePlayer.team == team;
    }

    public void EndTurn()
    {
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));
        board.ClearBoardOfPassant(GetOpponentToPlayer(activePlayer));
        ChessGameState currentState = CheckIfGameIsFinished(); 
        if (currentState == ChessGameState.GameWon)
        {
            EndGame();
        }
        else if(currentState == ChessGameState.GameStalemate){
            StalemateGame();
        }
        else if(currentState == ChessGameState.GameCheck){
            ChangeActiveTeam(); //check back
        }
        else
        {
            ChangeActiveTeam();
        }
    }
    public bool getCheckStatus(ChessPlayer player){
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));
        ChessPlayer oppositePlayer = GetOpponentToPlayer(player);
        bool isInCheck = oppositePlayer.CheckIfIsAttackingPiece<King>();
        return isInCheck;
    }
    public bool getActiveCheckStatus(){
        return getCheckStatus(activePlayer);
    }
    public bool GetOpponentCheckStatus(){
        return getCheckStatus(GetOpponentToPlayer(activePlayer));
    }
    public ChessGameState CheckIfGameIsFinished()
    {
        bool isInCheck = false;
        ChessPlayer oppositePlayer = GetOpponentToPlayer(activePlayer);
        foreach (var piece in oppositePlayer.activePieces){
            bool foundAttack = oppositePlayer.RemoveMovesEnablingAttackOnPieceOfType<King>(activePlayer, piece);
            if(foundAttack == true){
                isInCheck = true;
            }
        }
        
        Piece[] kingAttackingPieces = activePlayer.GetPieceAttackingOppositePieceOfType<King>();
        
            
        Piece attackedKing = oppositePlayer.GetPiecesOfType<King>().FirstOrDefault();
        //oppositePlayer.RemoveMovesEnablingAttackOnPieceOfType<King>(activePlayer, attackedKing);
        test1 = new List<Vector2Int>();
        test2 = new List<Vector2Int>();
        if (oppositePlayer.team == TeamColor.White){
            test1 = oppositePlayer.GetAllPossibleMoves();
        }
        else{
            test2 = oppositePlayer.GetAllPossibleMoves();
        }
        
        int availableKingMoves = attackedKing.avaliableMoves.Count;
        if (availableKingMoves == 0)
        {
            bool canCoverKing = oppositePlayer.CanHidePieceFromAttack<King>(activePlayer);
            if (!canCoverKing){
                if (kingAttackingPieces.Length == 0){
                   return ChessGameState.GameStalemate;
                }
                return ChessGameState.GameWon;
            }

        }
        if(isInCheck){
            return ChessGameState.GameCheck;
        }
        return ChessGameState.GameRunning;
    }

    private void EndGame()
    {
        SetGameState(GameState.Finished);
        UIManager.OnGameFinished(activePlayer.team.ToString());
    }

    private void StalemateGame(){
        SetGameState(GameState.Finished);
        UIManager.OnGameStalemate();
    }
    public void RestartGame()
    {
        DestroyPieces();
        board.OnGameRestarted();
        whitePlayer.OnGameRestarted();
        blackPlayer.OnGameRestarted();
        StartNewGame();
    }

    private void DestroyPieces()
    {
        whitePlayer.activePieces.ForEach(p => Destroy(p.gameObject));
        blackPlayer.activePieces.ForEach(p => Destroy(p.gameObject));
    }

    private void ChangeActiveTeam()
    {
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;
        
    }

    private ChessPlayer GetOpponentToPlayer(ChessPlayer player)
    {
        return player == whitePlayer ? blackPlayer : whitePlayer;
    }

    internal void OnPieceRemoved(Piece piece)
    {
        ChessPlayer pieceOwner = (piece.team == TeamColor.White) ? whitePlayer : blackPlayer;
        pieceOwner.RemovePiece(piece);
    }

    internal void RemoveMovesEnablingAttackOnPieceOfType<T>(Piece piece) where T : Piece
    {
        activePlayer.RemoveMovesEnablingAttackOnPieceOfType<T>(GetOpponentToPlayer(activePlayer), piece);
    }
}

public enum ChessGameState{GameRunning, GameWon, GameStalemate, GameCheck}