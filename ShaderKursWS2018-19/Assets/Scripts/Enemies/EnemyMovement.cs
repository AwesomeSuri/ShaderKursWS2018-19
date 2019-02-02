using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { Swordsman, Hunter, Bat }

[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EnemyAnimation))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class EnemyMovement : MonoBehaviour
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    [SerializeField]
    [Tooltip("GameObject of the Swordsman mesh.")]
    GameObject swordsman;
    [SerializeField]
    [Tooltip("GameObject of the Hunter mesh.")]
    GameObject hunter;
    [SerializeField]
    [Tooltip("GameObject of the Bat mesh.")]
    GameObject bat;

    [Space]
    [SerializeField]
    [Tooltip("Speed of enemy when it is straying.")]
    float speed = 1;
    [SerializeField]
    [Tooltip("Speed of enemy when it is following target.")]
    float followSpeed = 2;
    [SerializeField]
    [Tooltip("The speed how fast the enemy turns.")]
    float turnSpeed = 10;
    [SerializeField]
    [Tooltip("Max time of pauses before going to next point.")]
    float maxPauseTime = 5;

    [Space]
    [SerializeField]
    [Tooltip("Trigger that activates player detection.")]
    Collider targetTrigger;
    [SerializeField]
    [Tooltip("Pool of collectibles drops an item when enemy is dead.")]
    CollectibleSpawner collectible;

    EnemyAnimation anim;                        // component that controlls its animation
    EnemyStats stats;                           // component that stores its stats
    Rigidbody rigid;                            // rigidbody component of this object
    Vector3 moveDirection;                      // direction the rigidbody should move to
    Collider col;                               // collider component of this object
    EnemyType enemyType;                        // type of the enemy (sword, bow, bat)
    Vector3 room;                               // the room position where the enemy currently is 
    Transform target;                           // tranform of focused object, when close enough
    IEnemyToPlayer player;                      // player interface to determine if player is active
    Vector3 toPos;                              // the position the enemy wants to go to in world pos
    float timer;                                // timer for the pause

    public int Lifetime { get; private set; }   // despawns when lifetime is zero, decreases when leaving the room


    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    private void Awake()
    {
        anim = GetComponent<EnemyAnimation>();
        stats = GetComponent<EnemyStats>();
        rigid = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    // Called when entering the room.
    public void Spawn(EnemySpawn spawn, RoomCoordinate room)
    {
        // set position
        this.room = new Vector3(room.x * 10, 0, room.y * 10);
        transform.position = spawn.Position;

        // set type
        this.enemyType = spawn.type;
        switch (spawn.type)
        {
            case EnemyType.Swordsman:
                swordsman.SetActive(true);
                break;
            case EnemyType.Hunter:
                hunter.SetActive(true);
                break;
            case EnemyType.Bat:
                bat.SetActive(true);
                break;
        }

        // set collision and trigger
        col.enabled = true;
        targetTrigger.enabled = true;
        (targetTrigger as SphereCollider).radius = (spawn.type == EnemyType.Hunter) ? 4 : 2.5f;

        // set stats
        Lifetime = 2;
        stats.SetStats(spawn.type);

        // set anim
        anim.SetAnimation(spawn.type);

        // start moving
        if (spawn.type == EnemyType.Bat)
        {
            maxPauseTime = 0;
        }
        SetDirection(false);
    }

    // Update is called once per frame
    void Update()
    {
        // check if active
        if (Lifetime <= 0)
        {
            return;
        }

        // check health
        if (stats.Health <= 0)
        {
            StartCoroutine(Death());

            return;
        }

        // check target
        if (target == null)
        {
            // set moving point after taking a break
            if (timer < 0)
            {
                SetDirection(false);

                // set next pause time
                timer = Random.Range(0, maxPauseTime);
            }
            else
            {
                // enemy is moving
                // take a break if moving point is reached
                if (Vector3.Distance(transform.position, toPos) < .3f)
                {
                    toPos = transform.position;

                    // taking a break
                    timer -= Time.deltaTime;
                }
            }
        }
        else
        {
            SetDirection(true);
        }
    }

    void FixedUpdate()
    {
        // check if active
        if (Lifetime <= 0)
        {
            return;
        }

        // check health
        if (stats.Health <= 0)
        {
            return;
        }

        UpdateMovement();
    }

    void LateUpdate()
    {
        // check if active
        if (Lifetime <= 0)
        {
            return;
        }

        // check health
        if (stats.Health <= 0)
        {
            return;
        }

        UpdateAnimation();
    }

    // Calculates AI for moving.
    // Moves to random position if target is null.
    void SetDirection(bool hasTarget)
    {
        // if target is null, choose a random moving point
        if (!hasTarget)
        {
            float x = Random.Range(-3.5f, 3.5f);
            float y = Random.Range(-3.5f, 3.5f);
            toPos = room + new Vector3(x, 0, y);
        }
        else if (Vector3.Distance(transform.position, target.position) < .5f)
        {
            // stop moving to attack if swordsman
            if (enemyType == EnemyType.Swordsman && anim.StartAttack())
            {
                toPos = transform.position;
            }
            else
            {
                // follow target
                toPos = target.position;
            }
        }
        else if (Vector3.Distance(transform.position, target.position) > (targetTrigger as SphereCollider).radius)
        {
            // lose target
            // hunters lose targets only after shooting
            if(enemyType != EnemyType.Hunter)
            {
                target = null;
                toPos = transform.position;
            }
            else
            {
                // follow target
                toPos = target.position;
            }
        }
        else
        {
            // follow target
            toPos = target.position;
        }
    }

    // Execute the movement using the stored variables.
    void UpdateMovement()
    {
        // move the enemy
        moveDirection = toPos - transform.position;
        if (moveDirection.magnitude > .1f)
        {
            // check if being pushed
            if (stats.InvincibleTimer > 0)
            {
                rigid.AddForce(moveDirection.normalized
                    * ((target == null) ? speed : followSpeed)
                    * Time.fixedDeltaTime);
            }
            // check if hunter has target
            else if (enemyType == EnemyType.Hunter && target != null)
            {
                rigid.velocity = Vector3.zero;
            }
            else
            {
                rigid.velocity = Vector3.zero;

                rigid.MovePosition(transform.position
                    + moveDirection.normalized
                    * ((target == null) ? speed : followSpeed)
                    * Time.fixedDeltaTime);
            }


            // turn the enemy
            rigid.MoveRotation(Quaternion.Lerp(
                rigid.rotation,
                Quaternion.LookRotation(moveDirection.normalized),
                turnSpeed * Time.fixedDeltaTime));
        }

        // clamp enemy position inside the room
        float x = Mathf.Clamp(transform.position.x, room.x - 3.5f, room.x + 3.5f);
        float y = Mathf.Clamp(transform.position.z, room.z - 3.5f, room.z + 3.5f);
        transform.position = new Vector3(x, 0, y);
    }

    // Apply movement values into the animation
    void UpdateAnimation()
    {
        // check if walking
        if (moveDirection.magnitude > .1f)
        {
            // get local look direction
            Vector3 animDirection = (transform.InverseTransformDirection(moveDirection)).normalized;

            // check running
            animDirection *= target != null ? 2 : 1;

            // apply values
            anim.UpdateMovement(animDirection.x, animDirection.z);
        }
        else
        {
            anim.UpdateMovement(0, 0);
        }
    }

    // called when leaving the room
    public void DecreaseLifetime()
    {
        Lifetime--;

        if (Lifetime <= 0)
        {
            DisableEnemy();
        }
    }

    // Called when health is zero.
    // Deactivates enemy.
    IEnumerator Death()
    {
        // TODO: play death anim

        yield return new WaitForSeconds(.5f);

        // drop item
        CollectibleType collectibleType = CollectibleType.Heart;
        if (enemyType == EnemyType.Hunter)
        {
            collectibleType = CollectibleType.Arrow;
        }
        collectible.Drop(transform.position, collectibleType);

        DisableEnemy();
    }

    // Called when dead or lifetime is zero
    public void DisableEnemy()
    {
        target = null;

        // deactivate mesh
        col.enabled = false;
        targetTrigger.enabled = false;
        swordsman.SetActive(false);
        hunter.SetActive(false);
        bat.SetActive(false);
    }

    // Detecting the player.
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (player == null)
            {
                player = other.GetComponent<IEnemyToPlayer>();
            }

            if (player.PlayerActive && target == null)
            {
                target = other.transform;

                if(enemyType == EnemyType.Hunter && !anim.IsAttacking)
                {
                    StartCoroutine(AimShoot());
                }
            }
            else if(enemyType != EnemyType.Hunter)
            {
                target = null;
            }
        }
    }

    // Getting hit.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerWeapon")
            && stats.InvincibleTimer <= 0)
        {
            // push back
            rigid.AddForce(other.transform.forward * 1000);

            stats.GetHit();
        }
    }

    // Aim and shoot
    IEnumerator AimShoot()
    {
        yield return anim.AimingAndShooting();

        target = null;
        SetDirection(false);
    }
}