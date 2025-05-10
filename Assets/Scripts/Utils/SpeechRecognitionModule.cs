using System.IO;
using System.Text.RegularExpressions;
using HuggingFace.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeechRecognitionModule : MonoBehaviour
{
    [SerializeField] private ExternalInputHandler externalInput;
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI sentenceText;
    [SerializeField] private TextMeshProUGUI moveText;
    [SerializeField] private Board board;
    [SerializeField] private ChessHistoryManager historyManager;
    private AudioClip clip;
    private byte[] bytes;
    private bool recording;

    private void Start()
    {
        startButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
        stopButton.interactable = false;
    }

    private void Update()
    {
        if (recording && Microphone.GetPosition(null) >= clip.samples)
        {
            StopRecording();
        }

        if(Input.GetKeyDown(KeyCode.Space)){
            StartRecording();
        }
        if(Input.GetKeyUp(KeyCode.Space)){
            StopRecording();
        }
    }

    private void StartRecording()
    {
        sentenceText.color = Color.white;
        sentenceText.text = "Recording...";
        startButton.interactable = false;
        stopButton.interactable = true;
        clip = Microphone.Start(null, false, 10, 44100);
        recording = true;
    }

    private void StopRecording()
    {
        var position = Microphone.GetPosition(null);
        Microphone.End(null);
        var samples = new float[position * clip.channels];
        clip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
        recording = false;
        SendRecording();
    }

    private void SendRecording()
    {
        sentenceText.color = Color.yellow;
        sentenceText.text = "Sending...";
        stopButton.interactable = false;
        HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response => {
            sentenceText.color = Color.white;
            sentenceText.text = response;
            ProcessMove(response);
            startButton.interactable = true;
        }, error => {
            sentenceText.color = Color.red;
            //sentenceText.text = error;
            sentenceText.text = "Connection error. Please try again.";
            startButton.interactable = true;
        });
    }
    void ProcessMove(string text)
    {
        var regex = new Regex(@".*([a-zA-Z]\d).*([a-zA-Z]\d).*");
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

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }
}
