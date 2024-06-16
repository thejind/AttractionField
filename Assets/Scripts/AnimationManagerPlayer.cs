using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AnimationManagerPlayer : MonoBehaviour
{
    public Animator animatorRef;
    public GameObject positiveAnim;
    public GameObject negativeAnim;

    // Start is called before the first frame update
    Coroutine activateAnim;
    [SerializeField] float animTime;

    public void setPlayerSpeed(float speed)
    {
        animatorRef.SetFloat("Speed",Math.Abs(speed));
    }

    public void polarityAnim(EPolarity polarity)
    {
        if(polarity != EPolarity.Neutral)
        {
            if(polarity == EPolarity.Positive)
            {
                if(negativeAnim.activeSelf)
                {
                    negativeAnim.SetActive(false);
                    StopCoroutine(activateAnim);
                }
                

                positiveAnim.SetActive(true);
                activateAnim = StartCoroutine(activeAnim(animTime,polarity));



            }
            else
            {
                if(positiveAnim.activeSelf)
                {
                    positiveAnim.SetActive(false);
                    StopCoroutine(activateAnim);
                }

                negativeAnim.SetActive(true);
                activateAnim = StartCoroutine(activeAnim(animTime, polarity));
            }
        }
    }

    IEnumerator activeAnim(float time, EPolarity polarity)
    {
        

        yield return new WaitForSeconds(time);
        if(polarity == EPolarity.Positive)
        positiveAnim.SetActive(false);
        else
        negativeAnim.SetActive(false);
    }
}
