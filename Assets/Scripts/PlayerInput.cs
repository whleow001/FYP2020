using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // Controller reference
    PlayerController playerController;

    // Inputs
    Joystick js;
    SkillButton skill1;
    SkillButton skill2;
    SkillButton dodge;
    SkillButton attack;

    // Start is called before the first frame update
    void Start() {
      playerController = GetComponent<PlayerController>();

      js = FindObjectOfType<Joystick>();
      skill1 = GameObject.Find("skill1").GetComponent<SkillButton>();
      skill2 = GameObject.Find("skill2").GetComponent<SkillButton>();
      dodge = GameObject.Find("dodge").GetComponent<SkillButton>();
      attack = GameObject.Find("attack").GetComponent<SkillButton>();
    }

    // Update is called once per frame
    void Update() {
      playerController.isMoving = js.Horizontal != 0 || js.Vertical != 0;

      if (skill1.pressed) {
        Debug.Log("Skill 1 pressed");
      }

      else if (skill2.pressed) {
        Debug.Log("Skill 2 pressed");
      }

      playerController.isDodging = dodge.pressed;
      playerController.isAttacking = attack.pressed;
    }

    public Vector2 GetJoystickVector() {
      return new Vector2(js.Vertical, js.Horizontal);
    }
}
