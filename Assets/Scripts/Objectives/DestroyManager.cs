using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DestroyManager : MonoBehaviour
{
    private AudioSource audioSource;
    private EventsManager eventsManager;

    [SerializeField]
    private AudioClip explosionClip;

    // Start is called before the first frame update
    void Start()
    {
      audioSource = GetComponent<AudioSource>();
      eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
    }

    public void CueSound() {
      eventsManager.PlayExplosion_S(GetComponent<PhotonView>().ViewID);
    }

    public void PlaySource()
    {
        audioSource.PlayOneShot(explosionClip, .4f);
    }
}
