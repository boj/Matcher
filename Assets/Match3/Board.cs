//-----------------------------------------------------------------
//  Copyright 2012 Brian Jones - Uncanny Works
//	All rights reserved
//-----------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PieceInfo {
  public string id;
  public GameObject piecePrefab;
  public float dropChance;
}

public class Board : MonoBehaviour {

  // pieces
  public PieceInfo[] pieces;
  // this double checks the minimum dropChance a piece requires, otherwise an error gets thrown
  // setting this to 0 will throw an error
  public float minChanceThreshold = 5;
  public float maxChanceThreshold = 100;
  
  // board settings
  public int boardSizeX;
  public int boardSizeY;
  public int lineSpacing;
  public bool animateOnStart = false; // animate the pieces to fall into place, otherwise instantiate in place
  public float startPosition = 200; // the point at which piece start falling in regards to their animation axis
  public int minimumTouchLimit = 3;
  public bool toggleOtherTiles = true; // if true, all other non selected tiles are given a different behaviour (faded, size, etc)
  
  private GameObject[,] board;
  private List<GameObject> touchedPieces = new List<GameObject>();
  private GameObject lastTouchedPiece;
  
  // user defined callback
  public delegate void ResultDelegate(string type, int count);
  private ResultDelegate resultDelegate = null;
  
  // the user defined callback with which the results get sent back to
  public void RegisterCallback(ResultDelegate callback) {
    resultDelegate = callback;
  }
  
  void Start() {
    if (pieces.Length == 0) {
      Debug.LogError("Your board has no pieces defined yet.");
    }
    
    if (minChanceThreshold <= 0) {
      Debug.LogError("Minimum chance threshold must be a value greater than 0.");
    }
    
    // verify all pieces meet the minimum chance threshold
    foreach (PieceInfo pieceInfo in pieces) {
      if (pieceInfo.dropChance < minChanceThreshold) {
        Debug.LogError("Minimum chance threshold for piece " + pieceInfo.id + " not met.  Did you set values higher than minChanceThreshold?");
      }
    }
    
    // instantiate the board
    Reset();
  }
  
  private int BoardSizeX() {
    return ((boardSizeX - 1) * lineSpacing);
  }
  
  private int BoardSizeY() {
    return ((boardSizeY - 1) * lineSpacing);
  }
  
  private Vector2 VerticalPosition() {
    return new Vector2(-(BoardSizeX() / 2), 0);
  }
  
  private Vector2 HorizontalPosition() {
    return new Vector2(0, -(BoardSizeY() / 2));
  }
  
  private Vector3 PositionAt(int x, int y) {
    float xLoc = HorizontalPosition().y + (lineSpacing * x) - (lineSpacing * 0.5f);
    float yLoc = VerticalPosition().x + (lineSpacing * y) - (lineSpacing * 0.5f);
    return new Vector3(transform.position.x + xLoc, transform.position.y + yLoc, transform.position.z);
  }
  
  private GameObject GetNewPiecePrefab() {    
    // build a new randomized list to cycle through and check drop rate against
    // based on http://en.wikipedia.org/wiki/Fisher-Yates_shuffle
    PieceInfo[] list = pieces;
    System.Random rng = new System.Random();  
    int n = list.Length;
    while (n > 1) {
      n--;
      int k = rng.Next(n + 1);
      PieceInfo value = list[k];
      list[k] = list[n];
      list[n] = value;
    }

    // set the actual chance value
    float chance = Random.Range(minChanceThreshold, maxChanceThreshold);
    foreach (PieceInfo piece in list) {
      if (chance <= piece.dropChance) {
        return piece.piecePrefab;
      }
    }
    
    // if for some reason things fail, return the first element
    // this should never be reached
    return list[0].piecePrefab;
  }
  
  private GameObject InstantiatePiece(int x, int y) {
    GameObject piece = (GameObject)Instantiate(GetNewPiecePrefab(), PositionAt(x, y), Quaternion.identity);
    piece.GetComponent<Piece>().SetBoardRef(GetComponent<Board>());
    piece.transform.parent = transform;
    return piece;
  }
  
  private GameObject InstantiatePieceWithAnimation(int x, int y, float start) {
    GameObject piece = InstantiatePiece(x, y);
    piece.GetComponent<Piece>().AnimatePieceStart(piece.transform.position.x, piece.transform.position.y, start);
    return piece;
  }
  
