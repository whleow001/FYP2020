using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SniperSkillManager : MonoBehaviour
{
    private AudioSource audioSource;
    private EventsManager eventsManager;

    [SerializeField]
    private AudioClip laserClip;

    // Start is called before the first frame update
    void Start()
    {
      audioSource = GetComponent<AudioSource>();
      eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
    }

    public void CueSound() {
      eventsManager.PlaySniperSkill_S(GetComponent<PhotonView>().ViewID);
    }

    public void PlaySource()
    {
        audioSource.PlayOneShot(laserClip, .4f);
    }
}
