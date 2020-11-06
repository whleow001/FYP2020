using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private PlayerController playerController;

    private int isIdleParam = Animator.StringToHash("isIdle");
    private int isRunningParam = Animator.StringToHash("isRunning");
    private int isAttackingParam = Animator.StringToHash("isAttacking");
    private int dodgeParam = Animator.StringToHash("dodge");
    private int deadParam = Animator.StringToHash("isDead");
    private int victoryParam = Animator.StringToHash("hasWon");
    private int defeatParam = Animator.StringToHash("hasLost");

    private List<int> paramList = new List<int>();

    private bool triggered = false;

    private void Start() {
      // ReinitializeAnimator();
      playerController = GetComponent<PlayerController>();

      paramList.Add(isIdleParam);
      paramList.Add(isRunningParam);
      paramList.Add(isAttackingParam);
      paramList.Add(dodgeParam);
      paramList.Add(deadParam);
      paramList.Add(victoryParam);
      paramList.Add(defeatParam);
    }

    // Update is called once per frame
    private void Update() {
      if (anim) {
        foreach (PlayerController.CharacterState state in Enum.GetValues(typeof(PlayerController.CharacterState)))
          if (state != PlayerController.CharacterState.Dodging)
            anim.SetBool(paramList[(int)state], playerController.characterState == state);

        if (playerController.characterState == PlayerController.CharacterState.Dodging) {
          if (!triggered) {
            anim.SetTrigger(dodgeParam);
            triggered = true;
          }
        }
        else
          triggered = false;
      }
      else
        ReinitializeAnimator();
    }

    public void ReinitializeAnimator() {
      PlayerManager playerManager = GetComponent<PlayerManager>();
      GameObject playerClone = playerManager.GetPlayerClone();

      if (playerClone)
        anim = playerClone.GetComponent<Animator>();
      // anim = GetComponent<PlayerManager>().GetPlayerClone().GetComponent<Animator>();
    }
}
