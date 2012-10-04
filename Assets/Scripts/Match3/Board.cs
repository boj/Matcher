using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {

  // pieces
  public GameObject[] piecePrefabs;
  // board settings
  public int boardSizeX;
  public int boardSizeY;
  public int lineSpacing;
  public bool animateOnStart = false; // animate the pieces to fall into place, otherwise instantiate in place
  public float startPosition = 200; // the point at which piece start falling in regards to their animation axis
  public int minimumTouchLimit = 3;
  
  private GameObject[,] board;
  private List<GameObject> touchedPieces = new List<GameObject>();
  private GameObject lastTouchedPiece;
  
  void Start() {
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
    return new Vector3(xLoc, yLoc, transform.position.z);
  }
  
  private GameObject InstantiatePiece(int x, int y) {
    GameObject piece = (GameObject)Instantiate(piecePrefabs[Random.Range(0, piecePrefabs.Length)], PositionAt(x, y), Quaternion.identity);
    piece.GetComponent<Piece>().SetBoardRef(GetComponent<Board>());
    piece.transform.parent = transform;
    return piece;
  }
  
  private GameObject InstantiatePieceWithAnimation(int x, int y, float start) {
    GameObject piece = InstantiatePiece(x, y);
    piece.GetComponent<Piece>().AnimatePieceStart(piece.transform.position.x, piece.transform.position.y, start);
    return piece;
  }
  
  public void Reset() {
    // init board
    board = new GameObject[boardSizeX, boardSizeY];
    
    // init pieces
    for (int x = 0; x < boardSizeX; x++) {
      for (int y = 0; y < boardSizeY; y++) {
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
  
  public GameObject LastTouchedPiece() {
    return lastTouchedPiece;
  }
  
  public bool PieceIsConnected(GameObject piece) {
    foreach (GameObject p in touchedPieces) {
      if (piece == p) {
        return true;
      }
    }
    return false;
  }
  
  public bool PieceIsPreviousConnected(GameObject piece) {
    if (touchedPieces.Count > 1) {
      if (piece == touchedPieces[touchedPieces.Count - 2]) {
        touchedPieces.RemoveAt(touchedPieces.Count - 1);
        lastTouchedPiece.GetComponent<Piece>().UnTouchPiece();
        lastTouchedPiece = piece;
        return true;
      }
    }
    return false;
  }
  
  public bool CheckSurroundingPieces(GameObject piece) {
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
  
  public bool MatchesType(GameObject piece) {
    if (piece.GetComponent<Piece>().pieceType == touchedPieces[0].GetComponent<Piece>().pieceType) {
      return true;
    }
    return false;
  }
  
  public bool AddTouchedPiece(GameObject piece) {
    // if the list is empty then it's a new touch round
    // if we're extending the list and this is the same piece type, add it
    if (touchedPieces.Count == 0 || (touchedPieces.Count > 0 && MatchesType(piece) && CheckSurroundingPieces(piece))) {
      touchedPieces.Add(piece);
      lastTouchedPiece = piece;
      return true;
    }
    return false;
  }
  
  private void ProcessBoard() {
    touchedPieces.Clear();
    
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
  }
  
  void Update() {
    if (touchedPieces.Count >= minimumTouchLimit && InputSystem.Up(0)) {
      ProcessBoard();
    } else if (InputSystem.Up(0)) {
      foreach (GameObject piece in touchedPieces) {
        piece.GetComponent<Piece>().UnTouchPiece();
      }
      touchedPieces.Clear();
    }
  }

}
