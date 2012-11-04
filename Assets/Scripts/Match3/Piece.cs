//-----------------------------------------------------------------
//  Copyright 2012 Brian Jones - Uncanny Works
//	All rights reserved
//-----------------------------------------------------------------

using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (BoxCollider))]
public class Piece : MonoBehaviour {
  
  public string pieceType;
  public float touchScaleFactor = 2.0f;
  public int animationSpeed = 10;
  private bool isTouched = false;
  private bool isAnimating;
  private Vector2 coords;
  private Board board;
  [HideInInspector]
  public float animationDest;
  
  // <summary>
  // Overrideable method for handling logic in regards to the direction the piece was touched from.
  // </summary>
  // <param name="direction">
  // 0 - Resets
  // 1 - Touched
  // 2 - Touched Down
  // 3 - Touched Left
  // 4 - Touched Lower Left
  // 5 - Touched Lower Right
  // 6 - Touched Right
  // 7 - Touched Up
  // 8 - Touched Upper Left
  // 9 - Touched Upper Right
  // </param>
  public virtual void Touched(int direction) {
    
  }
  
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
  
  public void SetAnimating(bool a) {
    isAnimating = a;
  }
  
  public void SetTouched(bool t) {
    isTouched = t;
  }
  
  public bool IsTouched() {
    return isTouched;
  }
  
  public bool IsAnimating() {
    return isAnimating;
  }

  public void AnimatePieceStart(float x, float y, float start) {
    transform.position += new Vector3(0, start, 0);
    animationDest = y;
    SetAnimating(true);
  }

  public void AnimatePieceDown(float dest) {
    animationDest = dest;
    SetAnimating(true);
  }
  
  public void SysDoAnimate() {
    DoAnimate();
    if (transform.position.y <= animationDest) {
      AnimationFinished();
    }
  }
  
  public void AnimationFinished() {
    // make sure pieces line up exactly
    transform.position = new Vector3(transform.position.x, animationDest, transform.position.z);
    SetAnimating(false);
  }
  
  //// <summary>
  //// Animates the piece very generically along the Y axis.
  //// Override to use other animate methods, such as iTween.
  //// Make sure to call AnimationFinished() if setting isAnimating = false and overriding the logic.
  //// </summary>
  public virtual void DoAnimate() {
    transform.Translate(-Vector3.up * animationSpeed);
  }
  
  public void SysTouchPiece() {
    TouchPiece();
    SetTouched(true);
  }
  
  //// <summary>
  //// Defines what happens which this piece is touched.  By default slightly reduces the piece scale.
  //// Override to alter it's behaviour.
  //// </summary>
  public virtual void TouchPiece() {
    transform.localScale -= new Vector3(touchScaleFactor, touchScaleFactor, 0);
  }
  
  public void SysUnTouchPiece() {
    UnTouchPiece();
    SetTouched(false);
    Touched(0); // untouched command
  }
  
  //// <summary>
  //// Defines what happens which this piece is touched.  By default slightly increases the piece scale.
  //// Override to alter it's behaviour.
  //// </summary>
  public virtual void UnTouchPiece() {
    transform.localScale += new Vector3(touchScaleFactor, touchScaleFactor, 0);
  }
  
  public void CheckPiece() {
    // if this is a connectable piece add it
    if (board.AddTouchedPiece(gameObject)) {
      SysTouchPiece();
    }
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
    
    if (IsAnimating()) {
      SysDoAnimate();
    }
  }
  
}
