using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotor : MonoBehaviour {

    private Vector3 offset = new Vector3(-10, 9, -10);
    GameObject player;

    // Update is called once per frame
    void Update() {
      if (player != null)
        transform.position = offset + player.transform.position;
    }

    public void SetPlayer(GameObject _player) {
      player = _player;
    }
}
