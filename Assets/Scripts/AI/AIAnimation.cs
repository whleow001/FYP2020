using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAnimation : MonoBehaviour
{
  private Animator anim;
  private AIController aiController;

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
    aiController = GetComponent<AIController>();

    paramList.Add(isIdleParam);
    paramList.Add(isRunningParam);
    paramList.Add(isAttackingParam);
    paramList.Add(dodgeParam);
    paramList.Add(deadParam);
    paramList.Add(victoryParam);
    paramList.Add(defeatParam);
  }

  private void Update() {
    if (anim) {
      foreach (PlayerController.CharacterState state in Enum.GetValues(typeof(PlayerController.CharacterState))) {
        if (state != PlayerController.CharacterState.Dodging)
          anim.SetBool(paramList[(int)state], aiController.characterState == state);
      }
    }
  }

  public void ReinitializeAnimator(GameObject aiClone) {
    anim = aiClone.GetComponent<Animator>();
  }
}
