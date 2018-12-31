using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TDS_PlayerMovement : MonoBehaviour
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    [Header("Input Settings")]
    [SerializeField]
    [Tooltip("Name of the axis with wich the player can move sideways.")]
    string horizontalAxis = "Horizontal";
    [SerializeField]
    [Tooltip("Name of the axis with wich the player can move straight.")]
    string verticalAxis = "Vertical";
    [SerializeField]
    [Tooltip("Name of the button with wich the player can toggle running.")]
    string runButton = "Fire2";
    [Space]
    [Header("Move Settings")]
    [SerializeField]
    [Tooltip("The speed how fast the player walks.")]
    float walkSpeed = 5;
    [SerializeField]
    [Tooltip("The speed how fast the player runs.")]
    float runSpeed = 10;
    [SerializeField]
    [Tooltip("The speed how fast the player turns.")]
    float turnSpeed = 10;
    [SerializeField]
    [Tooltip("Layer that detects the ray casted from mouse position.")]
    LayerMask floorMask;

    [Space]
    [SerializeField]
    [Tooltip("PlayerAnimator component of the mesh.")]
    PlayerAnimation anim;

    Rigidbody rigid;                        // rigidbody directs all the movements
    Vector3 moveDirection;                  // direction the rigidbody should move to
    Vector3 lookDirection;                  // direction the rigidbody should face to
    bool running;                           // if true, player moves at runSpeed
    bool lookAtMouse;                       // true, if some mouse action is active (eg. shooting)

    public bool PlayerActive { get; set; }  // true if player can controll this object


    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    // Use this for initialization
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        moveDirection = Vector3.zero;
        lookDirection = Vector3.forward;
        running = false;
        lookAtMouse = false;
        PlayerActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerActive)
        {
            // get user's input if able
            // otherwise it's probably during a cutscene
            GetInput();
        }
    }

    void FixedUpdate()
    {
        UpdateMovement();
    }

    void LateUpdate()
    {
        UpdateAnimation();
    }

    // Update the input.
    void GetInput()
    {
        // get WASD input for moveDirection
        float horizontal = Input.GetAxis(horizontalAxis);
        float vertical = Input.GetAxis(verticalAxis);
        moveDirection = new Vector3(horizontal, 0, vertical);

        // get mouse input for lookDirection
        if (lookAtMouse)
        {
            // cast a ray from mouse to the floor
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit floorHit;
            if (Physics.Raycast(camRay, out floorHit, 100, floorMask))
            {
                lookDirection = floorHit.point - transform.position;
                lookDirection.y = 0f;
            }
        }
        else if (moveDirection.magnitude > .2f)
        {
            // turn to moveDirection otherwise
            // only if moveDirection has a value
            lookDirection = moveDirection;
        }

        // get mouse input for running
        running = Input.GetButton(runButton);
    }

    // Externally set the player movements.
    // For cutscenes for example.
    public void SetInput(Vector3 moveDirection, Vector3 lookDirection, bool running)
    {
        this.moveDirection = moveDirection;
        this.lookDirection = lookDirection;
        this.running = running;
    }

    // Execute the movement using the stored variables.
    void UpdateMovement()
    {
        // move the player
        rigid.MovePosition(transform.position + moveDirection * (running ? runSpeed : walkSpeed) * Time.fixedDeltaTime);

        // turn the player
        rigid.MoveRotation(Quaternion.Lerp(rigid.rotation, Quaternion.LookRotation(lookDirection), turnSpeed * Time.fixedDeltaTime));
    }

    // Apply movement values into the animation
    void UpdateAnimation()
    {
        // check if walking
        if(moveDirection.magnitude > 0)
        {
            // get local look direction
            Vector3 animDirection = (transform.InverseTransformDirection(moveDirection)).normalized;

            // check running
            animDirection *= running ? 2 : 1;

            // apply values
            anim.UpdateMovement(animDirection.x, animDirection.z);
        }
        else
        {
            anim.UpdateMovement(0, 0);
        }
    }
}
