using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotor : MonoBehaviour {

    private Vector3 offset = new Vector3(-10, 9, -10);
    GameObject player;

    private Quaternion iniRot;

    // Start is called before the first frame update
    void Start() {
        //transform.rotation = Quaternion.Euler(30, 45, 0);
    }

    // Update is called once per frame
    void Update() {
      if (player != null)
        transform.position = offset + player.transform.position;
    }

    public void SetPlayer(GameObject _player) {
      player = _player;
    }
}
