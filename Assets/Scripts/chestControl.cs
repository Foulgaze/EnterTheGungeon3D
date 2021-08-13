using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chestControl : MonoBehaviour
{
    // Start is called before the first frame update
    Animator animator;
    void Start()
    {
        animator = transform.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp("s"))
        {
            animator.Play("greenchestOpening");
        }
    }
}
