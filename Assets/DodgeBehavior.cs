using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgeBehavior : StateMachineBehaviour
{
    [Range(0.01f, 1f)]
    public float transitionTiming = 0.8f;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
      animator.gameObject.GetComponentInParent<PlayerContainer>().GetPlayerManager().GetComponent<PlayerController>().OnDodgeAnimationFinish();
    }
}
