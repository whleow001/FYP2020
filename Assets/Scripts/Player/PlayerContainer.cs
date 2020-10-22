using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerContainer : MonoBehaviourPun
{
    private GameObject _mainCamera;
    private PlayerManager playerManager;

    private bool fovInstantiated = false;

    // Start is called before the first frame update
    void Awake()
    {
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fovInstantiated == false)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.Log("FOV");
                photonView.RPC("AllocateFOV", RpcTarget.All);
                fovInstantiated = true;
            }
        }
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

    public PlayerManager GetPlayerManager() {
      return playerManager;
    }

    private void OnCollisionEnter(Collision other) {

      if (other.gameObject.tag == "Projectile" && other.gameObject.layer == playerManager.GetDirector().GetOtherFactionLayer()) {
        playerManager.TakeDamage(20, other.gameObject.GetComponent<PhotonViewReference>().GetPhotonView());
      }
    }

    //broadcast health to all clients in the server
    [PunRPC]
    void BroadcastHealth(int victimID)
    {
        //playerManager.SetHealthBar((int)victim.CustomProperties["Health"]);
        PhotonView PV = PhotonView.Find(victimID);
        Player victim = PV.Owner;
        Slider mainslider = PV.gameObject.GetComponentInChildren<Slider>();
        Image mainfill = PV.gameObject.transform.Find("Canvas").Find("Healthbar").Find("fill").GetComponent<Image>();
        playerManager.SetHealthBar((int)victim.CustomProperties["Health"], mainslider, mainfill);
        //GetComponent<PlayerManager>().SetHealthBar((int)victim.CustomProperties["Health"], mainslider, mainfill);
      }

    [PunRPC]
    void AllocateFOV()
    {
        playerManager.GetDirector().AllocateFOVMask();
    }

    [PunRPC]
    void ChangeMaterials(int ParentViewID, int ModelViewID, int selectedMaterial, int selectedLayer)
    {
        PhotonView ParentPV = PhotonView.Find(ParentViewID);
        PhotonView ModelPV = PhotonView.Find(ModelViewID);
        ParentPV.gameObject.layer = selectedLayer;
        Material[] materials = ModelPV.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().materials;
        materials[1] = playerManager.GetTeamMaterials(selectedMaterial);
        ModelPV.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().materials = materials;
        ModelPV.gameObject.layer = selectedLayer;
    }

    [PunRPC]
    void InstantiateBullet(Vector3 position, Vector3 velocity, int layer, PhotonMessageInfo info)
    {
        Debug.Log("Instantiate");
        playerManager.GetComponent<PlayerController>().InstantiateBullet(position, velocity, layer, info.photonView);
    }

    [PunRPC]
    void ChangeIcons(int viewID)
    {
        PhotonView PV = PhotonView.Find(viewID);
        Player player = PV.Owner;

        if ((byte)player.CustomProperties["_pt"] == 0)
        {
            for (int i = 0; i < playerManager.GetDirector().GovtPlayers.Length; i++)
            {
                if (playerManager.GetDirector().GovtPlayers[i] == player)
                {
                    playerManager.GovtIcons.transform.GetChild(i).GetComponent<Image>().sprite = Resources.Load<Sprite>("UISprites/number" + (int)player.CustomProperties["Class"]);
                }
            }
        }
        else
        {
            for (int i = 0; i < playerManager.GetDirector().RebelPlayers.Length; i++)
            {
                if (playerManager.GetDirector().RebelPlayers[i] == player)
                {
                    playerManager.RebelIcons.transform.GetChild(i).GetComponent<Image>().sprite = Resources.Load<Sprite>("UISprites/number" + (int)player.CustomProperties["Class"]);
                }
            }
        }
    }
}
