using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessHistoryManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void RecordMove(){

    }

    
}

public class ChessMove{
    Vector2Int origin;
    Vector2Int destination;
    bool isCastleMove;
    Piece capturedPiece;
}