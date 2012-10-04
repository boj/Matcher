using UnityEngine;

public class InputSystem {
  
  public static bool Down(int t) {
    if ((Input.touchCount > 0 && Input.GetTouch(t).phase == TouchPhase.Began) || Input.GetMouseButton(t)) {
      return true;
    } else {
      return false;
    }
  }
  
  public static bool DownOne(int t) {
    if ((Input.touchCount > 0 && Input.GetTouch(t).phase == TouchPhase.Began) || Input.GetMouseButtonDown(t)) {
      return true;
    } else {
      return false;
    }
  }
  
  public static bool Up(int t) {
    if ((Input.touchCount > 0 && Input.GetTouch(t).phase == TouchPhase.Ended) || !Input.GetMouseButton(t)) {
      return true;
    } else {
      return false;
    }
  }
  
  public static bool UpOne(int t) {
    if ((Input.touchCount > 0 && Input.GetTouch(t).phase == TouchPhase.Ended) || !Input.GetMouseButtonUp(t)) {
      return true;
    } else {
      return false;
    }
  }
  
  public static Vector3 Position(int t) {
    if (Input.touchCount > 0) {
      return Input.GetTouch(t).position;
    } else {
      return Input.mousePosition;
    }
  }
  
}
