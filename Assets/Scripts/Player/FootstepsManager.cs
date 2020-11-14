using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FootstepsManager : MonoBehaviour
{
    private AudioSource audioSource;
    private EventsManager eventsManager;
    private bool playFootsteps = false;
    private Coroutine walkCoroutine;

    [SerializeField]
    private AudioClip walkingClip;

    // Start is called before the first frame update
    void Start()
    {
      audioSource = GetComponent<AudioSource>();
      eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
    }

    // Update is called once per frame
    void Update()
    {
      if (playFootsteps && walkCoroutine == null)
        walkCoroutine = StartCoroutine("Play");
    }

    public void SetPlayFootsteps(bool play) {
      playFootsteps = play;
    }

    private IEnumerator Play() {
      eventsManager.PlayFootsteps_S(GetComponent<PhotonView>().ViewID);
      yield return new WaitForSeconds(0.4f);

      if (playFootsteps)
        walkCoroutine = StartCoroutine("Play");
      else
        walkCoroutine = null;
    }

    public void PlaySource()
    {
      audioSource.PlayOneShot(walkingClip, .2f);
    }
}
