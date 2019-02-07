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
    [Tooltip("Min time of pauses before going to next point.")]
    float minPauseTime = 1;
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

    [Space]
    [SerializeField]
    [Tooltip("Particle System that is played when enemy gets a normal hit.")]
    ParticleSystem hit;
    [SerializeField]
    [Tooltip("Particle System that is played when enemy gets a critical hit.")]
    ParticleSystem lightning;
    [SerializeField]
    [Tooltip("Particle System that is played when enemy dies.")]
    ParticleSystem explosion;

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
                bat.GetComponent<Collider>().enabled = true;
                break;
        }

        // set collision and trigger
        col.enabled = true;
        targetTrigger.enabled = true;
        (targetTrigger as SphereCollider).radius = (spawn.type == EnemyType.Hunter) ? 4 : 2.5f;

        // set stats
        Lifetime = 1;
        stats.SetStats(spawn.type);

        // set anim
        anim.SetAnimation(spawn.type);

        // start moving
        if (spawn.type == EnemyType.Bat)
        {
            minPauseTime = 0;
            maxPauseTime = 0;
        }
        timer = -1;
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

        // let be pushed if hit
        if (stats.InvincibleTimer > Time.time)
        {
            return;
        }

        // do nothing if is attacking unless hunter
        if (anim.IsAttacking && enemyType != EnemyType.Hunter)
        {
            return;
        }

        // check target
        if (target == null)
        {
            // set moving point after taking a break
            // timer -1 => enemy is moving
            if (timer > 0 && Time.time > timer)
            {
                SetDirection(false);

                // set moving
                timer = -1;
            }
            else
            {
                // enemy is moving
                // take a break if moving point is reached
                if (Vector3.Distance(transform.position, toPos) < .5f && timer < 0)
                {
                    toPos = transform.position;

                    // set breaktime
                    timer = Time.time + Random.Range(minPauseTime, maxPauseTime);
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
            if (enemyType == EnemyType.Swordsman && !anim.IsAttacking)
            {
                StartCoroutine(anim.Swinging());

                toPos = transform.position;
                target = null;
            }
            else
            {
                // follow target
                toPos = target.position;
            }
        }
        else if (Vector3.Distance(transform.position, target.position) > (targetTrigger as SphereCollider).radius + 2)
        {
            // lose target
            // hunters lose targets only after shooting
            if (enemyType != EnemyType.Hunter)
            {
                target = null;
                timer = 1;
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
            if (stats.InvincibleTimer > Time.time)
            {
                transform.Translate(moveDirection.normalized
                    * 5
                    * Time.fixedDeltaTime,
                    Space.World);
            }
            else
            {
                // check if hunter has target
                if (!(enemyType == EnemyType.Hunter && target != null) && !anim.IsAttacking)
                {
                    transform.Translate(moveDirection.normalized
                        * ((target == null) ? speed : followSpeed)
                        * Time.fixedDeltaTime,
                        Space.World);
                }

                // turn the enemy
                transform.rotation = (Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation(moveDirection.normalized),
                    turnSpeed * Time.fixedDeltaTime));
            }
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
        anim.Die();

        yield return new WaitForSeconds(1f);

        // explode
        explosion.transform.position = transform.position + Vector3.up * .5f;
        explosion.Emit(1);
        explosion.GetComponent<AudioSource>().Play();

        // drop item
        CollectibleType collectibleType = CollectibleType.Heart;
        if (enemyType == EnemyType.Hunter)
        {
            collectibleType = CollectibleType.Bow;
        }
        collectible.Drop(transform.position, collectibleType);

        DisableEnemy();
    }

    // Called when dead or lifetime is zero
    public void DisableEnemy()
    {
        target = null;
        Lifetime = 0;

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

            if (player.PlayerActive && target == null && stats.InvincibleTimer < Time.time && !anim.IsAttacking)
            {
                target = other.transform;

                if (enemyType == EnemyType.Hunter && !anim.IsAttacking)
                {
                    StartCoroutine(AimShoot());
                }
            }
            else if (enemyType != EnemyType.Hunter)
            {
                target = null;
            }
        }
    }

    // Getting hit.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerWeapon")
            && stats.InvincibleTimer < Time.time)
        {
            // push back
            target = null;
            toPos = transform.position + (transform.position - other.transform.position).normalized * 10;
            toPos = new Vector3(toPos.x, 0, toPos.z);

            // set next toPos
            timer = 1;

            stats.GetHit();

            // check health
            if (stats.Health <= 0)
            {
                StopAllCoroutines();
                StartCoroutine(Death());

                bat.GetComponent<Collider>().enabled = false;

                // lightning effect
                lightning.transform.position = transform.position + Vector3.up * .5f;
                lightning.Emit(1);
                lightning.GetComponent<AudioSource>().Play();
            }
            else
            {
                // hit effect
                Vector3 hitPos = (transform.position + other.transform.position) / 2;
                hit.transform.position = hitPos + Vector3.up * .5f;
                hit.Emit(1);
                hit.GetComponent<AudioSource>().Play();
            }
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