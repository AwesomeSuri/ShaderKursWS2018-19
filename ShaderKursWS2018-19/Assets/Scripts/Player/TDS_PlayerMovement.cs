using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField]
    [Tooltip("The component of GamManager that controls the global dissolve.")]
    GlobalDissolveToBlackController dissolve;

    [Space]
    [SerializeField]
    [Tooltip("Particle System that is played when player gets a normal hit.")]
    ParticleSystem hit;
    [SerializeField]
    [Tooltip("Particle System that is played when player gets a critical hit.")]
    ParticleSystem lightning;

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
    [SerializeField]
    [Tooltip("Parent of the player arrow pool.")]
    ArrowSpawner arrow;
    [SerializeField]
    [Tooltip("Transform where the arrows shoot from.")]
    Transform arrowOrigin;


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
    GrassController grass;                              // component that moves the grass near the player
    bool arrowLoaded;                                   // true if aiming with an arrow

    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    #region Init
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
        grass = GetComponent<GrassController>();
    }

    void Start()
    {
        levelUI.EnterRoom(room.x, room.y);
    }

    // Called when player y position is below 0.
    // Death animation plays.
    // Respawn.
    IEnumerator DeathAndSpawn()
    {
        stats.PlayerActive = false;

        anim.Die();

        yield return new WaitForSeconds(2);
        print("start");
        // TODO: add spawn anim
        yield return dissolve.DissolveAll();

        yield return new WaitForSeconds(.5f);

        print("end");
        maze.DeactivateRoom(room);
        room.x = 0;
        room.y = 0;

        maze.ActivateRoom(room);
        cam.JumpCamera(room);
        transform.position = Vector3.zero;

        // reset stats
        stats.ResetStats();

        // reset anim
        anim.ResetAnim();

        // set grass offset
        grass.SetOffset(room);

        yield return dissolve.JumpIntoRoom(room);

        // enable moving
        stats.PlayerActive = true;
        stats.SetRoomTransfering(false);
    }
    #endregion

    #region Update
    // Update is called once per frame
    void Update()
    {
        if (stats.PlayerActive && Time.time > stats.InvincibleTimer)
        {
            if (Time.time > stats.InvincibleTimer)
            {
                // get user's input if able
                // otherwise it's probably during a cutscene
                GetInput();

                // check falling
                if (transform.position.y < -.1f)
                {
                    stats.GetHit();
                    stats.GetHit();
                    stats.GetHit();
                }
            }

            if (stats.Health <= 0)
            {
                StartCoroutine(DeathAndSpawn());
            }
        }
    }

    void FixedUpdate()
    {
        UpdateMovement();
    }

    void LateUpdate()
    {
        UpdateAnimation();

        Shader.SetGlobalVector("_PlayerPos", transform.position);
    }

    void OnValidate()
    {
        Shader.SetGlobalVector("_PlayerPos", transform.position);
    }
    #endregion

    #region Input
    // Update the input.
    void GetInput()
    {
        // get WASD input for moveDirection
        float horizontal = Input.GetAxisRaw(horizontalAxis);
        float vertical = Input.GetAxisRaw(verticalAxis);
        moveDirection = new Vector3(horizontal, 0, vertical);

        if (moveDirection.magnitude < .1f)
        {
            moveDirection = Vector3.zero;
        }
        else
        {
            moveDirection = moveDirection.normalized;
        }

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
        else if (moveDirection.magnitude > .5f)
        {
            // turn to moveDirection otherwise
            // only if moveDirection has a value
            lookDirection = moveDirection;
        }

        // get mouse input for running
        running = Input.GetButton(runButton);

        // get mouse input for attacking
        if (Input.GetButtonDown("Fire1")
            && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()
            && !anim.IsSwinging && anim.IsAiming <= 0)
        {
            Attack();
        }
        if (Input.GetButtonUp("Fire1") && anim.IsAiming > 0)
        {
            Shoot();
        }
    }

    // Externally set the player movements.
    // For cutscenes for example.
    public void SetInput(Vector3 moveDirection, Vector3 lookDirection, bool running)
    {
        this.moveDirection = moveDirection;
        this.lookDirection = lookDirection;
        this.running = running;
    }
    #endregion

    #region Walk
    // Execute the movement using the stored variables.
    void UpdateMovement()
    {
        // move the player
        transform.Translate(moveDirection.normalized
            * ((Time.time < stats.InvincibleTimer) ? 10 :
            (stats.PlayerActive ? (running ? runSpeed : walkSpeed) : walkSpeedCutscene))
            * Time.fixedDeltaTime,
            Space.World);

        if (Time.time > stats.InvincibleTimer)
        {
            // turn the player if not being pushed
            transform.rotation = (Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(lookDirection.normalized),
                turnSpeed * Time.fixedDeltaTime));
        }

        // clamp if being pushed
        if (stats.InvincibleTimer > Time.time)
        {
            Vector3 room = new Vector3(this.room.x, 0, this.room.y) * 10;
            float x = Mathf.Clamp(transform.position.x, room.x - 3.5f, room.x + 3.5f);
            float y = Mathf.Clamp(transform.position.z, room.z - 3.5f, room.z + 3.5f);
            transform.position = new Vector3(x, 0, y);
        }
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

    // Start room transfer animation
    IEnumerator TransferingRoom(Transform transferTrigger)
    {
        // deactivate all movements
        stats.PlayerActive = false;
        lookAtMouse = false;
        running = false;
        stats.SetRoomTransfering(true);
        if (anim.IsAiming > 0)
        {
            Shoot();
        }

        // check transfer direction
        // update room coordinate
        char dissolveDir = ' ';
        RoomCoordinate roomOld = room;
        Vector3 roomPosition = new Vector3(10 * room.x, 0, 10 * room.y);
        Vector3 transferDirection = (transferTrigger.position - roomPosition).normalized;
        if (transferDirection.x > .5f)
        {
            transferDirection = Vector3.right;
            room.x++;

            dissolveDir = 'r';
        }
        else if (transferDirection.x < -.5f)
        {
            transferDirection = Vector3.left;
            room.x--;

            dissolveDir = 'l';
        }
        else if (transferDirection.z > .5f)
        {
            transferDirection = Vector3.forward;
            room.y++;

            dissolveDir = 't';
        }
        else if (transferDirection.z < -.5f)
        {
            transferDirection = Vector3.back;
            room.y--;

            dissolveDir = 'b';
        }
        Vector3 toPos = roomPosition
            + 5 * Vector3.right * transferDirection.x
            + 5 * Vector3.forward * transferDirection.z;

        // move player
        yield return MovePlayer(toPos);

        // move shader
        yield return dissolve.CloseRoom(dissolveDir);

        // deactivate previous room
        maze.DeactivateRoom(roomOld);

        // move camera
        cam.MoveCamera(room);
        yield return new WaitWhile(() => cam.IsMoving);

        // activate next room
        maze.ActivateRoom(room);
        levelUI.EnterRoom(room.x, room.y);

        // move shader
        yield return dissolve.OpenRoom(dissolveDir);

        // set grass offset
        grass.SetOffset(room);

        // move player
        toPos += 1.5f * Vector3.right * transferDirection.x
            + 1.5f * Vector3.forward * transferDirection.z;
        yield return MovePlayer(toPos);

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
            yield return DeathAndSpawn();
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
    #endregion

    #region Attack
    void Attack()
    {
        if (stats.CurrentEquipment == Equipment.Sword)
        {
            StartCoroutine(Swinging());
        }

        if (stats.CurrentEquipment == Equipment.Bow)
        {
            Aim();
        }
    }

    IEnumerator Swinging()
    {
        lookAtMouse = true;

        yield return anim.Swinging();

        yield return new WaitForSeconds(.5f);

        if (!anim.IsSwinging)
        {
            lookAtMouse = false;
        }
    }

    void Aim()
    {
        // check if currently aiming
        if (anim.IsAiming <= -1)
        {
            anim.StartAiming(stats.ArrowAmount > 0);

            // check if arrow available
            if (stats.ArrowAmount > 0)
            {
                arrowLoaded = true;
            }

            lookAtMouse = true;
        }
    }

    void Shoot()
    {
        // shoot arrow if loaded
        if (arrowLoaded)
        {
            // TODO: add shoot sound

            arrow.Shoot(arrowOrigin, anim.IsAiming);

            arrowLoaded = false;

            stats.Shoot();
        }

        lookAtMouse = false;
        anim.StopAiming();
    }
    #endregion

    #region Item
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
    }
    #endregion

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

        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyWeapon")
            && Time.time > stats.InvincibleTimer)
        {
            stats.GetHit();

            if (stats.Health > 0)
            {
                moveDirection = (transform.position - other.transform.position).normalized * 10;
                moveDirection = new Vector3(moveDirection.x, 0, moveDirection.z);

                // hit effect
                Vector3 hitPos = (transform.position + other.transform.position) / 2;
                hit.transform.position = hitPos + Vector3.up * .5f;
                hit.Emit(1);
                hit.GetComponent<AudioSource>().Play();
            }
            else
            {
                moveDirection = Vector3.zero;
                StartCoroutine(DeathAndSpawn());

                // lightning effect
                lightning.transform.position = transform.position + Vector3.up * .5f;
                lightning.Emit(1);
                lightning.GetComponent<AudioSource>().Play();
            }
        }
    }
}
