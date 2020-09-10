using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAnimation : MonoBehaviour
{
    Animator charAnim;
    private Joystick js;
    private bool runState = false;
    // Start is called before the first frame update
    void Start()
    {
        charAnim = GetComponent<Animator>();
        js = FindObjectOfType<Joystick>();
    }

    // Update is called once per frame
    void Update()
    {
        float mvY = js.Horizontal;
        float mvX = js.Vertical;

        if (mvX != 0 || mvY != 0)
        {
            if (runState == false)
            {
                charAnim.SetTrigger("run");
                runState = true;
                //Debug.Log("Set to walk");  
            }
        }
        else
        {
            if (runState == true)
            {
                charAnim.SetTrigger("idle");
                runState = false;
                //Debug.Log("Set to stop");
            }
        }
    }
}
