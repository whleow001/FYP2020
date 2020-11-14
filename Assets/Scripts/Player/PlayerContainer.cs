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

    // skill variables
    //private bool skillActive = false;
    private Coroutine usedSkillCoroutine;
    private bool readyForSkill = true;

    // Start is called before the first frame update
    void Awake()
    {
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        //Debug.Log("my own layer: " + playerManager.GetDirector().GetFactionLayer());
        //Debug.Log("opposite team layer: " + playerManager.GetDirector().GetOtherFactionLayer());
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

    public GameObject GetCamera()
    {
        return _mainCamera;
    }

    private void OnCollisionEnter(Collision other) {

        if (photonView.IsMine)
        {
            if (other.gameObject.tag == "Projectile" && other.gameObject.layer == playerManager.GetDirector().GetOtherFactionLayer())
            {
                //Debug.Log(other.gameObject.layer);
                //Debug.Log(other.gameObject.GetComponent<PhotonViewReference>().GetPhotonView());
                playerManager.TakeDamage(new Damage(20, other.gameObject.transform.position), other.gameObject.GetComponent<PhotonViewReference>().GetPhotonView().ViewID, other.gameObject.GetComponent<PhotonViewReference>().GetBot());
            }
        }
    }

    public void TakeDamage(Damage dmg)
    {
        if (photonView.IsMine)
        {
            playerManager.TakeDamage(dmg);
        }
    }

    public bool IsDead()
    {
        if (playerManager.GetProperty("Health") <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator skillDuration()
    {
        yield return new WaitForSeconds(3);
        gameObject.transform.Find("FireBall").gameObject.SetActive(false);
        gameObject.transform.Find("Shield").gameObject.SetActive(false);
        if (photonView.IsMine)
        {
            playerManager.setAnimatorSpeed(1);
            playerManager.GetComponent<PlayerController>().MoveSpdUp();
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
    void InstantiateBullet(Vector3 position, Vector3 velocity, int layer, int skill, int botPosition, string botName, PhotonMessageInfo info)
    {
        playerManager.GetComponent<PlayerController>().InstantiateBullet(position, velocity, layer, skill, botPosition, botName, info.photonView);

    }

    [PunRPC]
    void ChangeIcons(int viewID)
    {
        PhotonView PV = PhotonView.Find(viewID);
        Player player = PV.Owner;

        //broadcast nameplate to all client
        Text name = PV.gameObject.transform.Find("Canvas").Find("Text").GetComponent<Text>();
        name.text = player.NickName;
        name.color = Color.white;
        Image healthColor = PV.gameObject.transform.Find("Canvas").Find("Healthbar").Find("fill").GetComponent<Image>();

        if ((byte)player.CustomProperties["_pt"] == 0)
        {
            healthColor.color = Color.red;
            for (int i = 0; i < playerManager.GetDirector().GovtPlayers.Length; i++)
            {
                if (playerManager.GetDirector().GovtPlayers[i] == player)
                {
                    playerManager.GovtIcons.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UISprites/number" + (int)player.CustomProperties["Class"]);
                }
            }
        }
        else
        {
            healthColor.color = Color.blue;
            for (int i = 0; i < playerManager.GetDirector().RebelPlayers.Length; i++)
            {
                if (playerManager.GetDirector().RebelPlayers[i] == player)
                {
                    playerManager.RebelIcons.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UISprites/number" + (int)player.CustomProperties["Class"]);
                }
            }
        }
    }

    [PunRPC]
    void ShowSkillEffect(int viewID, int classIndex)
    {
        int currentClass = classIndex;
        Debug.Log(currentClass);
        playerManager.GetComponent<PlayerController>().SkillActive = true;
        if(gameObject.GetPhotonView().ViewID == viewID)
        {
            //gunslinger
            if (currentClass == 1)
            {
                gameObject.transform.Find("FireBall").gameObject.SetActive(true);
                if (photonView.IsMine)
                {
                    playerManager.GetComponent<PlayerController>().MoveSpdUp();
                    playerManager.setAnimatorSpeed(100);
                }

            }
            //sniper
            else if (currentClass == 2)
            {

            }
            //juggernaut
            else
            {
                gameObject.transform.Find("Shield").gameObject.SetActive(true);
            }
            playerManager.GetComponent<PlayerController>().SkillActive = false;
            usedSkillCoroutine = StartCoroutine(skillDuration());
        }

    }
}
