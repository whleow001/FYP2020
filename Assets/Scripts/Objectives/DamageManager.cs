using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DamageManager : MonoBehaviour
{
    private AudioSource audioSource;
    private EventsManager eventsManager;

    [SerializeField]
    private AudioClip onHitClip;

    // Start is called before the first frame update
    void Start()
    {
      audioSource = GetComponent<AudioSource>();
      eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
    }

    public void CueSound() {
      eventsManager.PlayOnHit_S(GetComponent<PhotonView>().ViewID);
    }

    public void PlaySource()
    {
        audioSource.PlayOneShot(onHitClip, .4f);
    }
}
