using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle", menuName = "StateData/Idle")]
public class Idle : StateData {
  public override void OnEnter(State state, Animator animator, AnimatorStateInfo stateInfo) {
    
  }

  public override void UpdateAbility(State state, Animator animator, AnimatorStateInfo stateInfo) {
    PlayerController playerController = state.GetPlayerController(animator);

    state.UpdateParams(state.GetRunParam(), state.GetAttackParam(), state.GetDodgeParam());
  }

  public override void OnExit(State state, Animator animator, AnimatorStateInfo stateInfo) {

  }
}
