using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[CreateAssetMenu(fileName = "Attack", menuName = "StateData/Attack")]
public class Attack : StateData {

  [Range(0.01f, 1f)]
  public float attackTime;
  private bool attacked;

  public override void OnEnter(State state, Animator animator, AnimatorStateInfo stateInfo) {
    state.GetPlayerController(animator);

    attacked = false;
  }

  public override void UpdateAbility(State state, Animator animator, AnimatorStateInfo stateInfo) {
    float actualNormalizedTime = stateInfo.normalizedTime - (float)Math.Truncate(stateInfo.normalizedTime);
    //Debug.Log(actualNormalizedTime);

    if (actualNormalizedTime >= attackTime && !attacked) {
      attacked = true;

      state.GetPlayerController(animator).TurnAndFireNearestTarget();
      if (state.GetPlayerController(animator).ReadyForFiring)
        state.GetPlayerController(animator).photonView.RPC("Fire", RpcTarget.All);
    }

    if (actualNormalizedTime < attackTime)
      attacked = false;

    state.UpdateParams(state.GetRunParam(), state.GetAttackParam());
  }

  public override void OnExit(State state, Animator animator, AnimatorStateInfo stateInfo) {
    state.GetPlayerController(animator).ReadyForFiring = false;
  }
}
