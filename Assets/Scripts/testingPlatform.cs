using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testingPlatform : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject dummy;
    public GameObject topRight;
    public GameObject bottomLeft;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp("g"))
        {
            spawnDummy();
        }
    }


    void spawnDummy()
    {
        float xpos = Random.Range(bottomLeft.transform.position.x, topRight.transform.position.x);
        float zpos = Random.Range(bottomLeft.transform.position.z, topRight.transform.position.z);
        GameObject.Instantiate(dummy, new Vector3(xpos, 3.0f, zpos), Quaternion.Euler(0f,180f,0f));
        
    }
}
