using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class SkillButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler {

    [HideInInspector]
    public bool pressed;

    [SerializeField]
    private bool hasCooldown;
    private int cooldown = 5;
    private int cooldownTime;
    private Coroutine dodgeCoroutine;
    private bool enabled = true;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    private IEnumerator CooldownTimer() {
      gameObject.transform.GetChild(0).GetChild(0).gameObject.SetActive(cooldownTime > 0);
      gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = cooldownTime.ToString();

      cooldownTime--;

      if (cooldownTime < 0) {
        dodgeCoroutine = null;
        enabled = true;
      }
      else {
        yield return new WaitForSeconds(1.0f);
        dodgeCoroutine = StartCoroutine(CooldownTimer());
      }

    }

    public void OnPointerDown(PointerEventData eventData) {
      if (enabled) {
        pressed = true;

        if (hasCooldown) {
          enabled = false;
          cooldownTime = cooldown;
          dodgeCoroutine = StartCoroutine(CooldownTimer());
        }
      }
    }

    public void OnPointerUp(PointerEventData eventData) {
      pressed = false;
    }
}
