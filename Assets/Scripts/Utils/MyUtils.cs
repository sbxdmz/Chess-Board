using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Analytics;
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
    public static Vector2Int getCoords(string squareString)
    {
        char firstChar = squareString[0];
        char secondChar = squareString[1];

        int x = 0;
        switch (firstChar)
        {
            case 'a': x = 0; break;
            case 'b': x = 1; break;
            case 'c': x = 2; break;
            case 'd': x = 3; break;
            case 'e': x = 4; break;
            case 'f': x = 5; break;
            case 'g': x = 6; break;
            case 'h': x = 7; break;
        }

        int y = (int)Char.GetNumericValue(secondChar) - 1;

        Vector2Int result = new Vector2Int(x, y);
        // Debug.Log(result);
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
    public static Type GetPieceTypeFromAbbreviation(char piece)
    {

        Type pieceType = typeof(Piece);

        switch (char.ToUpper(piece))
        {
            case 'P': pieceType = typeof(Pawn); break;
            case 'N': pieceType = typeof(Knight); break;
            case 'K': pieceType = typeof(King); break;
            case 'B': pieceType = typeof(Bishop); break;
            case 'Q': pieceType = typeof(Queen); break;
            case 'R': pieceType = typeof(Rook); break;
        }
        return pieceType;
    }

    public static string GetNumberFromWord(string word){
        switch(word.ToLower()){
            case "one": return "1";
            case "two": return "2";
            case "three": return "3";
            case "four": return "4";
            case "five": return "5";
            case "six": return "6";
            case "seven": return "7";
            case "eight": return "8"; 
        }
        return "";
    }
    public static TeamColor GetTeamColorFromAbbreviation(char piece)
    {
        return char.IsUpper(piece) ? TeamColor.White : TeamColor.Black;
    }
}