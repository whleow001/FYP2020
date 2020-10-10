using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // Inputs
    [Header("Inputs")]
    [SerializeField]
    private Joystick joystick;
    [SerializeField]
    private List<SkillButton> skillButtons;

    public enum Ability : int
    {
      Skill1 = 0,
      Skill2 = 1,
      Dodge = 2,
      Attack = 3
    }

    private void Start()
    {
        joystick = FindObjectOfType<Joystick>();
        Debug.Log(joystick);
    }

    public bool IsJoystickMoving() {
      return joystick.Horizontal != 0 || joystick.Vertical != 0;
    }

    public bool IsPressed(Ability index) {
      return skillButtons[(int)index].pressed;
    }

    public Vector2 GetJoystickVector() {
      return new Vector2(joystick.Vertical, joystick.Horizontal);
    }
}
