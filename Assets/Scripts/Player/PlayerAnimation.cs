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
    private int isDodingParam = Animator.StringToHash("isDodging");
    private int deadParam = Animator.StringToHash("isDead");
    private int victoryParam = Animator.StringToHash("hasWon");
    private int defeatParam = Animator.StringToHash("hasLost");

    private List<int> paramList = new List<int>();

    void Start() {
      anim = GetComponent<PlayerManager>().GetPlayerClone().GetComponent<Animator>();
      playerController = GetComponent<PlayerController>();

      paramList.Add(isIdleParam);
      paramList.Add(isRunningParam);
      paramList.Add(isAttackingParam);
      paramList.Add(isDodingParam);
      paramList.Add(deadParam);
      paramList.Add(victoryParam);
      paramList.Add(defeatParam);
    }

    // Update is called once per frame
    void Update() {
      if (anim)
        foreach (PlayerController.CharacterState state in Enum.GetValues(typeof(PlayerController.CharacterState)))
          anim.SetBool(paramList[(int)state], playerController.characterState == state);
    }
}
