using UnityEngine;
using TMPro;
using System.Collections;

public class RobotDialogue : MonoBehaviour
{
    public GameObject bubbleBackground;
    public TMP_Text dialogueText;       
    public float typingSpeed = 0.025f;
    public Animator robotAnimator;

    void Start()
    {
        bubbleBackground.SetActive(false);
    }

    public void Say(string message)
    {
        StopAllCoroutines(); 
        StartCoroutine(TypeMessage(message));
    }

    IEnumerator TypeMessage(string message)
    {
        bubbleBackground.SetActive(true);
        dialogueText.text = "";
        if (robotAnimator != null)
        {
            robotAnimator.SetBool("isTalking", true);
        }

        foreach (char letter in message.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(2f);
        bubbleBackground.SetActive(false);
        if (robotAnimator != null)
        {
            robotAnimator.SetBool("isTalking", false);
        }
    }
}