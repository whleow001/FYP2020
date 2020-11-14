using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GunslingerSkillManager : MonoBehaviour
{
    private AudioSource audioSource;
    private EventsManager eventsManager;

    [SerializeField]
    private AudioClip sparkClip;

    // Start is called before the first frame update
    void Start()
    {
      audioSource = GetComponent<AudioSource>();
      eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
    }

    public void CueSound() {
      eventsManager.PlayGunslingerSkill_S(GetComponent<PhotonView>().ViewID);
    }

    public void PlaySource()
    {
        audioSource.PlayOneShot(sparkClip, .4f);
    }
}
