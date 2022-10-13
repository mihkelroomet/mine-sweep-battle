using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;


    private Vector3 mousePos;
    private Camera camera;
    private LineRenderer lineRend;
    private float timer;
    private bool defusing;


    // Player
    float walkSpeed = 4f;
    private float speedLimiter = 0.7f;
    private float inputHorizontal;
    private float inputVertical;

    // Animations and states
    private Animator animator;
    private string currentState;
    private const string PLAYER_IDLE = "Player_Idle";
    private const string PLAYER_MOVE_L = "Player_Move_L";
    private const string PLAYER_MOVE_R = "Player_Move_R";
    private const string PLAYER_MOVE_U = "Player_Move_U";
    private const string PLAYER_MOVE_D = "Player_Move_D";

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        
        lineRend = gameObject.GetComponent<LineRenderer>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }


    void Update()
    {
        mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");


        // Shooting the laserbeam
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            defusing = false; // Breaks defusing process
            lineRend.startWidth = 0.1f;
            lineRend.startColor = Color.red;
            lineRend.positionCount = 2;
            lineRend.SetPosition(0, gameObject.transform.position);
            lineRend.SetPosition(1, mousePos);
            timer = 0;
        }


        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            defusing = true;
            lineRend.startWidth = 0.2f;
            lineRend.startColor = Color.green;
            lineRend.positionCount = 2;
            lineRend.SetPosition(0, gameObject.transform.position);
            lineRend.SetPosition(1, mousePos);
            timer = 0;
        }

        // Make the beam disappear
        if (lineRend.positionCount > 0)
        {
            timer += Time.deltaTime;
            if (defusing)
            {
                if (timer > 3)
                {
                    lineRend.positionCount = 0;
                }
            }
            else
            {
                if (timer > 0.1)
                {
                    lineRend.positionCount = 0;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (inputHorizontal != 0 || inputVertical != 0)
        {
            // if player tries to move during defusing, break the process
            if (defusing)
            {
                defusing = false;
                lineRend.positionCount = 0;
            }
            
            // To avoid player getting faster when moving diagonally
            if (inputHorizontal != 0 && inputVertical != 0)
            {
                inputHorizontal *= speedLimiter;
                inputVertical *= speedLimiter;
            }
            rb.velocity = new Vector2(inputHorizontal * walkSpeed, inputVertical * walkSpeed);

            // Update player animations
            if (inputVertical < 0)
            {
                ChangeAnimationState(PLAYER_MOVE_D);
            }
            else if (inputVertical > 0)
            {
                ChangeAnimationState(PLAYER_MOVE_U);
            }
            else if (inputHorizontal > 0)
            {
                ChangeAnimationState(PLAYER_MOVE_R);
            }
            else if (inputHorizontal < 0)
            {
                ChangeAnimationState(PLAYER_MOVE_L);
            }

        }
        else
        {
            rb.velocity = new Vector2(0f, 0f);
            ChangeAnimationState(PLAYER_IDLE);
        }
    }

    // Animation state changer
    void ChangeAnimationState(string newState)
    {
        // Stop animation from interrupting self
        if (currentState == newState) return;

        //Play new animation
        animator.Play(newState);

        // Update current state
        currentState = newState;
    }
}
