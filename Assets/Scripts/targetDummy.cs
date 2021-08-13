using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class targetDummy : MonoBehaviour
{
    // Start is called before the first frame update
    int health = 3;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage()
    {
        health -= 1;
        if (health < 0)
        {
            GameObject.Destroy(transform.gameObject);
        }

    }
}
