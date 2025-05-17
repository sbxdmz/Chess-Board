using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;

[System.Serializable]
public class ChessPlayer
{
	public TeamColor team { get; set; }
	public Board board { get; set; }
	public List<Piece> activePieces { get; private set; }

	public ChessPlayer(TeamColor team, Board board)
	{
		activePieces = new List<Piece>();
		this.board = board;
		this.team = team;
	}


	public List<Piece> GetAllTargeting(Vector2Int coords)
	{
		List<Piece> result = new List<Piece>();
		foreach (var piece in activePieces)
		{
			if (board.HasPiece(piece) && piece.CanMoveToSquare(coords)){
				result.Add(piece);
			}
		}
		return result;
	}

	public void AddPiece(Piece piece)
	{
		if (!activePieces.Contains(piece))
			activePieces.Add(piece);
	}

	public void RemovePiece(Piece piece)
	{
		if (activePieces.Contains(piece))
			activePieces.Remove(piece);
	}
	public void ClearPieces()
	{
		activePieces.Clear();
	}

	public void GenerateAllPossibleMoves()
	{
		foreach (var piece in activePieces)
		{
			if(board.HasPiece(piece))
				piece.SelectAvailableSquares();
		}
	}

	public List<Vector2Int> GetAllPossibleMoves(){
		List<Vector2Int> Result = new List<Vector2Int>();
		foreach (var piece in activePieces)
		{
			if(board.HasPiece(piece))
				Result.AddRange(piece.avaliableMoves);
		}

		return Result;
	} 

	public Piece[] GetPieceAttackingOppositePieceOfType<T>() where T : Piece
	{
		return activePieces.Where(p => p.IsAttackingPieceOfType<T>()).ToArray();
	}

	public Piece[] GetPiecesOfType<T>() where T : Piece
	{
		return activePieces.Where(p => p is T).ToArray();
	}

	public bool RemoveMovesEnablingAttackOnPieceOfType<T>(ChessPlayer opponent, Piece selectedPiece) where T : Piece
	{
		List<Vector2Int> coordsToRemove = new List<Vector2Int>();

		coordsToRemove.Clear();
		Vector2Int orgPos = selectedPiece.occupiedSquare;
		foreach (var coords in selectedPiece.avaliableMoves)
		{
			Piece pieceOnCoords = board.GetPieceOnSquare(coords);
			board.UpdateBoardOnPieceMove(coords, selectedPiece.occupiedSquare, selectedPiece, null);
			selectedPiece.occupiedSquare = coords;
            /*if (coords == new Vector2(6, 5))
            {
				Debug.Log(selectedPiece.occupiedSquare);
            }*/
            opponent.GenerateAllPossibleMoves();
			if (opponent.CheckIfIsAttackingPiece<T>())
			{
				coordsToRemove.Add(coords);
			}
			board.UpdateBoardOnPieceMove(orgPos, coords, selectedPiece, pieceOnCoords);
            selectedPiece.occupiedSquare = orgPos;
        }
        selectedPiece.occupiedSquare = orgPos;
        foreach (var coords in coordsToRemove)
        {
            // Debug.Log("removing " + selectedPiece + " " + coords);
            selectedPiece.avaliableMoves.Remove(coords);
		}
		// Debug.Log("Coords to remove" + coordsToRemove.Count);
		if(coordsToRemove.Count > 0){
			return true;
		} 
		return false;

	}

	internal bool CheckIfIsAttackingPiece<T>() where T : Piece
	{
		foreach (var piece in activePieces)
		{
			if (board.HasPiece(piece) && piece.IsAttackingPieceOfType<T>()){
				return true;
			}
		}
		return false;
	}

	public bool CanHidePieceFromAttack<T>(ChessPlayer opponent) where T : Piece
	{
		foreach (var piece in activePieces)
		{
			Vector2Int orgPos = piece.occupiedSquare;
			foreach (var coords in piece.avaliableMoves)
			{
				Piece pieceOnCoords = board.GetPieceOnSquare(coords);
				board.UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece, null);
                piece.occupiedSquare = coords;
                opponent.GenerateAllPossibleMoves();
				if (!opponent.CheckIfIsAttackingPiece<T>())
				{
					board.UpdateBoardOnPieceMove(orgPos, coords, piece, pieceOnCoords);
                    piece.occupiedSquare = orgPos;
                    return true;
				}
				board.UpdateBoardOnPieceMove(orgPos, coords, piece, pieceOnCoords);
                piece.occupiedSquare = orgPos;
            }
		}
		return false;
	}

	internal void OnGameRestarted()
	{
		activePieces.Clear();
	}



}