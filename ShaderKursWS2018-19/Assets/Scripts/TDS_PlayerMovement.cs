using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RoomCoordinate
{
    public int x;
    public int y;
}

public interface IGameManagerToPlayerMovement
{
    void SetInput(Vector3 moveDirection, Vector3 lookDirection, bool running);
}

public interface IUIToPlayerMovement
{
    void SetWings(bool wings);
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerStats))]
public class TDS_PlayerMovement : MonoBehaviour, IGameManagerToPlayerMovement, IUIToPlayerMovement
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
    [Tooltip("The speed how fast the player walks.")]
    float walkSpeedCutscene = 2.5f;
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
    [SerializeField]
    [Tooltip("Component of the MainCamera.")]
    CameraController camObject;
    [SerializeField]
    [Tooltip("Component of the Maze.")]
    MazeController mazeObject;
    [SerializeField]
    [Tooltip("Controller of the UI.")]
    LevelUIController levelUIObject;


    IPlayerMovementToCamera cam;                        // interface between this script and the camera
    IPlayerToMaze maze;                                 // interface between this script and the maze
    IPlayerMovementToUI levelUI;                        // interface between this script and the ui

    Rigidbody rigid;                                    // rigidbody directs all the movements
    PlayerStats stats;                                  // stores all the stats
    Vector3 moveDirection;                              // direction the rigidbody should move to
    Vector3 lookDirection;                              // direction the rigidbody should face to
    bool running;                                       // if true, player moves at runSpeed
    bool lookAtMouse;                                   // true, if some mouse action is active (eg. shooting)
    RoomCoordinate room;                                // stores the coordinate of the current room


    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    // Use this for initialization
    void Awake()
    {
        cam = camObject;
        camObject = null;
        maze = mazeObject;
        mazeObject = null;
        levelUI = levelUIObject;
        levelUIObject = null;

        rigid = GetComponent<Rigidbody>();
        stats = GetComponent<PlayerStats>();
        moveDirection = Vector3.zero;
        lookDirection = Vector3.forward;
        running = false;
        room.x = 0;
        room.y = 0;
        lookAtMouse = false;
    }

    void Start()
    {
        levelUI.EnterRoom(room.x, room.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (stats.PlayerActive)
        {
            // get user's input if able
            // otherwise it's probably during a cutscene
            GetInput();
        }

        // check falling
        if(transform.position.y < -.1f)
        {
            Death();
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
        rigid.MovePosition(transform.position
            + moveDirection.normalized
            * (stats.PlayerActive ? (running ? runSpeed : walkSpeed) : walkSpeedCutscene)
            * Time.fixedDeltaTime);

        // turn the player
        rigid.MoveRotation(Quaternion.Lerp(
            rigid.rotation,
            Quaternion.LookRotation(lookDirection.normalized),
            turnSpeed * Time.fixedDeltaTime));
    }

    // Apply movement values into the animation
    void UpdateAnimation()
    {
        // check if walking
        if (moveDirection.magnitude > 0)
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

    // Called when player y position is below 0.
    // Death animation plays.
    // Respawn.
    void Death()
    {
        stats.PlayerActive = false;

        // TODO: add spawn anim
        maze.DeactivateRoom(room);
        room.x = 0;
        room.y = 0;
        maze.ActivateRoom(room);
        cam.JumpCamera(room);
        transform.position = Vector3.zero;

        // enable moving
        stats.PlayerActive = true;
        stats.SetRoomTransfering(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!stats.PlayerActive || stats.RoomTranfering)
        {
            return;
        }

        if (other.tag == "WallTransfer")
        {
            StartCoroutine(TransferingRoom(other.transform));
        }

        if (other.tag == "Collectible")
        {
            PickUp(other.transform.GetComponent<IPlayerToCollectible>().PickUp());
        }
    }

    // Start room transfer animation
    IEnumerator TransferingRoom(Transform transferTrigger)
    {
        // deactivate all movements
        stats.PlayerActive = false;
        lookAtMouse = false;
        running = false;
        stats.SetRoomTransfering(true);

        // check transfer direction
        // update room coordinate
        RoomCoordinate roomOld = room;
        Vector3 roomPosition = new Vector3(10 * room.x, 0, 10 * room.y);
        Vector3 transferDirection = (transferTrigger.position - roomPosition).normalized;
        if (transferDirection.x > .5f)
        {
            transferDirection = Vector3.right;
            room.x++;
        }
        else if (transferDirection.x < -.5f)
        {
            transferDirection = Vector3.left;
            room.x--;
        }
        else if (transferDirection.z > .5f)
        {
            transferDirection = Vector3.forward;
            room.y++;
        }
        else if (transferDirection.z < -.5f)
        {
            transferDirection = Vector3.back;
            room.y--;
        }
        Vector3 toPos = roomPosition
            + 5 * Vector3.right * transferDirection.x
            + 5 * Vector3.forward * transferDirection.z;

        // activate next room
        maze.ActivateRoom(room);
        levelUI.EnterRoom(room.x, room.y);

        // move player
        yield return MovePlayer(toPos);

        // move shader
        // TODO

        // move camera
        cam.MoveCamera(room);
        yield return new WaitWhile(() => cam.IsMoving);

        // move shader
        // TODO

        // move player
        toPos += 1.5f * Vector3.right * transferDirection.x
            + 1.5f * Vector3.forward * transferDirection.z;
        yield return MovePlayer(toPos);

        // deactivate previous room
        maze.DeactivateRoom(roomOld);

        // check instant death
        RoomType type = maze.GetRoomType(room);
        if (type == RoomType.Standard
            || type == RoomType.Lava && stats.CurrentEquipment == Equipment.Barrier
            || type == RoomType.Pit && stats.CurrentEquipment == Equipment.Wings
            || type == RoomType.Wumpus && stats.WumpusSlayer)
        {
            // enable moving
            stats.PlayerActive = true;
            stats.SetRoomTransfering(false);
            
            // TODO: add wumpus event

        }
        else
        {
            // TODO: add death anim

            // respawn at start
            Death();
        }
    }

    // Automatically moves the player to desired position
    IEnumerator MovePlayer(Vector3 toPos)
    {
        moveDirection = (toPos - transform.position).normalized;
        lookDirection = moveDirection;
        yield return new WaitWhile(() => Vector3.Distance(transform.position, toPos) > .1f);
        moveDirection = Vector3.zero;
    }

    // When a collectible is touched.
    // Change stats.
    // Disable collectible.
    void PickUp(CollectibleType type)
    {
        stats.PickUp(type);
    }

    public void SetWings(bool wings)
    {
        anim.SetFlying(wings);
        rigid.constraints = wings ? RigidbodyConstraints.FreezePositionY : RigidbodyConstraints.None;
    }
}
