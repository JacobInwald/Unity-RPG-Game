using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum state { 
    walk,
    interact,
    run,
}
public class playerMovement : MonoBehaviour
{
    // Sets the inputs to this script
    private Rigidbody2D rigidBody;
    private Vector2 movementVector;
    public float moveSpeed = 5/64; // this is not pixel by pixel but instead unit by unit according to Unity's unit system
    public state state;

    // Initialising the double tap variables
    private const float timeSeparation = 0.1f;
    private const float sprintMultiplier = 1.2f;
    private bool moving, nextTapWillRun = false;
    private float timeSinceLastPress;


    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        state = state.walk;
    }

    // Update is called once per frame
    void Update()
    {
        // gets inputs in both directions
        movementVector.x = Input.GetAxisRaw("Horizontal");
        movementVector.y = Input.GetAxisRaw("Vertical");


        // checks for double tap inputs, to check if the player is sprinting or not, 
        // placed into a check for interact state so accidental state chages are not possible
        if (state != state.interact)
        {
            CheckAndSetSprint();
        }

        // does the movement, based on the state that the player is in
        move();
    }

    void CheckAndSetSprint()
    {
        if (moving && movementVector.x == 0 && movementVector.y == 0)
        {
            moving = false;
            if (Time.time > timeSinceLastPress + timeSeparation)
            {
                nextTapWillRun = false;
                state = state.walk;
            }
        }
        else if (!moving && !(movementVector.x == 0 && movementVector.y == 0))
        {
            moving = true;
            if (nextTapWillRun)
            {
                state = state.run;
                nextTapWillRun = false;
            }
            else
            {
                nextTapWillRun = true;
                timeSinceLastPress = Time.time;
            }
        }
    }

    void move()
    {
        if (state == state.walk)
        {
            rigidBody.MovePosition(rigidBody.position + movementVector.normalized * moveSpeed);
        }
        else if (state == state.run)
        {
            rigidBody.MovePosition(rigidBody.position + movementVector.normalized * moveSpeed * sprintMultiplier);
        }
    }

}
