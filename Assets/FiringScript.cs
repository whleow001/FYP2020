using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiringScript : StateMachineBehaviour
{
    [Range(0.01f, 1f)]
    public float fireTiming = 0.5f;

    private bool fired = false;

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
      float normalizedTime = stateInfo.normalizedTime - Mathf.Floor(stateInfo.normalizedTime);

      if (normalizedTime >= fireTiming && normalizedTime < fireTiming + 0.1f && !fired) {
        animator.gameObject.GetComponentInParent<PlayerContainer>().GetPlayerManager().GetComponent<PlayerController>().ReadyForFiring(true);
        fired = true;
      }
      else {
        animator.gameObject.GetComponentInParent<PlayerContainer>().GetPlayerManager().GetComponent<PlayerController>().ReadyForFiring(false);
        fired = false;
      }
    }
}
