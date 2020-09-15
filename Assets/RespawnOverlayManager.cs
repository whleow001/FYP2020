using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RespawnOverlayManager : MonoBehaviour
{
    [SerializeField]
    private Text timer;
    private Coroutine co;

    float currentTime = 0;
    public bool IsActive;

    // Update is called once per frame
    void Update() {
      IsActive = currentTime > 0;

      if (IsActive) {
        SetTimerText();
        currentTime -= Time.deltaTime;
      }
    }

    public void SetTimer(int seconds) {
      currentTime = seconds;
    }

    private void SetTimerText() {
      Debug.Log(currentTime);

      string minutes = (currentTime / 60).ToString("00");
      string seconds = (currentTime % 60).ToString("00");
      timer.text = $"{minutes}:{seconds}";
    }
}
