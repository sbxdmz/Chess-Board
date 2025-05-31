using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class VoskSpeechRecognitionModule : MonoBehaviour 
{
    [SerializeField] private ExternalInputHandler externalInput;
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI sentenceText;
    [SerializeField] private TextMeshProUGUI moveText;
    [SerializeField] private Board board;
    [SerializeField] private ChessHistoryManager historyManager;
    public VoskSpeechToText VoskSpeechToText;

    void Awake()
    {
        VoskSpeechToText.OnTranscriptionResult += OnTranscriptionResult;
    }

    private void OnTranscriptionResult(string obj)
    {
        string resultString = "";
        var result = new RecognitionResult(obj);
        for (int i = 0; i < 1; i++)
        {
            if (i > 0)
            {
                resultString += ", ";

            }

            resultString += result.Phrases[i].Text;
        }
        ProcessMove(resultString);
    }

    void ProcessMove(string text)
    {
        string numbers = "\b(one|two|three|four|five|six|seven|eight)\b";
        var regex = new Regex(@".*([a-zA-Z] " + numbers + ").*([a-zA-Z] " + numbers + ").*");
        var match = regex.Match(text);
        if(match.Success)
        {
            string first = match.Groups[1].Value;
            string second = match.Groups[2].Value;
            string final = first + second;
            final = final.ToLower();
            moveText.text = "Making move: " + first + " to " + second;
            externalInput.MakeMove(final);
        }
        else
        {
            if(text.ToLower().Contains("queen")){
                moveText.text = "Promoting to a Queen";
                board.ChoosePromotionPiece("Queen"); 
            }
            else if(text.ToLower().Contains("rook")){
                
                moveText.text = "Promoting to a Rook";
                board.ChoosePromotionPiece("Rook"); 
            }
            else if(text.ToLower().Contains("bishop")){
                moveText.text = "Promoting to a Bishop";
                board.ChoosePromotionPiece("Bishop"); 
            }
            else if(text.ToLower().Contains("knight") || text.ToLower().Contains("night") ){
                moveText.text = "Promoting to a Knight";
                board.ChoosePromotionPiece("Knight"); 
            }
            else if(text.ToLower().Contains("undo")){
                moveText.text = "Undoing move";
                historyManager.UndoMove();
            }
            else if(text.ToLower().Contains("cancel")){
                moveText.text = "Cancelling promotion";
                board.ChoosePromotionPiece("Cancel");
            }
            else{
            moveText.text = "Was unable to parse command. Please try again.";
            }
        }
    }
}