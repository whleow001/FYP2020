using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FiringManager : MonoBehaviour
{
    private AudioSource audioSource;
    private EventsManager eventsManager;

    [SerializeField]
    private AudioClip firingClip;

    // Start is called before the first frame update
    void Start()
    {
      audioSource = GetComponent<AudioSource>();
      eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
    }

    public void CueSound() {
      print("Cueing");
      eventsManager.PlayNormalAttack_S(GetComponent<PhotonView>().ViewID);
    }

    public void PlaySource()
    {
        print("Playing firing");
        audioSource.PlayOneShot(firingClip, .4f);
    }
}
