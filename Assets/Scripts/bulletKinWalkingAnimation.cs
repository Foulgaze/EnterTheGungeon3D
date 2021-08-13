using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletKinWalkingAnimation : MonoBehaviour
{
    public float animationTime = 0.5f;
    public GameObject animationFrame;
    float animationCounter = 0;
    int frame = 0;
    int frameCap;
    // Start is called before the first frame update
    void Start()
    {
        animationFrame = transform.Find("Animation").gameObject;
        animationCounter = animationTime;
        frameCap = animationFrame.transform.childCount;
        for(int i = 0; i < frameCap; i++)
        {
            if(i != 0)
            {
                animationFrame.transform.GetChild(i).gameObject.SetActive(false);

            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(animationCounter <= 0)
        {
            animationFrame.transform.GetChild(frame).gameObject.SetActive(false);
            frame = frame + 1 < frameCap ? frame + 1 : 0;
            animationFrame.transform.GetChild(frame).gameObject.SetActive(true);
            animationCounter = animationTime;
        }
        else
        {
            animationCounter -= Time.deltaTime;
        }
        
    }
}
