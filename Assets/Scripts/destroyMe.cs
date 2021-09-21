using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyMe : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            Debug.Log("Collide");
            GameObject.Destroy(transform.gameObject);

        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("TRIGGERED");
        GameObject.Destroy(transform.gameObject);
    }
}
