using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [SerializeField] private TMP_InputField FENInput;

    private PiecesCreator pieceCreator;
    public ChessPlayer whitePlayer;
    public ChessPlayer blackPlayer;
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

    private void Update(){
        
    }
    private void StartNewGame()
    {
        UIManager.HideUI();
        board.SetDependencies(this, historyManager);
        SetGameState(GameState.Init);
        StartFromFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
    }
    public void SubmitFEN()
    {
        StartFromFEN(FENInput.text);
    }
    public void StartFromFEN(string FEN)
    {
        //rnbqkbnr/ppp1pppp/8/8/2Pp4/3PP3/PP3PPP/RNBQKBNR b KQkq c3 0 1
        board.ResetBoard();
        //CreatePiecesFromLayout(startingBoardLayout);
        activePlayer = whitePlayer;
        CreatePiecesFromFEN(FEN);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));
        GenerateAllPossiblePlayerMoves(activePlayer);
        SetGameState(GameState.Play);
        historyManager.displayFEN(board.grid, activePlayer);
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

    enum FENPhase
    {
        BoardLayout,
        Team,
        CastlingRights,
        EnPassant,
        HalfmoveClock,
        FullmoveNumber
    }
    private void CreatePiecesFromFEN(string FEN)
    {
        TeamColor nextToPlay = (TeamColor)(-1);
        
        King whiteKing = null;
        King blackKing = null;

        int file = 0;
        int rank = 7;
        FENPhase currentPhase = FENPhase.BoardLayout;
        for (int characterIndex = 0; characterIndex < FEN.Length; characterIndex++) 
        {
            char c = FEN[characterIndex];
            if(c == ' ')
            {
                if(currentPhase == FENPhase.BoardLayout)
                {
                    whiteKing.canCastleLeft = false;
                    whiteKing.canCastleRight = false;
                    blackKing.canCastleLeft = false;
                    blackKing.canCastleRight = false;
                }
                currentPhase++;
                continue;
            }

            if (currentPhase == FENPhase.BoardLayout)
            {
                if (c == '/')
                {
                    rank--;
                    file = 0;
                    continue;
                }

                if (char.IsNumber(c))
                {
                    int numberOfSpaces = (int)char.GetNumericValue(c);
                    file += numberOfSpaces;
                    continue;
                }

                Type newPieceType = MyUtils.GetPieceTypeFromAbbreviation(c);
                TeamColor newTeam = MyUtils.GetTeamColorFromAbbreviation(c);
                Vector2Int squareCoords = new Vector2Int(file, rank);
                Piece createdPiece = CreatePieceAndInitialize(squareCoords, newTeam, newPieceType);
                
                if(newPieceType == typeof(King))
                {
                    if(newTeam == TeamColor.White)
                    {
                        whiteKing = (King)createdPiece;
                    }
                    else if (newTeam == TeamColor.Black)
                    {
                        blackKing = (King)createdPiece;
                    }
                }
                if(newPieceType == typeof(Pawn)){
                    Debug.Log(rank);
                    if((newTeam == TeamColor.White && rank != 1) || (newTeam == TeamColor.Black && rank != 6)){
                        createdPiece.hasMoved = true;
                    }
                }
                file++;
            }
            else if (currentPhase == FENPhase.Team)
            {
                if (c == 'w')
                {
                    activePlayer = whitePlayer;
                }
                else if (c == 'b')
                {
                    activePlayer = blackPlayer;
                }
            }
            else if (currentPhase == FENPhase.CastlingRights)
            {
                switch (c)
                {
                    case 'K':
                        whiteKing.canCastleRight = true;
                        break;

                    case 'Q':
                        whiteKing.canCastleLeft = true;
                        break;

                    case 'k':
                        blackKing.canCastleRight = true;
                        break;

                    case 'q':
                        blackKing.canCastleLeft = true;
                        break;
                }
            }
            else if (currentPhase == FENPhase.EnPassant)
            {
                if(char.IsLetter(c) && char.IsNumber(FEN[characterIndex + 1]))
                {
                    string squareString = (c.ToString() + FEN[characterIndex + 1].ToString());
                    Vector2Int squareCoords = MyUtils.getCoords(squareString);
                    Piece createdPiece = CreateEnPassantPiece(squareCoords);
                }
            }
            else if (currentPhase == FENPhase.HalfmoveClock)
            {

            }
            else if (currentPhase == FENPhase.FullmoveNumber)
            {

            }


        }
    }

    public Piece CreatePieceAndInitialize(Vector2Int squareCoords, TeamColor team, Type type)
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

        return newPiece;
    }
    public Piece CreateEnPassantPiece(Vector2Int squareCoords)
    {
        Vector2Int squareAbove = squareCoords + Vector2Int.up;
        Vector2Int squareBelow = squareCoords + Vector2Int.down;
        Piece targetPawn = null;
        Piece pieceAbove = board.GetPieceOnSquare(squareAbove);
        Piece pieceBelow = board.GetPieceOnSquare(squareBelow);
        if (pieceAbove != null && pieceAbove.GetType() == typeof(Pawn))
        {
            targetPawn = pieceAbove;
        }
        else if (pieceBelow != null && pieceBelow.GetType() == typeof(Pawn))
        {
            targetPawn = pieceBelow;
        }
        board.SetPieceOnBoard(squareCoords, targetPawn);
        return targetPawn;
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
        EndTurn(true);
    }

    public void EndTurn(bool changeTeam)
    {
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));
        board.ClearBoardOfPassant(GetOpponentToPlayer(activePlayer));
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));
        ChessGameState currentState = CheckIfGameIsFinished(); 
        if (currentState == ChessGameState.GameWon)
        {
            EndGame();
        }
        else if(currentState == ChessGameState.GameStalemate){
            StalemateGame();
        }
        else if(changeTeam && currentState == ChessGameState.GameCheck){
            ChangeActiveTeam(); //check back
        }
        else if(changeTeam)
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

    public void ChangeActiveTeam()
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

    public ChessPlayer GetOppositeOfActive(){
        return GetOpponentToPlayer(activePlayer);
    }
}

public enum ChessGameState{GameRunning, GameWon, GameStalemate, GameCheck}