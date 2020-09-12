using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ForceTransition", menuName = "StateData/ForceTransition")]
public class ForceTransition : StateData {

  [Range(0.01f, 1f)]
  public float transitionTiming = 0.8f;
  public bool dodgeAnimation;

  public override void OnEnter(State state, Animator animator, AnimatorStateInfo stateInfo) {
    state.GetPlayerController(animator);

    if (dodgeAnimation)
      state.GetPlayerController(animator).Dodge();
  }

  public override void UpdateAbility(State state, Animator animator, AnimatorStateInfo stateInfo) {
    animator.SetBool(state.GetForceTransitionParam(), stateInfo.normalizedTime >= transitionTiming);
  }

  public override void OnExit(State state, Animator animator, AnimatorStateInfo stateInfo) {
    animator.SetBool(state.GetForceTransitionParam(), false);
  }
}