  //// <summary>
  //// Resets the board to a randomized state.
  //// </summary>
  public void Reset() {
    // init board
    board = new GameObject[boardSizeX, boardSizeY];
    
    // init pieces
    for (int x = 0; x < boardSizeX; x++) {
      for (int y = 0; y < boardSizeY; y++) {
        if (board[x,y] != null) {
          Destroy(board[x,y]); // remove old pieces
        }
        
        GameObject piece;
        if (animateOnStart) {
          piece = InstantiatePieceWithAnimation(x, y, startPosition);
        } else {
          piece = InstantiatePiece(x, y);
        }
        // assign piece to board
        board[x,y] = piece;
        board[x,y].GetComponent<Piece>().SetCoords(x, y);
      }
    }
  }

  private GameObject LastTouchedPiece() {
    return lastTouchedPiece;
  }

  private bool PieceIsConnected(GameObject piece) {
    foreach (GameObject p in touchedPieces) {
      if (piece == p) {
        return true;
      }
    }
    return false;
  }
  
  private bool PieceIsPreviousConnected(GameObject piece) {
    if (touchedPieces.Count > 1) {
      if (piece == touchedPieces[touchedPieces.Count - 2]) {
        touchedPieces.RemoveAt(touchedPieces.Count - 1);
        lastTouchedPiece.GetComponent<Piece>().SysUnTouchPiece();
        lastTouchedPiece = piece;
        return true;
      }
    }
    return false;
  }
  
  private bool CheckSurroundingPieces(GameObject piece) {
    Piece p = piece.GetComponent<Piece>();
    Vector2 coords = p.GetCoords();
    // upper left
    if ((int)coords.x - 1 >= 0 && (int)coords.y + 1 < board.GetLength(1)) {
      if (board[(int)coords.x - 1, (int)coords.y + 1] == lastTouchedPiece) {
        return true;
      }
    }
    // top
    if ((int)coords.y + 1 < board.GetLength(1)) {
      if (board[(int)coords.x, (int)coords.y + 1] == lastTouchedPiece) {
        return true;
      }
    }
    // upper right
    if ((int)coords.x + 1 < board.GetLength(0) && (int)coords.y + 1 < board.GetLength(1)) {
      if (board[(int)coords.x + 1, (int)coords.y + 1] == lastTouchedPiece) {
        return true;
      }
    }
    // right
    if ((int)coords.x + 1 < board.GetLength(0)) {
      if (board[(int)coords.x + 1, (int)coords.y] == lastTouchedPiece) {
        return true;
      }
    }
    // lower right
    if ((int)coords.x + 1 < board.GetLength(0) && (int)coords.y - 1 >= 0) {
      if (board[(int)coords.x + 1, (int)coords.y - 1] == lastTouchedPiece) {
        return true;
      }
    }
    // bottom
    if ((int)coords.y - 1 >= 0) {
      if (board[(int)coords.x, (int)coords.y - 1] == lastTouchedPiece) {
        return true;
      }
    }
    // lower left
    if ((int)coords.x - 1 >= 0 && (int)coords.y - 1 >= 0) {
      if (board[(int)coords.x - 1, (int)coords.y - 1] == lastTouchedPiece) {
        return true;
      }
    }
    // left
    if ((int)coords.x - 1 >= 0) {
      if (board[(int)coords.x - 1, (int)coords.y] == lastTouchedPiece) {
        return true;
      }
    }
    return false;
  }

  private bool MatchesType(GameObject piece) {
    if (piece.GetComponent<Piece>().pieceType == touchedPieces[0].GetComponent<Piece>().pieceType) {
      return true;
    }
    return false;
  }
  
  private int GetPieceDirection(GameObject piece) {
    Piece p = piece.GetComponent<Piece>();
    Vector2 coords = p.GetCoords();
    // touch lower right
    if ((int)coords.x - 1 >= 0 && (int)coords.y + 1 < board.GetLength(1)) {
      if (board[(int)coords.x - 1, (int)coords.y + 1] == lastTouchedPiece) {
        return 5;
      }
    }
    // touch down
    if ((int)coords.y + 1 < board.GetLength(1)) {
      if (board[(int)coords.x, (int)coords.y + 1] == lastTouchedPiece) {
        return 2;
      }
    }
    // touch lower left
    if ((int)coords.x + 1 < board.GetLength(0) && (int)coords.y + 1 < board.GetLength(1)) {
      if (board[(int)coords.x + 1, (int)coords.y + 1] == lastTouchedPiece) {
        return 4;
      }
    }
    // touch left
    if ((int)coords.x + 1 < board.GetLength(0)) {
      if (board[(int)coords.x + 1, (int)coords.y] == lastTouchedPiece) {
        return 3;
      }
    }
    // touch upper left
    if ((int)coords.x + 1 < board.GetLength(0) && (int)coords.y - 1 >= 0) {
      if (board[(int)coords.x + 1, (int)coords.y - 1] == lastTouchedPiece) {
        return 8;
      }
    }
    // touch up
    if ((int)coords.y - 1 >= 0) {
      if (board[(int)coords.x, (int)coords.y - 1] == lastTouchedPiece) {
        return 7;
      }
    }
    // touch upper right
    if ((int)coords.x - 1 >= 0 && (int)coords.y - 1 >= 0) {
      if (board[(int)coords.x - 1, (int)coords.y - 1] == lastTouchedPiece) {
        return 9;
      }
    }
    // touch right
    if ((int)coords.x - 1 >= 0) {
      if (board[(int)coords.x - 1, (int)coords.y] == lastTouchedPiece) {
        return 6;
      }
    }
    return 0;
  }

