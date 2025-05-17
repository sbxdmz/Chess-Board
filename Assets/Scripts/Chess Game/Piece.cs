using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MaterialSetter))]
[RequireComponent(typeof(IObjectTweener))]
public abstract class Piece : MonoBehaviour
{
	[SerializeField] private MaterialSetter materialSetter;
	public Board board { protected get; set; }
	public Vector2Int occupiedSquare { get; set; }
	public TeamColor team { get; set; }
	public Sprite whiteSprite;
	public Sprite blackSprite;
	public bool hasMoved;
	public List<Vector2Int> avaliableMoves;

	private IObjectTweener tweener;

	public abstract List<Vector2Int> SelectAvailableSquares();

	private void Awake()
	{
		avaliableMoves = new List<Vector2Int>();
		tweener = GetComponent<IObjectTweener>();
		materialSetter = GetComponent<MaterialSetter>();
		hasMoved = false;
	}

	public void SetMaterial(Material selectedMaterial)
	{
		materialSetter.SetSingleMaterial(selectedMaterial);
	}

	public void SetSprite(Sprite selectedSprite){
		GetComponentInChildren<SpriteRenderer>().sprite = selectedSprite;
	}
	public bool IsFromSameTeam(Piece piece)
	{
		return team == piece.team;
	}

	public Sprite GetSprite(TeamColor team){
		return team == TeamColor.White ? whiteSprite : blackSprite;
	}

	public bool CanMoveTo(Vector2Int coords)
	{
		return avaliableMoves.Contains(coords);
	}

	public virtual moveType MovePiece(Vector2Int coords)
	{
		Vector3 targetPosition = board.CalculatePositionFromCoords(coords);
		occupiedSquare = coords;
		hasMoved = true;
		tweener.MoveTo(transform, targetPosition);

		return moveType.normal;
	}


	protected void TryToAddMove(Vector2Int coords)
	{
		avaliableMoves.Add(coords);
	}

	public void SetData(Vector2Int coords, TeamColor team, Board board)
	{
		this.team = team;
		occupiedSquare = coords;
		this.board = board;
		transform.position = board.CalculatePositionFromCoords(coords);
	}

	public bool IsAttackingPieceOfType<T>() where T : Piece
	{
		foreach (var square in avaliableMoves)
		{
			if (board.GetPieceOnSquare(square) is T)
				return true;
		}
		return false;
	}

	public bool CanMoveToSquare(Vector2Int coordinates){
		return avaliableMoves.Contains(coordinates);
	}
}
public enum moveType{
	normal, shortCastle, longCastle, promotion
}