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
        if (animator.gameObject.GetComponentInParent<PlayerContainer>())
          animator.gameObject.GetComponentInParent<PlayerContainer>().GetPlayerManager().GetComponent<PlayerController>().ReadyForFiring(true);
        else if (animator.gameObject.GetComponentInParent<AIController>())
          animator.gameObject.GetComponentInParent<AIController>().ReadyForFiring(true);

        fired = true;
      }
      else {
        if (animator.gameObject.GetComponentInParent<PlayerContainer>())
          animator.gameObject.GetComponentInParent<PlayerContainer>().GetPlayerManager().GetComponent<PlayerController>().ReadyForFiring(false);
        else if (animator.gameObject.GetComponentInParent<AIController>())
          animator.gameObject.GetComponentInParent<AIController>().ReadyForFiring(false);

        fired = false;
      }
    }
}
