using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContainer : MonoBehaviourPun
{
    private GameObject _mainCamera;
    private PlayerManager playerManager;

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
          _mainCamera = Instantiate(camera, Vector3.zero, Quaternion.Euler(30, 45, 0));

        _mainCamera.GetComponent<CameraMotor>().SetPlayer(player);
    }

    public void SetPlayerManager(PlayerManager _playerManager) {
      playerManager = _playerManager;
    }

    //broadcast health to all clients in the server
    [PunRPC]
    void BroadcastHealth(Player victim)
    {
        playerManager.SetHealthBar((int)victim.CustomProperties["Health"]);
        /*PhotonView PV = PhotonView.Find(VictimID);
        Player victim = PV.Owner;
        Slider mainslider = PV.gameObject.GetComponentInChildren<Slider>();
        Image mainfill = PV.gameObject.transform.Find("Canvas").Find("Healthbar").Find("fill").GetComponent<Image>();
        GetComponent<PlayerManager>().SetHealthBar((int)victim.CustomProperties["Health"], mainslider, mainfill);*/
      }

    [PunRPC]
    void AllocateFOV()
    {
        playerManager.GetDirector().AllocateFOVMask();
    }

    [PunRPC]
    void Fire()
    {
      /*ray.origin = raycastOrigin.transform.position;
      ray.direction = raycastOrigin.forward;

      if (Physics.Raycast(ray, out hitInfo, range)) {
        if (hitInfo.collider.gameObject.layer != gameObject.layer) {
                if (hitInfo.collider.gameObject.tag == "Player")
                    hitInfo.transform.gameObject.GetComponent<PlayerController>().TakeDamage(20, photonView);
                else if (hitInfo.collider.gameObject.tag == "Generator")
                {
                    hitInfo.transform.gameObject.GetComponent<GeneratorHealth>().TakeDamage(20);
                }
        }
      }

      Debug.DrawRay(ray.origin, transform.TransformDirection(Vector3.forward) * range, Color.red, 0.5f);*/
    }
}
