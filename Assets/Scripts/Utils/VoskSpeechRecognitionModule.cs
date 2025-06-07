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
    
    public void ToggleRecording(){
        VoskSpeechToText.ToggleRecording();
        if(VoskSpeechToText.Running){
            startButton.interactable = false;
            stopButton.interactable = true; 
        }
        else{
            startButton.interactable = true;
            stopButton.interactable = false; 
        }
    }

    void Awake()
    {
        startButton.interactable = true;
        stopButton.interactable = false; 
        VoskSpeechToText.OnTranscriptionResult += OnTranscriptionResult;
        startButton.onClick.AddListener(ToggleRecording);
        stopButton.onClick.AddListener(ToggleRecording);
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
        Debug.Log(text);
        var regex = new Regex(@".*(^|\s)(([a-zA-Z]) (one|two|three|four|five|six|seven|eight)).*(^|\s)(([a-zA-Z]) (one|two|three|four|five|six|seven|eight)).*");
        var match = regex.Match(text);
        if(match.Success)
        {
            string firstLetter = match.Groups[3].Value;
            string firstNumber = MyUtils.GetNumberFromWord(match.Groups[4].Value);
            string secondLetter = match.Groups[7].Value;
            string secondNumber = MyUtils.GetNumberFromWord(match.Groups[8].Value);
            string final = firstLetter + firstNumber + secondLetter + secondNumber;
            final = final.ToLower();
            moveText.text = "Making move: " + firstLetter + firstNumber + " to " + secondLetter + secondNumber;
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