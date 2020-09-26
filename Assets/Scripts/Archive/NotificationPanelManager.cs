using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationPanelManager : MonoBehaviour
{
    private Text text;

    // Start is called before the first frame update
    void Awake() {
      text = transform.GetChild(0).GetComponent<Text>();
    }

    public void SetText(string message) {
      text.text = message;
    }

    public void SetActiveState(bool state) {
      gameObject.SetActive(state);
    }
}
