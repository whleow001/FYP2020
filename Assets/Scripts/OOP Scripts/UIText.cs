using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIText : MonoBehaviour {
  // Panel
  [SerializeField]
  private GameObject panel;

  // Text
  [SerializeField]
  private Text text;

  // Timer
  [SerializeField]
  private bool ActiveOnDefault = true;
  private bool timer = false;
  private string message;
  private float secondsElasped;
  private float seconds;

    public bool OverrideCurrentText = false;

  void Awake() {
    SetActiveState(ActiveOnDefault);
  }

  void Update() {
    if (secondsElasped != 0) {
      secondsElasped -= Time.deltaTime;

            if (secondsElasped <= 0)
            {
                SetActiveState(false);
                OverrideCurrentText = false;
            }

    }

    if (timer)
      text.text = message + GetTimerText();
  }

  // Function to set the text of component
  public void SetText(string _message, float _seconds = 0, bool _timer = false) {
    /*
    Parameters
    ==========
    message : 'str'
      The message to be written on text
    seconds : 'float', optional
      The time the panel will be active
    timer : 'bool', optional
      If the text should be updated every second with timer countdown
    */

    if (!_timer)
      text.text = _message;
    else {
      timer = _timer;
      message = _message;

      text.text = message + GetTimerText();
    }

    if (_seconds != 0) {
      SetActiveState(true);
      secondsElasped = _seconds;
    }
  }

  // Function to convert timer to minutes and seconds
  string GetTimerText() {
    string minutes = (secondsElasped / 60).ToString("00");
    string seconds = (secondsElasped % 60).ToString("00");
    return $"{minutes}:{seconds}";
  }

    // Function to change state of panel
    // had to change protection level for eventsmanager access
    public void SetActiveState(bool state) {
    panel.SetActive(state);
  }
}
