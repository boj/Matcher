using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour {
  
  public string pieceType;
  public float touchScaleFactor = 2.0f;
  public int animationSpeed = 10;
  private float animationDest;
  private bool isTouched = false;
  private bool isAnimating;
  private Vector2 coords;
  
  private Board board;
  
  public void SetBoardRef(Board b) {
    board = b;
  }
  
  public void SetCoords(int x, int y) {
    coords.x = x;
    coords.y = y;
  }
  
  public Vector2 GetCoords() {
    return coords;
  }

  public void AnimatePieceStart(float x, float y, float start) {
    transform.position += new Vector3(0, start, 0);
    animationDest = y;
    isAnimating = true;
  }
  
  public void AnimatePieceDown(float dest) {
    animationDest = dest;
    isAnimating = true;
  }
  
  public virtual void DoAnimate() {
    transform.Translate(-Vector3.up * animationSpeed);
    if (transform.position.y <= animationDest) {
      // make sure pieces line up exactly
      transform.position = new Vector3(transform.position.x, animationDest, transform.position.z);
      isAnimating = false;
    }
  }
  
  public bool IsTouched() {
    return isTouched;
  }
  
  public void CheckPiece() {
    if (board.LastTouchedPiece() == gameObject || board.PieceIsPreviousConnected(gameObject) || board.PieceIsConnected(gameObject)) {
      return;
    // if this is a connectable piece add it
    } else if (board.AddTouchedPiece(gameObject)) {
      TouchPiece();
    }
  }
  
  public void TouchPiece() {
    transform.localScale -= new Vector3(touchScaleFactor, touchScaleFactor, 0);
    isTouched = true;
  }
  
  public void UnTouchPiece() {
    transform.localScale += new Vector3(touchScaleFactor, touchScaleFactor, 0);
    isTouched = false;
  }
  
  void Update() {
    if (InputSystem.Down(0)) {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;
      if (collider.Raycast(ray, out hit, 1000)) {
        if (hit.transform == transform) {
          CheckPiece();
        }
      }
    }
    
    if (isAnimating) {
      DoAnimate();
    }
  }
  
}
