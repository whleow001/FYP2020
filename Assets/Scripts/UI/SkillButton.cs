using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class SkillButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{

    [HideInInspector]
    public bool pressed;

    [SerializeField]
    private bool hasCooldown;
    private int cooldown = 5;
    private int skillCooldown = 10;
    private int cooldownTime;
    private int skillCooldownTime;
    private Coroutine dodgeCoroutine;
    private Coroutine skillOneCoroutine;
    private bool enabled = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator CooldownTimer()
    {
        gameObject.transform.GetChild(0).GetChild(0).gameObject.SetActive(cooldownTime > 0);
        gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = cooldownTime.ToString();

        cooldownTime--;

        if (cooldownTime < 0)
        {
            dodgeCoroutine = null;
            enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
            dodgeCoroutine = StartCoroutine(CooldownTimer());
        }

    }

    private IEnumerator SkillCooldownTimer()
    {
        gameObject.transform.GetChild(3).gameObject.SetActive(skillCooldownTime > 0);
        gameObject.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = skillCooldownTime.ToString();

        skillCooldownTime--;

        if (skillCooldownTime < 0)
        {
            skillOneCoroutine = null;
            enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
            skillOneCoroutine = StartCoroutine(SkillCooldownTimer());
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (enabled)
        {
            pressed = true;
           
                Debug.Log(EventSystem.current.currentSelectedGameObject.name);
            
            if (EventSystem.current.currentSelectedGameObject.name == "dodge")
            {
                if (hasCooldown)
                {
                    enabled = false;
                    cooldownTime = cooldown;
                    dodgeCoroutine = StartCoroutine(CooldownTimer());
                }
            }

            if (EventSystem.current.currentSelectedGameObject.name == "skill1")
            {
                if (hasCooldown)
                {
                    enabled = false;
                    skillCooldownTime = skillCooldown;
                    skillOneCoroutine = StartCoroutine(SkillCooldownTimer());
                }
            }

            //Debug.Log(EventSystem.current.currentSelectedGameObject.name);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
    }
}
