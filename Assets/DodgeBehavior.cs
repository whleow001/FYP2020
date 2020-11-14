using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgeBehavior : StateMachineBehaviour
{
    [Range(0.01f, 1f)]
    public float transitionTiming = 0.8f;

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
      float normalizedTime = stateInfo.normalizedTime - Mathf.Floor(stateInfo.normalizedTime);

      if (normalizedTime >= transitionTiming)
        animator.gameObject.GetComponentInParent<PlayerContainer>().GetPlayerManager().GetComponent<PlayerController>().OnDodgeAnimationFinish();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
      animator.gameObject.GetComponentInParent<PlayerContainer>().GetPlayerManager().GetComponent<PlayerController>().OnDodgeAnimationFinish();
    }
}
