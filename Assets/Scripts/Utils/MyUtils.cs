using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public static class MyUtils{
    public static string getPieceAbbr(Piece piece){
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
    public static string getPieceAbbrFEN(Piece piece){
        switch (piece.GetType().Name){
            case "Pawn": return "P"; 
            case "Knight": return "N";
            case "King": return "K";
            case "Bishop": return "B";
            case "Queen": return "Q"; 
            case "Rook": return "R";
        }
        return null;
    }
    public static string getSquare(Vector2Int coords){
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
    public static string getSquarePhonetic(Vector2Int coords){
        string result = "";
        switch (coords.x){
            case 0: result += "eigh"; break;
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
}