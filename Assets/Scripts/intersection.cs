using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class intersection : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject bc;
    public GameObject sc;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
       
    }

    /*    private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("OOPS :( ");
            transform.root.name = "BADLeverSpawner";
        }
       

        private void OnTriggerEnter(Collider other)
        {

            transform.root.name = "BADLeverSpawner";

        }

    */
    private void OnTriggerStay(Collider other)
    {
        /*transform.root.name = "BADLeverSpawner";*/
        //Debug.Log(transform.position);

    }
    private void OnTriggerEnter(Collider other)
    {
  /*      Debug.Log("IM CUBING");
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = transform.position;
        cube.transform.localScale = new Vector3(1, 10, 1);*/
    }
    /*    private void OnCollisionStay(Collision collision)
        {
            Debug.Log("IM COLLIDING HERE");
        }*/
}