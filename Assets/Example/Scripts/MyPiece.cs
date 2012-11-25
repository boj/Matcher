using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MyPiece : Piece {

  //private PackedSprite packedSprite;
  
  void Start() {
    //packedSprite = GetComponent<PackedSprite>();
    iTween.Init(gameObject);
  }
  
  public override void DoAnimate() {
    SetAnimating(false); // since we are letting iTween handle animation, disable this
    Hashtable tweenHash = new Hashtable();
    tweenHash.Add("speed",      animationSpeed);
    tweenHash.Add("easetype",   iTween.EaseType.linear);
    tweenHash.Add("y",          animationDest);
    tweenHash.Add("oncomplete", "AnimationFinished");
    iTween.MoveTo(gameObject, tweenHash);
  }

  //public override void TouchPiece() {
  //  packedSprite.SetSize(packedSprite.width - touchScaleFactor, packedSprite.height - touchScaleFactor);
  //}
  //
  //public override void UnTouchPiece() {
  //  packedSprite.SetSize(packedSprite.width + touchScaleFactor, packedSprite.height + touchScaleFactor);
  //}
  
}
