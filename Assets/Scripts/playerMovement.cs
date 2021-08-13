using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public int moveSpeed = 5;
    float multiplier = 1.0f;
    float gravity = 0.1f;
    float currGravity = 0.0f;
    bool isMoving = true;
    // Start is called before the first frame update
    CharacterController cont;
    public Camera cam;
    public float degree;
    void Start()
    {
        cont = transform.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {

        cam.transform.position = new Vector3(transform.position.x + (Input.mousePosition.x - (Screen.width / 2.0f))/(degree*1.5f), cam.transform.position.y, transform.position.z + (Input.mousePosition.y - (Screen.height / 2.0f))/degree);
        if (cont.isGrounded)
        {
            currGravity = 0.0f;
        }
        else
        {
            currGravity += gravity;
        }

        if (isMoving)
        {
            float HorizontalMove = Input.GetAxisRaw("Horizontal") * Time.deltaTime * moveSpeed;
            float VerticalMove = Input.GetAxisRaw("Vertical") * Time.deltaTime * moveSpeed;
            cont.Move(new Vector3(HorizontalMove, 0, VerticalMove));
        }

        cont.Move(new Vector3(0, -1 * currGravity, 0));
    }


    void updateMultiplier(float multi)
    {
        multiplier += multi; 
    }
}

