using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContainer : MonoBehaviourPun
{
    private GameObject _mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnCamera(GameObject camera, GameObject player)
    {
        if (!photonView.IsMine) return;

        if (_mainCamera == null)
        {
            _mainCamera = Instantiate(camera, Vector3.zero, Quaternion.Euler(30, 45, 0));
            _mainCamera.GetComponent<CameraMotor>().SetPlayer(player);
        }
        else
        {
            _mainCamera.GetComponent<CameraMotor>().SetPlayer(player);
        }
        
    }
}
