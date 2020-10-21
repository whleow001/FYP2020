using System;
﻿using System.Collections;
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

    public bool IsJoystickMoving() {
      return joystick.Horizontal != 0 || joystick.Vertical != 0;
    }

    public bool IsPressed(Ability index) {
      return skillButtons[(int)index].pressed;
    }

    public int GetJoystickAngle() {
      double angle = (Math.Atan2(joystick.Vertical, -joystick.Horizontal) * 180 / Math.PI) - 90;
      if (angle < 0)
        angle += 360;

      return Convert.ToInt32(angle+45);
    }
}
