using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float horizontalInput;
    public float verticalInput;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Read the Input from the keyboeard
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Move the player
        transform.Translate(Vector3.right * horizontalInput * Time.deltaTime * 10);
        transform.Translate(Vector3.forward * verticalInput * Time.deltaTime * 10);

        // if the position of the cube in y direction is less than -2.5, then display then print the Game over message and reset the position of the cube to 0,0.5,0
        if(transform.position.y < -2.5)
        {
            Debug.Log("Game Over!");
            transform.position = new Vector3(0, 0.5f, 0);
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        

        
    }
}
