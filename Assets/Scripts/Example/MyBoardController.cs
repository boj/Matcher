using UnityEngine;
using System.Collections;

public class MyBoardController : MonoBehaviour {

  void Start() {
    GetComponent<Board>().RegisterCallback(ProcessResult);
  }
  
  void ProcessResult(string type, int count) {
    print("TYPE: " + type + ", COUNT: " + count);
  }
  
}
