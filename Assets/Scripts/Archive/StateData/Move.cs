/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "StateData/Move")]
public class Move : StateData {
  public float speed = 10.0f;
  float angle;

  public override void OnEnter(State state, Animator animator, AnimatorStateInfo stateInfo) {
    PlayerController playerController = state.GetPlayerController(animator);
  }

  public override void UpdateAbility(State state, Animator animator, AnimatorStateInfo stateInfo) {
    state.GetPlayerController(animator).Move();

    state.UpdateParams(state.GetRunParam(), state.GetAttackParam(), state.GetDodgeParam());
  }

  public override void OnExit(State state, Animator animator, AnimatorStateInfo stateInfo) {

  }
}*/
