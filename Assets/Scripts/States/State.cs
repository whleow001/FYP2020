using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State : StateMachineBehaviour {
    // References
    protected PlayerController playerController;
    protected Animator animator;

    // List of abilities for this state
    public List<StateData> abilityDataList = new List<StateData>();

    // Animator params
    private int runParam = Animator.StringToHash("run");
    private int attackParam = Animator.StringToHash("attack");
    private int forceTransitionParam = Animator.StringToHash("forceTransition");
    private int dodgeParam = Animator.StringToHash("dodge");

    public PlayerController GetPlayerController(Animator _animator) {
      if (playerController == null) {
        animator = _animator;
        playerController = animator.GetComponentInParent<PlayerController>();
      }
      return playerController;
    }

    public void UpdateParams(params int[] toUpdate) {
      if (!playerController.IsPhotonViewMine()) return;
      
      foreach (int param in toUpdate)
        animator.SetBool(param, GetParamProperty(param));
    }

    private bool GetParamProperty(int param) {
      if (param == runParam)
        return playerController.isMoving;
      else if (param == attackParam)
        return playerController.isAttacking;
      else if (param == dodgeParam)
        return playerController.isDodging;
      return false;
    }

    // Animation state override functions
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      foreach (StateData stateData in abilityDataList)
        stateData.OnEnter(this, animator, stateInfo);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      foreach (StateData stateData in abilityDataList)
        stateData.UpdateAbility(this, animator, stateInfo);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      foreach (StateData stateData in abilityDataList)
        stateData.OnExit(this, animator, stateInfo);
    }

    // Gets for animator params
    public int GetRunParam() {
      return runParam;
    }

    public int GetAttackParam() {
      return attackParam;
    }

    public int GetForceTransitionParam() {
      return forceTransitionParam;
    }

    public int GetDodgeParam() {
      return dodgeParam;
    }
}