  public bool AddTouchedPiece(GameObject piece) {
    if (LastTouchedPiece() == piece || PieceIsPreviousConnected(piece) || PieceIsConnected(piece)) {
      return false;
    }    
    // if the list is empty then it's a new touch round
    // if we're extending the list and this is the same piece type, add it
    if (touchedPieces.Count == 0 || (touchedPieces.Count > 0 && MatchesType(piece) && CheckSurroundingPieces(piece))) {
      // determine which direction the piece was touched
      if (touchedPieces.Count == 0) {
        piece.GetComponent<Piece>().Touched(1); // initial touch
      } else {
        piece.GetComponent<Piece>().Touched(GetPieceDirection(piece));
      }
      // add to touched piece chain
      touchedPieces.Add(piece);
      // set last touched piece to this piece
      lastTouchedPiece = piece;
      MarkExternals();
      return true;
    }
    return false;
  }
  
  private void MarkExternals() {
    if (!toggleOtherTiles) return;
    
    Piece p = lastTouchedPiece.GetComponent<Piece>();
    for (int x = 0; x < boardSizeX; x++) {
      for (int y = 0; y < boardSizeY; y++) {
        Piece piece = board[x,y].GetComponent<Piece>();
        if (piece.pieceType != p.pieceType) {
          piece.SysSetExternalPiece();
        }
      }
    }
  }
  
  private void UnMarkExternals() {
    if (!toggleOtherTiles) return;
    
    Piece p = lastTouchedPiece.GetComponent<Piece>();
    for (int x = 0; x < boardSizeX; x++) {
      for (int y = 0; y < boardSizeY; y++) {
        Piece piece = board[x,y].GetComponent<Piece>();
        if (piece.pieceType != p.pieceType) {
          piece.SysResetExternalPiece();
        }
      }
    }
  }
  
  private void ProcessBoard() {   
    // remove the selected pieces
    for (int x = 0; x < boardSizeX; x++) {
      for (int y = 0; y < boardSizeY; y++) {
        Piece piece = board[x,y].GetComponent<Piece>();
        if (piece.IsTouched()) {
          Destroy(board[x,y]);
          board[x,y] = null;
        }
      }
    }
    
    // animate existing pieces down
    for (int x = 0; x < boardSizeX; x++) {
      for (int y = 0; y < boardSizeY; y++) {
        // if this board area is null, animate the first non-empty piece found up the axis
        if (board[x,y] == null) {
          Vector3 position = PositionAt(x, y); // position to animate to
          for (int y2 = y + 1; y2 < boardSizeY; y2++) {
            // first non-null piece
            if (board[x,y2] != null) {
              Piece piece = board[x,y2].GetComponent<Piece>();
              piece.AnimatePieceDown(position.y);
              // assign the piece to it's new slot
              board[x,y] = board[x,y2];
              board[x,y2] = null;
              board[x,y].GetComponent<Piece>().SetCoords(x, y);
              break;
            }
          }
        }
      }
    }

    // animate new pieces down to empty slots
    for (int x = 0; x < boardSizeX; x++) {
      for (int y = 0; y < boardSizeY; y++) {
        if (board[x,y] == null) {
          GameObject piece = InstantiatePieceWithAnimation(x, y, startPosition);
          board[x,y] = piece;
          board[x,y].GetComponent<Piece>().SetCoords(x, y);
        }
      }
    }
    
    // call the user defined callback if it exists
    if (resultDelegate != null) {
      resultDelegate(touchedPieces[0].GetComponent<Piece>().pieceType, touchedPieces.Count);
    }
    
    touchedPieces.Clear();
    lastTouchedPiece = null;
  }
  
  void Update() {
    if (touchedPieces.Count >= minimumTouchLimit && InputSystem.Up(0)) {
      // reset marked pieces
       UnMarkExternals();
      
      ProcessBoard();
    } else if (InputSystem.Up(0)) {
      if (lastTouchedPiece != null) {
        // reset marked pieces
         UnMarkExternals();
        
        foreach (GameObject piece in touchedPieces) {
          piece.GetComponent<Piece>().SysUnTouchPiece();
        }

        touchedPieces.Clear();
      }
    }
  }

}
