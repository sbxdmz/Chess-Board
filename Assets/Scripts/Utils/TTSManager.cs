using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpeechLib;
using UnityEngine.UI;

public class TTSManager : MonoBehaviour
{
    SpVoice voice = new SpVoice();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // public ChessMove(Vector2Int origin, Vector2Int destination, moveType MT, Piece capturedPiece, Piece movingPiece, ChessPlayer team, bool causedCheck, bool causedCheckmate){
    public void AnnounceMove(ChessMove move){
        if(!SettingsManager.main.textToSpeechEnabled) { return; }
        string speechString = "";
        if(move.MT == moveType.longCastle){
            speechString = "Long Castle";
        }
        else if(move.MT == moveType.shortCastle){
            speechString = "Short Castle";
        }
        else{
            speechString = move.movingPiece.GetType().Name;
            speechString += " " + MyUtils.getSquarePhonetic(move.origin);
            if(move.capturedPiece != ""){
                speechString += " takes " + move.capturedPiece + " on ";
            }
            else{
                speechString += " to ";
            } 
            speechString += MyUtils.getSquarePhonetic(move.destination);
        }
        voice.Speak(speechString, SpeechVoiceSpeakFlags.SVSFlagsAsync | SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
        
    }
}
